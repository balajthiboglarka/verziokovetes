using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
namespace HotcakesShopManager.Views
{
    public class ProductListControl : UserControl
    {
        private DataGridView _dgvProducts;
        private Button btnRefresh;
        public event EventHandler RefreshRequested;
        public ProductListControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            var topPanel = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 10) };
            var titleLabel = new Label
            {
                Text = "Termékek listája",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 75, 155),
                Dock = DockStyle.Left,
                Padding = new Padding(10, 15, 10, 15),
                AutoSize = true
            };
            btnRefresh = new Button
            {
                Text = "🔄 Frissítés",
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(240, 240, 240),
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 5, 10, 5),
                Location = new Point(0, 15),
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
            topPanel.Controls.Add(titleLabel);
            topPanel.Controls.Add(btnRefresh);
            topPanel.Resize += (s, e) => {
                btnRefresh.Left = topPanel.Width - btnRefresh.Width - 20;
            };
            var borderLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };
            _dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                GridColor = Color.LightGray,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            _dgvProducts.DefaultCellStyle.Padding = new Padding(5);
            _dgvProducts.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 75, 155),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(5)
            };
            var paddingPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            paddingPanel.Controls.Add(_dgvProducts);
            this.Controls.Add(paddingPanel);
            this.Controls.Add(borderLine);
            this.Controls.Add(topPanel);
        }
        public void LoadData(List<ProductViewModel> products)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => LoadData(products)));
                return;
            }
            _dgvProducts.DataSource = null;
            _dgvProducts.DataSource = products;
            if (_dgvProducts.Columns["Bvin"] != null) _dgvProducts.Columns["Bvin"].Visible = false;
            if (_dgvProducts.Columns["ImageUrl"] != null) _dgvProducts.Columns["ImageUrl"].Visible = false;
            if (_dgvProducts.Columns["DisplayName"] != null) _dgvProducts.Columns["DisplayName"].Visible = false;
            if (_dgvProducts.Columns["Sku"] != null) _dgvProducts.Columns["Sku"].HeaderText = "Cikkszám";
            if (_dgvProducts.Columns["ProductName"] != null) _dgvProducts.Columns["ProductName"].HeaderText = "Terméknév";
            if (_dgvProducts.Columns["Category"] != null) _dgvProducts.Columns["Category"].HeaderText = "Kategória";
            if (_dgvProducts.Columns["Price"] != null) _dgvProducts.Columns["Price"].HeaderText = "Ár (Ft)";
            if (_dgvProducts.Columns["Quantity"] != null) _dgvProducts.Columns["Quantity"].HeaderText = "Készlet";
        }
    }
}
