using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
namespace HotcakesShopManager.Views
{
    public class BulkAssignmentControl : UserControl
    {
        private CheckedListBox clbProducts;
        private CheckedListBox clbCategories;
        private Button btnAssign;
        private Button btnRemove;
        private HotcakesApiService _apiService;
        private List<ProductViewModel> _products;
        private List<CategoryViewModel> _categories;
        public event EventHandler AssignmentChanged;
        public BulkAssignmentControl(HotcakesApiService apiService)
        {
            _apiService = apiService;
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            var titleLabel = new Label
            {
                Text = "Tömeges sorolás",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 75, 155),
                Dock = DockStyle.Top,
                Padding = new Padding(10, 15, 10, 15),
                AutoSize = true
            };
            var borderLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(20)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            var pnlProducts = new Panel { Dock = DockStyle.Fill };
            var lblProd = new Label { Text = "Kiválasztott termékek", Dock = DockStyle.Top, BackColor = Color.LightGray, Padding = new Padding(5), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            clbProducts = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, Font = new Font("Segoe UI", 10), IntegralHeight = false };
            pnlProducts.Controls.Add(clbProducts);
            pnlProducts.Controls.Add(lblProd);
            pnlProducts.BorderStyle = BorderStyle.FixedSingle;
            var pnlCategories = new Panel { Dock = DockStyle.Fill };
            var lblCat = new Label { Text = "Kategóriák", Dock = DockStyle.Top, BackColor = Color.LightGray, Padding = new Padding(5), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true };
            clbCategories = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, Font = new Font("Segoe UI", 10), IntegralHeight = false };
            pnlCategories.Controls.Add(clbCategories);
            pnlCategories.Controls.Add(lblCat);
            pnlCategories.BorderStyle = BorderStyle.FixedSingle;
            var pnlButtons = new Panel { Dock = DockStyle.Fill };
            btnAssign = new Button { 
                Text = "Hozzárendelés", 
                BackColor = Color.FromArgb(0, 75, 155), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(15, 8, 15, 8)
            };
            btnAssign.Anchor = AnchorStyles.None;
            btnAssign.Click += BtnAssign_Click;
            btnRemove = new Button { 
                Text = "Eltávolítás", 
                BackColor = Color.FromArgb(0, 75, 155), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(15, 8, 15, 8)
            };
            btnRemove.Anchor = AnchorStyles.None;
            btnRemove.Click += BtnRemove_Click;
            pnlButtons.Controls.Add(btnAssign);
            pnlButtons.Controls.Add(btnRemove);
            pnlButtons.Resize += (s, e) => {
                btnAssign.Left = (pnlButtons.Width - btnAssign.Width) / 2;
                btnRemove.Left = (pnlButtons.Width - btnRemove.Width) / 2;
                btnAssign.Top = pnlButtons.Height / 2 - 50;
                btnRemove.Top = pnlButtons.Height / 2 + 10;
            };
            mainPanel.Controls.Add(pnlProducts, 0, 0);
            mainPanel.Controls.Add(pnlButtons, 1, 0);
            mainPanel.Controls.Add(pnlCategories, 2, 0);
            this.Controls.Add(mainPanel);
            this.Controls.Add(borderLine);
            this.Controls.Add(titleLabel);
        }
        public void LoadData(List<ProductViewModel> products, List<CategoryViewModel> categories)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => LoadData(products, categories)));
                return;
            }
            _products = products;
            _categories = categories;
            clbProducts.Items.Clear();
            if (products != null)
            {
                foreach (var p in products)
                {
                    clbProducts.Items.Add(new ProductItem { Bvin = p.Bvin, DisplayName = $"{p.ProductName} ({p.Sku})" });
                }
            }
            clbCategories.Items.Clear();
            if (categories != null)
            {
                foreach (var c in categories)
                {
                    clbCategories.Items.Add(new CategoryItem { Bvin = c.Bvin, DisplayName = c.Name });
                }
            }
        }
        private async void BtnAssign_Click(object sender, EventArgs e)
        {
            await ProcessBatch(true);
        }
        private async void BtnRemove_Click(object sender, EventArgs e)
        {
            await ProcessBatch(false);
        }
        private async Task ProcessBatch(bool isAssign)
        {
            if (clbProducts.CheckedItems.Count == 0 || clbCategories.CheckedItems.Count == 0)
            {
                ShowMessage("Válassz ki legalább egy terméket és egy kategóriát!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            btnAssign.Enabled = false;
            btnRemove.Enabled = false;
            var selectedProducts = new List<string>();
            foreach (ProductItem item in clbProducts.CheckedItems) selectedProducts.Add(item.Bvin);
            var selectedCategories = new List<string>();
            foreach (CategoryItem item in clbCategories.CheckedItems) selectedCategories.Add(item.Bvin);
            int success = 0;
            int skipped = 0;
            await Task.Run(() =>
            {
                foreach (var pBvin in selectedProducts)
                {
                    foreach (var cBvin in selectedCategories)
                    {
                        var prod = _products.FirstOrDefault(x => x.Bvin == pBvin);
                        var cat = _categories.FirstOrDefault(x => x.Bvin == cBvin);
                        if (prod != null && cat != null)
                        {
                            if (isAssign)
                            {
                                if (string.IsNullOrEmpty(prod.Category) || !prod.Category.Contains(cat.Name))
                                {
                                    if (_apiService.AssignProductToCategory(pBvin, cBvin)) success++;
                                }
                                else
                                {
                                    skipped++;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(prod.Category) && prod.Category.Contains(cat.Name))
                                {
                                    if (_apiService.RemoveProductFromCategory(pBvin, cBvin)) success++;
                                }
                                else
                                {
                                    skipped++;
                                }
                            }
                        }
                    }
                }
            });
            string opType = isAssign ? "hozzárendelés" : "eltávolítás";
            string skipReason = isAssign ? "Már eleve hozzá volt rendelve" : "Nem is volt hozzárendelve";
            ShowMessage($"Sikeres {opType}: {success} db.\n{skipReason}: {skipped} db.", "Eredmény", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (success > 0)
            {
                AssignmentChanged?.Invoke(this, EventArgs.Empty);
            }
            btnAssign.Enabled = true;
            btnRemove.Enabled = true;
        }

        protected virtual void ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, buttons, icon);
        }
        private class ProductItem
        {
            public string Bvin { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public override string ToString() => DisplayName;
        }
        private class CategoryItem
        {
            public string Bvin { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public override string ToString() => DisplayName;
        }
    }
}
