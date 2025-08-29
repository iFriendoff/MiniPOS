using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MiniPOS
{
    public class Inventory
    {
        public Dictionary<string, Item> Items = new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase);
        public string DataDir { get; private set; }
        public string InventoryPath => Path.Combine(DataDir, "inventory.csv");
        public string SalesPath => Path.Combine(DataDir, "sales.csv");

        public Inventory(string dataDir)
        {
            DataDir = dataDir;
            Directory.CreateDirectory(DataDir);
            Load();
        }

        public void Load()
        {
            Items.Clear();
            if (!File.Exists(InventoryPath))
            {
                File.WriteAllText(InventoryPath,
                    "barcode,name,price,stock\r\n" +
                    "4601234567890,Шоколад молочный,350,25\r\n" +
                    "4820001234567,Кофе растворимый 100г,1800,12\r\n" +
                    "2000000000012,Пакет майка,20,100\r\n",
                    Encoding.UTF8);
            }

            var lines = File.ReadAllLines(InventoryPath, Encoding.UTF8);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = SplitCsv(line);
                if (parts.Length < 4) continue;

                var item = new Item
                {
                    Barcode = parts[0].Trim(),
                    Name = parts[1].Trim(),
                    Price = ParseDec(parts[2]),
                    Stock = ParseInt(parts[3])
                };
                Items[item.Barcode] = item;
            }

            if (!File.Exists(SalesPath))
                File.WriteAllText(SalesPath, "datetime,barcode,qty,price,total\r\n", Encoding.UTF8);
        }

        public bool TryGet(string barcode, out Item item) => Items.TryGetValue(barcode, out item);

        public void UpsertItem(Item item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Barcode)) return;
            Items[item.Barcode] = item;
            SaveInventory();
        }

        public void SaveInventory()
        {
            var sb = new StringBuilder();
            sb.AppendLine("barcode,name,price,stock");
            foreach (var it in Items.Values)
            {
                sb.AppendLine($"{Esc(it.Barcode)},{Esc(it.Name)},{it.Price.ToString(CultureInfo.InvariantCulture)},{it.Stock}");
            }
            File.WriteAllText(InventoryPath, sb.ToString(), Encoding.UTF8);
        }

        public void AppendSale(IEnumerable<SaleLine> lines)
        {
            var sb = new StringBuilder();
            foreach (var l in lines)
                sb.AppendLine($"{l.Timestamp:yyyy-MM-dd HH:mm:ss},{Esc(l.Barcode)},{l.Qty},{l.Price.ToString(CultureInfo.InvariantCulture)},{l.Total.ToString(CultureInfo.InvariantCulture)}");
            File.AppendAllText(SalesPath, sb.ToString(), Encoding.UTF8);
        }

        static string[] SplitCsv(string line)
        {
            var list = new List<string>();
            bool q = false; var cur = new StringBuilder();
            foreach (var ch in line)
            {
                if (ch == '"') { q = !q; continue; }
                if (ch == ',' && !q) { list.Add(cur.ToString()); cur.Clear(); }
                else cur.Append(ch);
            }
            list.Add(cur.ToString());
            return list.ToArray();
        }
        static string Esc(string s) => (s ?? "").Contains(",") || (s ?? "").Contains("\"") ? "\"" + (s ?? "").Replace("\"", "\"\"") + "\"" : (s ?? "");
        static decimal ParseDec(string s)
        {
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("ru-RU"), out d)) return d;
            return 0m;
        }
        static int ParseInt(string s) { int.TryParse(s, out var i); return i; }
    }
}
