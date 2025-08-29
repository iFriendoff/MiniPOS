using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniPOS
{
    public class UniversalProductForm : Form
    {
        TextBox txtPrice, txtQty;
        public decimal Price { get; private set; }
        public int Qty { get; private set; }

        public UniversalProductForm()
        {
            Text = "Универсальный товар";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ClientSize = new Size(420, 220);
            Font = new Font("Segoe UI", 14f);

            int y = 20;

            Controls.Add(new Label { Text = "Цена:", Left = 10, Top = y + 8, AutoSize = true });
            txtPrice = new TextBox { Left = 120, Top = y, Width = 150, Font = this.Font, Text = "0" };
            Controls.Add(txtPrice);
            y += 60;

            Controls.Add(new Label { Text = "Кол-во:", Left = 10, Top = y + 8, AutoSize = true });
            txtQty = new TextBox { Left = 120, Top = y, Width = 150, Font = this.Font, Text = "1" };
            Controls.Add(txtQty);
            y += 70;

            var btnOk = new Button { Text = "Добавить", Left = 190, Top = y, Width = 120, Height = 45, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", Left = 320, Top = y, Width = 90, Height = 45, DialogResult = DialogResult.Cancel };
            Controls.AddRange(new Control[] { btnOk, btnCancel });

            btnOk.Click += (s, e) =>
            {
                if (!decimal.TryParse(txtPrice.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p))
                { MessageBox.Show("Некорректная цена."); this.DialogResult = DialogResult.None; return; }
                if (!int.TryParse(txtQty.Text, out var q) || q <= 0)
                { MessageBox.Show("Некорректное количество."); this.DialogResult = DialogResult.None; return; }

                Price = p;
                Qty = q;
            };

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
