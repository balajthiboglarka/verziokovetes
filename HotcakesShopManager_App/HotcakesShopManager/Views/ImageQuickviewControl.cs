using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
namespace HotcakesShopManager.Views
{
    public class ImageQuickviewControl : UserControl
    {
        private FlowLayoutPanel flowPanel;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private const int CardW = 480;
        private List<ProductViewModel> _allProducts;
        private TextBox _searchBox;
        private const int CardH = 530;
        private const int ImgH  = 360;
        public ImageQuickviewControl()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            var titleLabel = new Label
            {
                Text = "Kép gyorsnézet",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 75, 155),
                Dock = DockStyle.Top,
                Padding = new Padding(10, 15, 10, 15),
                AutoSize = true
            };
            var borderLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };
            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(20),
                WrapContents = true
            };
            var searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.White,
                WrapContents = false
            };
            
            var lblSearch = new Label
            {
                Text = "Keresés név alapján:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 8, 15, 0),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            var txtContainer = new Panel
            {
                Width = 400,
                Height = 44,
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            txtContainer.Paint += (s, e) => 
            {
                using(var pen = new Pen(Color.FromArgb(0, 75, 155), 2))
                {
                    e.Graphics.DrawRectangle(pen, 1, 1, txtContainer.Width - 3, txtContainer.Height - 3);
                }
            };
            
            _searchBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Location = new Point(10, 8),
                Width = 380,
                Font = new Font("Segoe UI", 12)
            };
            _searchBox.TextChanged += (s, e) => ApplyFilter();
            
            txtContainer.Controls.Add(_searchBox);
            
            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtContainer);

            scrollPanel.Controls.Add(flowPanel);
            this.Controls.Add(scrollPanel);
            this.Controls.Add(borderLine);
            this.Controls.Add(searchPanel);
            this.Controls.Add(titleLabel);
        }
        public void LoadData(List<ProductViewModel> products)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => LoadData(products)));
                return;
            }
            _allProducts = products;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allProducts == null)
            {
                RenderProducts(null);
                return;
            }
            
            var searchText = _searchBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                RenderProducts(_allProducts);
            }
            else
            {
                var filtered = new List<ProductViewModel>();
                foreach (var p in _allProducts)
                {
                    if (p.ProductName != null && p.ProductName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        filtered.Add(p);
                    }
                }
                RenderProducts(filtered);
            }
        }

        private void RenderProducts(IEnumerable<ProductViewModel> products)
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            flowPanel.SuspendLayout();
            foreach (Control c in flowPanel.Controls)
                foreach (Control child in c.Controls)
                    if (child is PictureBox pb && pb.Image != null)
                    { pb.Image.Dispose(); pb.Image = null; }
            flowPanel.Controls.Clear();
            if (products == null)
            {
                flowPanel.ResumeLayout(true);
                return;
            }
            var pending = new List<(PictureBox box, List<string> urls)>();
            foreach (var p in products)
            {
                var name = p.ProductName;
                var sku  = p.Sku;
                var card = new Panel
                {
                    Width = CardW,
                    Height = CardH,
                    Margin = new Padding(10),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };
                var imgBox = new PictureBox
                {
                    Location  = new Point(0, 0),
                    Size      = new Size(CardW, ImgH),
                    SizeMode  = PictureBoxSizeMode.Zoom,
                    Image     = CreatePlaceholderImage(),
                    BackColor = Color.FromArgb(240, 240, 240)
                };
                card.Controls.Add(imgBox);
                card.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    using (var nameFont = new Font("Segoe UI", 12, FontStyle.Bold))
                    using (var nameBrush = new SolidBrush(Color.FromArgb(30, 30, 30)))
                    {
                        var nameRect = new Rectangle(8, ImgH + 8, CardW - 16, 100);
                        g.DrawString(name, nameFont, nameBrush, nameRect,
                            new StringFormat { Trimming = StringTrimming.EllipsisWord });
                    }
                    using (var skuFont = new Font("Segoe UI", 10))
                    using (var skuBrush = new SolidBrush(Color.DarkGray))
                    {
                        g.DrawString($"Cikkszám: {sku}", skuFont, skuBrush,
                            new PointF(8, ImgH + 112));
                    }
                };
                flowPanel.Controls.Add(card);
                if (p.ImageUrls != null && p.ImageUrls.Count > 0)
                    pending.Add((imgBox, new List<string>(p.ImageUrls)));
            }
            flowPanel.ResumeLayout(true);
            this.Update(); 
            foreach (var item in pending)
            {
                IntPtr _ = item.box.Handle;
            }
            foreach (var item in pending)
            {
                StartImageLoad(item.box, item.urls, token);
            }
        }
        private void StartImageLoad(PictureBox box, List<string> urls, CancellationToken token)
        {
            Task.Run(async () =>
            {
                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12 |
                    System.Net.SecurityProtocolType.Tls11 |
                    System.Net.SecurityProtocolType.Tls;
                foreach (var url in urls)
                {
                    if (token.IsCancellationRequested) return;
                    try
                    {
                        byte[] bytes;
                        using (var wc = new System.Net.WebClient())
                            bytes = await wc.DownloadDataTaskAsync(new Uri(url));
                        if (bytes == null || bytes.Length < 100) continue;
                        using (var stream = new System.IO.MemoryStream(bytes))
                        using (var skBmp  = SkiaSharp.SKBitmap.Decode(stream))
                        {
                            if (skBmp == null) continue;
                            using (var skImg = SkiaSharp.SKImage.FromBitmap(skBmp))
                            using (var data  = skImg.Encode(SkiaSharp.SKEncodedImageFormat.Png, 90))
                            {
                                if (token.IsCancellationRequested) return;
                                var ms  = new System.IO.MemoryStream(data.ToArray());
                                var bmp = Image.FromStream(ms);
                                if (box.IsHandleCreated && !box.IsDisposed)
                                {
                                    box.Invoke(new Action(() =>
                                    {
                                        if (!box.IsDisposed)
                                        {
                                            var old = box.Image;
                                            box.Image = bmp;
                                            old?.Dispose();
                                            box.Parent?.Invalidate();
                                        }
                                    }));
                                }
                                return; 
                            }
                        }
                    }
                    catch { } 
                }
            });
        }
        private Bitmap CreatePlaceholderImage()
        {
            var bmp = new Bitmap(CardW, ImgH);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(235, 235, 235));
                using (var brush = new SolidBrush(Color.FromArgb(160, 160, 160)))
                using (var font  = new Font("Segoe UI", 14))
                {
                    var fmt = new StringFormat
                    {
                        Alignment     = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("Nincs kép", font, brush, new Rectangle(0, 0, CardW, ImgH), fmt);
                }
            }
            return bmp;
        }
    }
}
