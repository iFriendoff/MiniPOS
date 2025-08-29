using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniPOS
{
    public class PaymentForm : Form
    {
        TextBox txtCash;
        public decimal Cash { get; private set; }

        public PaymentForm(decimal total)
        {
            Text = "Оплата";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(600, 300);
            Font = new Font("Segoe UI", 20f);

            var lblTotal = new Label { Text = "К оплате: " + total.ToString("0.##"), Left = 10, Top = 20, AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
            var lblCash = new Label { Text = "Внести:", Left = 10, Top = 100, AutoSize = true };
            txtCash = new TextBox { Left = 150, Top = 90, Width = 200, Text = total.ToString("0.##"), Font = this.Font };

            var btnExact = new Button { Text = "Без сдачи", Left = 10, Top = 200, Width = 160, Height = 60, BackColor = Color.DodgerBlue, ForeColor = Color.White };
            btnExact.Click += (s, e) => { Cash = total; DialogResult = DialogResult.OK; };

            var btnOk = new Button { Text = "Оплатить", Left = 190, Top = 200, Width = 160, Height = 60, BackColor = Color.SeaGreen, ForeColor = Color.White, DialogResult = DialogResult.OK };
            btnOk.Click += (s, e) =>
            {
                if (!decimal.TryParse(txtCash.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var c))
                { MessageBox.Show("Некорректная сумма."); this.DialogResult = DialogResult.None; return; }
                Cash = c;
            };

            var btnCancel = new Button { Text = "Отмена", Left = 370, Top = 200, Width = 160, Height = 60, BackColor = Color.Firebrick, ForeColor = Color.White, DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] { lblTotal, lblCash, txtCash, btnExact, btnOk, btnCancel });
        }
    }
}
