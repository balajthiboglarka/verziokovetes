using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HotcakesShopManager.Views;
namespace HotcakesShopManager
{
    public class MainForm : Form
    {
        private const string API_URL = "http://40.67.241.74/";
        private const string API_KEY = "1-98be8ddb-b904-4c00-8cae-9351ca54f539";
        private HotcakesApiService _apiService;
        private List<ProductViewModel> _products;
        private List<CategoryViewModel> _categories;
        private List<OrderViewModel> _orders;
        private Panel pnlSidebar;
        private Panel pnlContent;
        private Label lblStatus;
        private Button btnProductList;
        private Button btnBulkAssign;
        private Button btnImageQuickview;
        private Button btnInventoryReports;
        private ProductListControl _productListControl;
        private BulkAssignmentControl _bulkAssignmentControl;
        private ImageQuickviewControl _imageQuickviewControl;
        private InventoryReportsControl _inventoryReportsControl;
        public MainForm()
        {
            _apiService = new HotcakesApiService(API_URL, API_KEY);
            InitializeComponent();
            this.Load += MainForm_Load;
        }
        private void InitializeComponent()
        {
            this.Text = "Hotcakes Webshop Manager";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.FromArgb(240, 240, 240) };
            var lblNavHeader = new Label
            {
                Text = "Navigáció",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 75, 155),
                Dock = DockStyle.Top,
                Padding = new Padding(15, 15, 10, 15),
                AutoSize = true
            };
            btnProductList = CreateMenuButton("Termékek listája");
            btnBulkAssign = CreateMenuButton("Tömeges sorolás");
            btnImageQuickview = CreateMenuButton("Kép gyorsnézet");
            btnInventoryReports = CreateMenuButton("Leltár jelentések");
            btnProductList.Click += (s, e) => NavigateTo(_productListControl, btnProductList);
            btnBulkAssign.Click += (s, e) => NavigateTo(_bulkAssignmentControl, btnBulkAssign);
            btnImageQuickview.Click += (s, e) => NavigateTo(_imageQuickviewControl, btnImageQuickview);
            btnInventoryReports.Click += (s, e) => NavigateTo(_inventoryReportsControl, btnInventoryReports);
            pnlSidebar.Controls.Add(btnInventoryReports);
            pnlSidebar.Controls.Add(btnImageQuickview);
            pnlSidebar.Controls.Add(btnBulkAssign);
            pnlSidebar.Controls.Add(btnProductList);
            pnlSidebar.Controls.Add(lblNavHeader);
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(240, 240, 240) };
            lblStatus = new Label { Text = "Ready.", Dock = DockStyle.Fill, ForeColor = Color.DarkGray, Padding = new Padding(5) };
            pnlStatus.Controls.Add(lblStatus);
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlStatus);
            _productListControl = new ProductListControl();
            _productListControl.RefreshRequested += async (s, e) => await LoadDataAsync();
            _bulkAssignmentControl = new BulkAssignmentControl(_apiService);
            _bulkAssignmentControl.AssignmentChanged += async (s, e) => await LoadDataAsync();
            _imageQuickviewControl = new ImageQuickviewControl();
            _inventoryReportsControl = new InventoryReportsControl();
        }
        private Button CreateMenuButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 15, 0, 15)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
        private async void MainForm_Load(object sender, EventArgs e)
        {
            NavigateTo(_productListControl, btnProductList);
            await LoadDataAsync();
        }
        private async Task LoadDataAsync()
        {
            lblStatus.Text = "Loading data from Hotcakes API...";
            lblStatus.ForeColor = Color.Blue;
            EnableMenu(false);
            try
            {
                await Task.Run(() =>
                {
                    _products = _apiService.GetProductData();
                    _categories = _apiService.GetCategories();
                    _orders = _apiService.GetOrders();
                });
                _productListControl.LoadData(_products);
                _bulkAssignmentControl.LoadData(_products, _categories);
                _imageQuickviewControl.LoadData(_products);
                _inventoryReportsControl.LoadData(_products, _orders);
                lblStatus.Text = $"Data loaded successfully. Products: {_products.Count}, Categories: {_categories.Count}, Orders: {_orders?.Count ?? 0}";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error loading data.";
                lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                EnableMenu(true);
            }
        }
        private void EnableMenu(bool enable)
        {
            btnProductList.Enabled = enable;
            btnBulkAssign.Enabled = enable;
            btnImageQuickview.Enabled = enable;
            btnInventoryReports.Enabled = enable;
        }
        private void NavigateTo(UserControl control, Button clickedButton)
        {
            btnProductList.BackColor = Color.FromArgb(240, 240, 240);
            btnProductList.ForeColor = Color.Black;
            btnBulkAssign.BackColor = Color.FromArgb(240, 240, 240);
            btnBulkAssign.ForeColor = Color.Black;
            btnImageQuickview.BackColor = Color.FromArgb(240, 240, 240);
            btnImageQuickview.ForeColor = Color.Black;
            btnInventoryReports.BackColor = Color.FromArgb(240, 240, 240);
            btnInventoryReports.ForeColor = Color.Black;
            clickedButton.BackColor = Color.FromArgb(0, 75, 155);
            clickedButton.ForeColor = Color.White;
            pnlContent.Controls.Clear();
            control.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(control);
        }
    }
}
