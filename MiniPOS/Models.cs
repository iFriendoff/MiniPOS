using System;

namespace MiniPOS
{
    public class Item
    {
        public string Barcode { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    // В корзине держим Selected, чтобы биндинг чекбокса работал
    public class CartLine
    {
        public bool Selected { get; set; }        // чекбокс в таблице
        public string Barcode { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal => Price * Qty;
    }

    public class SaleLine
    {
        public DateTime Timestamp { get; set; }
        public string Barcode { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Price * Qty;
    }
}
