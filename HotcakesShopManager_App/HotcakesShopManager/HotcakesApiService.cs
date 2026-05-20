using System;
using System.Collections.Generic;
using System.Linq;
using Hotcakes.CommerceDTO.v1.Client;
using Hotcakes.CommerceDTO.v1.Catalog;
using Hotcakes.CommerceDTO.v1.Orders;
namespace HotcakesShopManager
{
    public class OrderViewModel
    {
        public string Bvin { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalGrand { get; set; }
        public DateTime TimeOfOrder { get; set; }
    }
    public class ProductViewModel
    {
        public string Bvin { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string DisplayName => $"{ProductName} [{(string.IsNullOrEmpty(Category) ? "Nincs besorolva" : Category)}]";
        public override string ToString() => DisplayName;
    }
    public class CategoryViewModel
    {
        public string Bvin { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
    public class HotcakesApiService
    {
        private readonly Api _api;
        private readonly string _baseUrl;
        public HotcakesApiService(string baseUrl, string apiKey)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _api = new Api(baseUrl, apiKey);
        }
        public virtual List<ProductViewModel> GetProductData()
        {
            var productsResponse = _api.ProductsFindAll();
            if (productsResponse.Errors != null && productsResponse.Errors.Count > 0)
                throw new Exception(string.Join(", ", productsResponse.Errors.Select(e => e.Description)));
            var products = productsResponse.Content ?? new List<ProductDTO>();
            var result = new System.Collections.Concurrent.ConcurrentBag<ProductViewModel>();
            var allInventory = new List<ProductInventoryDTO>();
            try 
            {
                var invResponse = _api.ProductInventoryFindAll();
                if (invResponse.Content != null) allInventory = invResponse.Content;
            } catch { }
            System.Threading.Tasks.Parallel.ForEach(products, new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = 10 }, p =>
            {
                int quantity = allInventory.Where(i => i.ProductBvin == p.Bvin).Sum(i => i.QuantityOnHand);
                string categoryName = "N/A";
                try 
                {
                    var cats = _api.CategoriesFindForProduct(p.Bvin);
                    if (cats.Content != null && cats.Content.Count > 0)
                        categoryName = string.Join(", ", cats.Content.Select(c => c.Name));
                } catch { }
                var urls = new List<string>();
                if (!string.IsNullOrEmpty(p.ImageFileSmall))
                {
                    urls.Add($"{_baseUrl}/Portals/0/Hotcakes/Data/products/{p.Bvin}/small/{p.ImageFileSmall}");
                    urls.Add($"{_baseUrl}/Portals/0/Hotcakes/Data/products/{p.Bvin}/{p.ImageFileSmall}");
                    urls.Add($"{_baseUrl}/Portals/0/Images/{p.ImageFileSmall}");
                }
                if (!string.IsNullOrEmpty(p.ImageFileMedium))
                {
                    urls.Add($"{_baseUrl}/Portals/0/Hotcakes/Data/products/{p.Bvin}/medium/{p.ImageFileMedium}");
                    urls.Add($"{_baseUrl}/Portals/0/Hotcakes/Data/products/{p.Bvin}/{p.ImageFileMedium}");
                    urls.Add($"{_baseUrl}/Portals/0/Images/{p.ImageFileMedium}");
                }
                result.Add(new ProductViewModel
                {
                    Bvin = p.Bvin,
                    Sku = p.Sku,
                    ProductName = p.ProductName,
                    Price = p.SitePrice,
                    Category = categoryName,
                    Quantity = quantity,
                    ImageUrls = urls
                });
            });
            return result.OrderBy(r => r.ProductName).ToList();
        }
        public virtual List<CategoryViewModel> GetCategories()
        {
            var result = new List<CategoryViewModel>();
            var res = _api.CategoriesFindAll();
            if (res.Content != null)
            {
                result.AddRange(res.Content.Select(c => new CategoryViewModel { Bvin = c.Bvin, Name = c.Name }));
            }
            return result;
        }
        public virtual bool AssignProductToCategory(string productBvin, string categoryBvin)
        {
            var res = _api.CategoryProductAssociationsQuickCreate(productBvin, categoryBvin);
            return res.Content;
        }
        public virtual bool RemoveProductFromCategory(string productBvin, string categoryBvin)
        {
            var res = _api.CategoryProductAssociationsUnrelate(productBvin, categoryBvin);
            return res.Content;
        }
        public virtual List<OrderViewModel> GetOrders()
        {
            var result = new List<OrderViewModel>();
            try
            {
                var res = _api.OrdersFindAll();
                if (res.Content != null)
                {
                    result.AddRange(res.Content.Select(o => new OrderViewModel
                    {
                        Bvin = o.bvin,
                        OrderNumber = o.bvin, 
                        TotalGrand = o.TotalGrand,
                        TimeOfOrder = o.TimeOfOrderUtc.ToLocalTime()
                    }));
                }
            }
            catch { }
            return result.OrderByDescending(o => o.TimeOfOrder).ToList();
        }
    }
}
