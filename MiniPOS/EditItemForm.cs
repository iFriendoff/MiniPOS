using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniPOS
{
    public class EditItemForm : Form
    {
        TextBox txtName, txtPrice;
        public string ItemName { get; private set; }
        public decimal Price { get; private set; }

        public EditItemForm(string name, decimal price)
        {
            Text = "Изменить товар";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ClientSize = new Size(380, 160);

            var lbl1 = new Label { Text = "Название:", Left = 10, Top = 20, AutoSize = true };
            txtName = new TextBox { Left = 100, Top = 16, Width = 260, Text = name };

            var lbl2 = new Label { Text = "Цена:", Left = 10, Top = 60, AutoSize = true };
            txtPrice = new TextBox { Left = 100, Top = 56, Width = 120, Text = price.ToString("0.##") };

            var btnOk = new Button { Text = "OK", Left = 200, Top = 100, Width = 75, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", Left = 285, Top = 100, Width = 75, DialogResult = DialogResult.Cancel };

            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Введите название."); this.DialogResult = DialogResult.None; return; }
                if (!decimal.TryParse(txtPrice.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p))
                { MessageBox.Show("Некорректная цена."); this.DialogResult = DialogResult.None; return; }
                ItemName = txtName.Text.Trim();
                Price = p;
            };

            Controls.AddRange(new Control[] { lbl1, txtName, lbl2, txtPrice, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
