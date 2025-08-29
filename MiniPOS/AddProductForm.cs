using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniPOS
{
    public class AddProductForm : Form
    {
        TextBox txtBarcode, txtName, txtPrice, txtStock;
        public string Barcode { get; private set; }
        public string ItemName { get; private set; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }

        public AddProductForm(string presetBarcode = "")
        {
            Text = "Добавить новый товар";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ClientSize = new Size(480, 300);
            Font = new Font("Segoe UI", 14f);

            int y = 20;

            Controls.Add(new Label { Text = "Штрих-код:", Left = 10, Top = y + 8, AutoSize = true });
            txtBarcode = new TextBox { Left = 150, Top = y, Width = 300, Font = this.Font, Text = presetBarcode };
            Controls.Add(txtBarcode);
            y += 50;

            Controls.Add(new Label { Text = "Название:", Left = 10, Top = y + 8, AutoSize = true });
            txtName = new TextBox { Left = 150, Top = y, Width = 300, Font = this.Font };
            Controls.Add(txtName);
            y += 50;

            Controls.Add(new Label { Text = "Цена:", Left = 10, Top = y + 8, AutoSize = true });
            txtPrice = new TextBox { Left = 150, Top = y, Width = 150, Font = this.Font, Text = "0" };
            Controls.Add(txtPrice);
            y += 50;

            Controls.Add(new Label { Text = "Остаток:", Left = 10, Top = y + 8, AutoSize = true });
            txtStock = new TextBox { Left = 150, Top = y, Width = 150, Font = this.Font, Text = "1" };
            Controls.Add(txtStock);
            y += 70;

            var btnOk = new Button { Text = "Добавить", Left = 240, Top = y, Width = 120, Height = 45, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", Left = 370, Top = y, Width = 120, Height = 45, DialogResult = DialogResult.Cancel };
            Controls.AddRange(new Control[] { btnOk, btnCancel });

            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtBarcode.Text)) { MessageBox.Show("Введите штрих-код."); this.DialogResult = DialogResult.None; return; }
                if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Введите название."); this.DialogResult = DialogResult.None; return; }
                if (!decimal.TryParse(txtPrice.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p))
                { MessageBox.Show("Некорректная цена."); this.DialogResult = DialogResult.None; return; }
                if (!int.TryParse(txtStock.Text, out var st) || st < 0)
                { MessageBox.Show("Некорректный остаток."); this.DialogResult = DialogResult.None; return; }

                Barcode = txtBarcode.Text.Trim();
                ItemName = txtName.Text.Trim();
                Price = p;
                Stock = st;
            };

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
