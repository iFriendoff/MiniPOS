using System;
using System.Collections.Generic;
using System.ComponentModel; // BindingList
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MiniPOS
{
    public class MainForm : Form
    {
        TextBox txtScan;
        DataGridView grid;
        Label lblTotalVal, lblReceivedVal, lblChangeVal;
        Button btnAddUniversal, btnEdit, btnDelete, btnCheckout, btnQtyPlus, btnQtyMinus;
        CheckBox headerCheck;

        Inventory inv;
        BindingList<CartLine> cart = new BindingList<CartLine>();

        public MainForm()
        {
            Text = "MiniPOS — Продажа";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized; // старт во весь экран
            MinimumSize = new Size(1024, 600);
            Font = new Font("Segoe UI", 16f, FontStyle.Regular);

            BuildUi();

            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            inv = new Inventory(dataDir);

            BindCart();
            txtScan.Focus();
            UpdateEditButtonState();
        }

        void BuildUi()
        {
            // верх
            var top = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(12, 10, 12, 10) };
            var lbl = new Label { Text = "Сканируйте / введите штрих-код и Enter", AutoSize = true, Left = 8, Top = 4 };
            txtScan = new TextBox { Left = 8, Top = 28, Width = 600, Height = 38, Font = this.Font };
            txtScan.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    HandleScan(txtScan.Text.Trim());
                    txtScan.Clear();
                    e.SuppressKeyPress = true;
                }
            };
            top.Controls.AddRange(new Control[] { lbl, txtScan });
            Controls.Add(top);

            // таблица
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                Font = this.Font
            };
            grid.RowTemplate.Height = 50;
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colSel", HeaderText = "", DataPropertyName = "Selected", Width = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBarcode", HeaderText = "ШК", DataPropertyName = "Barcode", Width = 170 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "НАЗВАНИЕ", DataPropertyName = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "ЦЕНА", DataPropertyName = "Price", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##" } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQty", HeaderText = "КОЛ-ВО", DataPropertyName = "Qty", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSum", HeaderText = "СУММА", DataPropertyName = "Subtotal", Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##" } });

            headerCheck = new CheckBox { Size = new Size(18, 18) };
            headerCheck.CheckedChanged += (s, e) =>
            {
                foreach (var line in cart) line.Selected = headerCheck.Checked;
                grid.Refresh();
                UpdateEditButtonState();
            };
            grid.Controls.Add(headerCheck);
            grid.Paint += (s, e) => PositionHeaderCheckBox();

            grid.SelectionChanged += (s, e) => UpdateEditButtonState();
            grid.CellValueChanged += (s, e) =>
            {
                if (e.ColumnIndex == grid.Columns["colSel"].Index)
                    UpdateEditButtonState();
            };

            Controls.Add(grid);
            grid.RowPrePaint += (s, e) =>
            {
                var row = grid.Rows[e.RowIndex];
                var name = Convert.ToString(row.Cells["colName"].Value);
                if (name == "НЕИЗВ")
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                else
                    row.DefaultCellStyle.BackColor = Color.White;
            };


            // низ
            Controls.Add(BuildBottomPanel());
        }
        Panel BuildBottomPanel()
        {
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 220, Padding = new Padding(12) };

            // карточка итогов (слева)
            var card = new Panel { Left = 12, Top = 12, Width = 360, Height = 180, BackColor = Color.Gainsboro, Padding = new Padding(12) };
            var fBold = new Font(Font, FontStyle.Bold);
            lblTotalVal = new Label { Text = "0", AutoSize = true, Font = fBold, Left = 190, Top = 0 };
            lblReceivedVal = new Label { Text = "0", AutoSize = true, Left = 190, Top = 70 };
            lblChangeVal = new Label { Text = "0", AutoSize = true, Left = 190, Top = 130 };
            card.Controls.AddRange(new Control[]
            {
        new Label { Text = "ИТОГО",   Top = 0,   Left = 10, Font = fBold },
        lblTotalVal,
        new Label { Text = "ПОЛУЧЕНО",Top = 70,  Left = 10 },
        lblReceivedVal,
        new Label { Text = "СДАЧА",   Top = 130, Left = 10 },
        lblChangeVal
            });
            bottom.Controls.Add(card);

            // кнопки
            btnQtyPlus = new Button { Text = "+", Width = 90, Height = 70 };
            btnQtyMinus = new Button { Text = "−", Width = 90, Height = 70 };
            btnEdit = new Button { Text = "ИЗМЕНИТЬ", Width = 170, Height = 70 };
            btnDelete = new Button { Text = "УДАЛИТЬ", Width = 170, Height = 70, BackColor = Color.Firebrick, ForeColor = Color.White };
            btnAddUniversal = new Button { Text = "УНИВЕРСАЛЬНЫЙ", Width = 230, Height = 70 };
            btnCheckout = new Button { Text = "ОПЛАТА", Width = 300, Height = 160, BackColor = Color.SeaGreen, ForeColor = Color.White, Font = fBold };

            // стиль: чёрная обводка + лёгкая заливка для остальных
            Action<Button> style = b =>
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 2;
                b.FlatAppearance.BorderColor = Color.Black;
                if (b.BackColor == Color.Empty) { b.BackColor = Color.FromArgb(230, 230, 230); b.ForeColor = Color.Black; }
                b.Font = this.Font;
            };
            foreach (var b in new[] { btnQtyPlus, btnQtyMinus, btnEdit, btnDelete, btnAddUniversal, btnCheckout }) style(b);

            // раскладка в ДВА ряда; «Оплата» прижата к правому краю
            bottom.Resize += (s, e) =>
            {
                int pad = 12;
                int rowH = 70;                   // высота обычной кнопки
                int yTop = bottom.ClientSize.Height - rowH * 2 - pad * 2; // верхний ряд
                int yBot = yTop + rowH + pad;    // нижний ряд

                // правая большая кнопка
                btnCheckout.Left = bottom.ClientSize.Width - btnCheckout.Width - pad;
                btnCheckout.Top = yTop;

                // столбик +/− вплотную к «Оплата»
                int colLeft = btnCheckout.Left - pad - btnQtyPlus.Width;
                btnQtyPlus.Left = colLeft; btnQtyPlus.Top = yTop;
                btnQtyMinus.Left = colLeft; btnQtyMinus.Top = yBot;

                // 1-я строка слева от столбика: УНИВЕРСАЛЬНЫЙ
                btnAddUniversal.Left = colLeft - pad - btnAddUniversal.Width;
                btnAddUniversal.Top = yTop;

                // 2-я строка слева от столбика: ИЗМЕНИТЬ, УДАЛИТЬ (слева→вправо)
                // ставим их вплотную к столбику +/- (без щели)
                btnEdit.Top = yBot;
                btnDelete.Top = yBot;

                btnDelete.Left = colLeft - pad - btnDelete.Width;
                btnEdit.Left = btnDelete.Left - pad - btnEdit.Width;

                // добавляем в панель (однократно)
                foreach (var b in new[] { btnCheckout, btnQtyPlus, btnQtyMinus, btnAddUniversal, btnEdit, btnDelete })
                    if (!bottom.Controls.Contains(b)) bottom.Controls.Add(b);
            };
            bottom.PerformLayout();

            // обработчики
            btnAddUniversal.Click += (s, e) => AddUniversalProduct();
            btnEdit.Click += (s, e) => EditSelected();
            btnDelete.Click += (s, e) => DeleteSelected();
            btnQtyPlus.Click += (s, e) => ChangeQty(+1);
            btnQtyMinus.Click += (s, e) => ChangeQty(-1);
            btnCheckout.Click += (s, e) => Checkout();

            return bottom;
        }

        void PositionHeaderCheckBox()
        {
            var rect = grid.GetCellDisplayRectangle(grid.Columns["colSel"].Index, -1, true);
            headerCheck.Location = new Point(rect.X + (rect.Width - headerCheck.Width) / 2,
                                             rect.Y + (rect.Height - headerCheck.Height) / 2);
        }

        void BindCart()
        {
            grid.DataSource = null;
            grid.DataSource = cart;
            lblTotalVal.Text = cart.Sum(x => x.Subtotal).ToString("0.##");
        }

        // ====== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ======
        string CurrentBarcode() => grid.CurrentRow != null ? Convert.ToString(grid.CurrentRow.Cells["colBarcode"].Value) : null;
        bool AnyChecked() => cart.Any(c => c.Selected);

        void UpdateEditButtonState()
        {
            bool oneSelected = grid.CurrentRow != null && grid.SelectedRows.Count == 1;
            btnEdit.Enabled = oneSelected && !AnyChecked();
        }
        void HandleScan(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;

            if (!inv.TryGet(code, out var item))
            {
                // вместо вызова AddProductForm — сразу добавляем НЕИЗВ
                var bc = "NEW-" + code;
                cart.Add(new CartLine
                {
                    Barcode = bc,
                    Name = "НЕИЗВ",
                    Price = 0,
                    Qty = 1
                });
                BindCart();
                return;
            }
            AddToCart(item.Barcode, 1);
        }


        void AddToCart(string code, int qty)
        {
            if (!inv.TryGet(code, out var item)) return;
            var line = cart.FirstOrDefault(c => c.Barcode == code);
            if (line == null)
                cart.Add(new CartLine { Barcode = item.Barcode, Name = item.Name, Price = item.Price, Qty = qty });
            else
                line.Qty += qty;
            BindCart();
        }

        void DeleteSelected()
        {
            if (AnyChecked())
            {
                var toDel = cart.Where(c => c.Selected).ToList();
                foreach (var c in toDel) cart.Remove(c);
            }
            else
            {
                var bc = CurrentBarcode();
                if (string.IsNullOrEmpty(bc)) return;
                var line = cart.FirstOrDefault(x => x.Barcode == bc);
                if (line != null) cart.Remove(line);
            }
            headerCheck.Checked = false;
            BindCart();
            UpdateEditButtonState();
        }

        void ChangeQty(int delta)
        {
            var bc = CurrentBarcode();
            if (string.IsNullOrEmpty(bc)) return;
            var line = cart.FirstOrDefault(x => x.Barcode == bc);
            if (line == null) return;
            line.Qty += delta;
            if (line.Qty <= 0) line.Qty = 1;
            BindCart();
        }

        void EditSelected()
        {
            if (AnyChecked()) return;
            var bc = CurrentBarcode();
            if (string.IsNullOrEmpty(bc)) return;
            var line = cart.FirstOrDefault(x => x.Barcode == bc);
            if (line == null) return;

            using (var dlg = new EditItemForm(line.Name, line.Price) { Font = this.Font })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    line.Name = dlg.ItemName;
                    line.Price = dlg.Price;
                    if (!bc.StartsWith("UNIV-") && inv.Items.TryGetValue(bc, out var it))
                    {
                        it.Name = dlg.ItemName;
                        it.Price = dlg.Price;
                        inv.UpsertItem(it);
                    }
                    BindCart();
                }
            }
        }

        void AddUniversalProduct()
        {
            using (var dlg = new UniversalProductForm() { Font = this.Font })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var bc = "UNIV-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    cart.Add(new CartLine { Barcode = bc, Name = "Универсальный товар", Price = dlg.Price, Qty = dlg.Qty });
                    BindCart();
                }
            }
        }

        void Checkout()
        {
            if (cart.Count == 0) { MessageBox.Show("Корзина пуста."); return; }
            var total = cart.Sum(x => x.Subtotal);
            using (var dlg = new PaymentForm(total) { Font = this.Font })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var cash = dlg.Cash;
                if (cash < total) { MessageBox.Show("Внесено меньше суммы."); return; }
                var change = cash - total;
                lblReceivedVal.Text = cash.ToString("0.##");
                lblChangeVal.Text = change.ToString("0.##");

                var ts = DateTime.Now;
                var lines = cart.Select(c => new SaleLine { Timestamp = ts, Barcode = c.Barcode, Qty = c.Qty, Price = c.Price }).ToList();
                inv.AppendSale(lines);
                foreach (var c in cart)
                {
                    if (!c.Barcode.StartsWith("UNIV-") && inv.Items.TryGetValue(c.Barcode, out var it))
                        it.Stock -= c.Qty;
                }
                inv.SaveInventory();

                var receiptDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "receipts");
                Directory.CreateDirectory(receiptDir);
                var fname = Path.Combine(receiptDir, ts.ToString("yyyyMMdd_HHmmss") + ".txt");
                File.WriteAllText(fname, BuildReceipt(ts, cart.ToList(), total, cash, change), Encoding.UTF8);

                MessageBox.Show("Оплачено. Сдача: " + change.ToString("0.##") + "\r\nЧек: " + fname, "Успех");
                cart.Clear();
                BindCart();
                txtScan.Focus();
                headerCheck.Checked = false;
                UpdateEditButtonState();
            }
        }

        string BuildReceipt(DateTime ts, IList<CartLine> lines, decimal total, decimal cash, decimal change)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MINI POS ===");
            sb.AppendLine("Дата: " + ts.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("----------------------------");
            foreach (var c in lines)
            {
                sb.AppendLine($"{c.Name}");
                sb.AppendLine($"{c.Barcode}  {c.Qty} x {c.Price:0.##} = {c.Subtotal:0.##}");
            }
            sb.AppendLine("----------------------------");
            sb.AppendLine($"ИТОГО: {total:0.##}");
            sb.AppendLine($"НАЛИЧНЫЕ: {cash:0.##}");
            sb.AppendLine($"СДАЧА: {change:0.##}");
            sb.AppendLine("Спасибо за покупку!");
            return sb.ToString();
        }
    }
}
