using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace HotcakesShopManager.Views
{
    public class InventoryReportsControl : UserControl
    {
        private RadioButton rbFullInventory;
        private RadioButton rbLowInventory;
        private RadioButton rbCategorySummary;
        private RadioButton rbSalesReport;
        private Button btnGenerate;
        private Button btnExport;
        private TextBox txtOutput;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Panel pnlDateFilter;
        private List<ProductViewModel> _products;
        private List<OrderViewModel> _orders;
        public InventoryReportsControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            var titleLabel = new Label
            {
                Text = "Leltár jelentések",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 75, 155),
                Dock = DockStyle.Top,
                Padding = new Padding(10, 15, 10, 15),
                AutoSize = true
            };
            var borderLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var pnlOptions = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(0, 0, 0, 15)
            };
            var lblOptions = new Label
            {
                Text = "Jelentés típusa",
                Dock = DockStyle.Top,
                BackColor = Color.LightGray,
                Padding = new Padding(5),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
            var flpRadioButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(15, 10, 0, 0),
                WrapContents = true
            };
            rbFullInventory = new RadioButton
            {
                Text = "Teljes készlet áttekintés",
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                Checked = true,
                Margin = new Padding(0, 0, 20, 0)
            };
            rbLowInventory = new RadioButton
            {
                Text = "Alacsony készletű termékek",
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 0, 20, 0)
            };
            rbCategorySummary = new RadioButton
            {
                Text = "Kategóriánkénti összesítés",
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 0, 20, 0)
            };
            rbSalesReport = new RadioButton
            {
                Text = "Értékesítési jelentés",
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 0, 20, 0)
            };
            rbSalesReport.CheckedChanged += (s, e) => pnlDateFilter.Visible = rbSalesReport.Checked;
            rbFullInventory.CheckedChanged += (s, e) => pnlDateFilter.Visible = rbSalesReport.Checked;
            rbLowInventory.CheckedChanged += (s, e) => pnlDateFilter.Visible = rbSalesReport.Checked;
            rbCategorySummary.CheckedChanged += (s, e) => pnlDateFilter.Visible = rbSalesReport.Checked;
            flpRadioButtons.Controls.Add(rbFullInventory);
            flpRadioButtons.Controls.Add(rbLowInventory);
            flpRadioButtons.Controls.Add(rbCategorySummary);
            flpRadioButtons.Controls.Add(rbSalesReport);
            pnlDateFilter = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(15, 10, 0, 10),
                BackColor = Color.FromArgb(245, 248, 255),
                Visible = false
            };
            var flpDates = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false
            };
            var lblFrom = new Label
            {
                Text = "Dátumtól:",
                AutoSize = true,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 75, 155),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 4, 10, 0)
            };
            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 13),
                Value = DateTime.Today.AddMonths(-1),
                Width = 220,
                Margin = new Padding(0, 0, 30, 0)
            };
            var lblTo = new Label
            {
                Text = "Dátumig:",
                AutoSize = true,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 75, 155),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 4, 10, 0)
            };
            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 13),
                Value = DateTime.Today,
                Width = 220
            };
            flpDates.Controls.Add(lblFrom);
            flpDates.Controls.Add(dtpFrom);
            flpDates.Controls.Add(lblTo);
            flpDates.Controls.Add(dtpTo);
            pnlDateFilter.Controls.Add(flpDates);
            pnlOptions.Controls.Add(pnlDateFilter);
            pnlOptions.Controls.Add(flpRadioButtons);
            pnlOptions.Controls.Add(lblOptions);
            var flpButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 10, 0, 10),
                WrapContents = false
            };
            btnGenerate = new Button
            {
                Text = "Jelentés létrehozása",
                BackColor = Color.FromArgb(0, 75, 155),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(15, 8, 15, 8),
                Margin = new Padding(0, 0, 10, 0)
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += BtnGenerate_Click;
            btnExport = new Button
            {
                Text = "Exportálás CSV-be",
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(15, 8, 15, 8),
                Margin = new Padding(0, 0, 0, 0)
            };
            btnExport.FlatAppearance.BorderColor = Color.LightGray;
            btnExport.Click += BtnExport_Click;
            flpButtons.Controls.Add(btnGenerate);
            flpButtons.Controls.Add(btnExport);
            txtOutput = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.FromArgb(245, 245, 245),
                WordWrap = false,
                Text = "\r\n\r\n\r\n\r\n\r\n        Válasszon jelentéstípust és kattintson a \"Jelentés létrehozása\" gombra"
            };
            var pnlOutput = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            pnlOutput.Controls.Add(txtOutput);
            mainPanel.Controls.Add(pnlOutput);
            mainPanel.Controls.Add(flpButtons);
            mainPanel.Controls.Add(pnlOptions);
            this.Controls.Add(mainPanel);
            this.Controls.Add(borderLine);
            this.Controls.Add(titleLabel);
        }
        public void LoadData(List<ProductViewModel> products, List<OrderViewModel> orders)
        {
            _products = products;
            _orders = orders;
        }
        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (_products == null) return;
            var sb = new StringBuilder();
            if (rbFullInventory.Checked)
            {
                sb.AppendLine("=== Teljes készlet áttekintés ===");
                sb.AppendLine($"{"Cikkszám",-15} {"Terméknév",-45} {"Kategória",-30} {"Készlet",8} {"Ár (Ft)",12}");
                sb.AppendLine(new string('-', 115));
                foreach (var p in _products.OrderBy(x => x.Sku))
                    sb.AppendLine($"{p.Sku,-15} {p.ProductName,-45} {p.Category,-30} {p.Quantity,8} {p.Price.ToString("N0"),12}");
                sb.AppendLine(new string('-', 115));
                sb.AppendLine($"Összesen: {_products.Count} termék, Összes készlet: {_products.Sum(p => p.Quantity)} db");
            }
            else if (rbLowInventory.Checked)
            {
                var low = _products.Where(x => x.Quantity <= 10).OrderBy(x => x.Quantity).ToList();
                sb.AppendLine("=== Alacsony készletű termékek (Készlet <= 10) ===");
                sb.AppendLine($"{"Cikkszám",-15} {"Terméknév",-45} {"Kategória",-30} {"Készlet",8}");
                sb.AppendLine(new string('-', 102));
                foreach (var p in low)
                    sb.AppendLine($"{p.Sku,-15} {p.ProductName,-45} {p.Category,-30} {p.Quantity,8}");
                sb.AppendLine(new string('-', 102));
                sb.AppendLine($"Érintett termékek: {low.Count} db");
            }
            else if (rbCategorySummary.Checked)
            {
                sb.AppendLine("=== Kategóriánkénti összesítés ===");
                sb.AppendLine($"{"Kategória",-40} {"Termékek száma",16} {"Összes készlet",16}");
                sb.AppendLine(new string('-', 76));
                var grouped = _products.GroupBy(p => string.IsNullOrEmpty(p.Category) ? "Besorolatlan" : p.Category);
                foreach (var g in grouped.OrderBy(x => x.Key))
                    sb.AppendLine($"{g.Key,-40} {g.Count(),16} {g.Sum(p => p.Quantity),16}");
            }
            else if (rbSalesReport.Checked)
            {
                var from = dtpFrom.Value.Date;
                var to = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);
                sb.AppendLine("=== Értékesítési jelentés ===");
                sb.AppendLine($"Időszak: {from:yyyy.MM.dd} – {dtpTo.Value.Date:yyyy.MM.dd}");
                sb.AppendLine();
                if (_orders == null || _orders.Count == 0)
                {
                    sb.AppendLine("Nincs rögzített rendelés a rendszerben.");
                }
                else
                {
                    var filtered = _orders.Where(o => o.TimeOfOrder >= from && o.TimeOfOrder <= to).ToList();
                    sb.AppendLine($"{"Azonosító",-40} {"Dátum",-22} {"Végösszeg (HUF)",16}");
                    sb.AppendLine(new string('-', 82));
                    decimal totalRevenue = 0;
                    foreach (var o in filtered)
                    {
                        sb.AppendLine($"{o.Bvin,-40} {o.TimeOfOrder.ToString("yyyy.MM.dd HH:mm"),-22} {o.TotalGrand.ToString("N0"),16}");
                        totalRevenue += o.TotalGrand;
                    }
                    sb.AppendLine(new string('-', 82));
                    sb.AppendLine($"Összes bevétel: {totalRevenue.ToString("N0")} HUF");
                    sb.AppendLine($"Összes rendelés: {filtered.Count} db");
                    if (_orders.Count != filtered.Count)
                        sb.AppendLine($"(A szűrő {_orders.Count - filtered.Count} rendelést kizárt)");
                }
            }
            txtOutput.Text = sb.ToString();
        }
        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (_products == null)
            {
                MessageBox.Show("Nincs mit exportálni!", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var sfd = new SaveFileDialog()
            {
                Filter = "CSV fájl|*.csv",
                Title = "Jelentés mentése CSV-be",
                DefaultExt = "csv",
                FileName = $"jelentes_{DateTime.Now:yyyyMMdd_HHmm}"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var csv = new StringBuilder();
                    if (rbFullInventory.Checked)
                    {
                        csv.AppendLine("Cikkszám;Terméknév;Kategória;Készlet;Ár (Ft)");
                        foreach (var p in _products.OrderBy(x => x.Sku))
                            csv.AppendLine($"\"{p.Sku}\";\"{p.ProductName}\";\"{p.Category}\";{p.Quantity};{p.Price}");
                    }
                    else if (rbLowInventory.Checked)
                    {
                        csv.AppendLine("Cikkszám;Terméknév;Kategória;Készlet;Ár (Ft)");
                        foreach (var p in _products.Where(x => x.Quantity <= 10).OrderBy(x => x.Quantity))
                            csv.AppendLine($"\"{p.Sku}\";\"{p.ProductName}\";\"{p.Category}\";{p.Quantity};{p.Price}");
                    }
                    else if (rbCategorySummary.Checked)
                    {
                        csv.AppendLine("Kategória;Termékek száma;Összes készlet");
                        var grouped = _products.GroupBy(p => string.IsNullOrEmpty(p.Category) ? "Besorolatlan" : p.Category);
                        foreach (var g in grouped.OrderBy(x => x.Key))
                            csv.AppendLine($"\"{g.Key}\";{g.Count()};{g.Sum(p => p.Quantity)}");
                    }
                    else if (rbSalesReport.Checked && _orders != null)
                    {
                        var from = dtpFrom.Value.Date;
                        var to = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);
                        var filtered = _orders.Where(o => o.TimeOfOrder >= from && o.TimeOfOrder <= to).ToList();
                        csv.AppendLine("Azonosító;Dátum;Végösszeg (HUF)");
                        foreach (var o in filtered)
                            csv.AppendLine($"\"{o.Bvin}\";\"{o.TimeOfOrder:yyyy.MM.dd HH:mm}\";{o.TotalGrand}");
                    }
                    System.IO.File.WriteAllText(sfd.FileName, csv.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("CSV sikeresen exportálva!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
