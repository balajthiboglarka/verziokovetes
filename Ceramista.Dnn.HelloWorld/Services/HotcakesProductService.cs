using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;
using Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Models;

namespace Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Services
{
    public class HotcakesProductService
    {
        private static string ConnectionString
        {
            get { return WebConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString; }
        }

        public List<ProductViewModel> GetAllProducts()
        {
            var products = new Dictionary<string, ProductViewModel>(StringComparer.OrdinalIgnoreCase);

            const string productSql = @"
                SELECT DISTINCT
                    p.bvin,
                    pt.ProductName,
                    p.SitePrice,
                    p.ImageFileSmall,
                    p.Featured
                FROM hcc_Product p
                JOIN hcc_ProductTranslations pt ON pt.ProductId = p.bvin
                WHERE p.Status = 1
                  AND p.StoreId = 1
                  AND pt.Culture = 'hu-HU'
                ORDER BY pt.ProductName";

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(productSql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader["bvin"].ToString();
                        var imageFile = reader["ImageFileSmall"] as string;
                        products[id] = new ProductViewModel
                        {
                            Id = id,
                            Name = reader["ProductName"].ToString(),
                            Price = Convert.ToDecimal(reader["SitePrice"]),
                            ImageUrl = !string.IsNullOrWhiteSpace(imageFile)
                                ? string.Format("/Portals/0/Hotcakes/Data/products/{0}/small/{1}", id, imageFile)
                                : null,
                            IsFeatured = reader["Featured"] != DBNull.Value && Convert.ToBoolean(reader["Featured"])
                        };
                    }
                }
            }

            if (products.Count == 0)
                return new List<ProductViewModel>();

            const string catSql = @"
                SELECT pxc.ProductId, pxc.CategoryId
                FROM hcc_ProductXCategory pxc
                INNER JOIN hcc_Product p ON p.bvin = pxc.ProductId
                WHERE p.Status = 1 AND p.StoreId = 1";

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(catSql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var productId = reader["ProductId"].ToString();
                        ProductViewModel product;
                        if (products.TryGetValue(productId, out product))
                        {
                            product.CategoryIds.Add(reader["CategoryId"].ToString());
                        }
                    }
                }
            }

            return new List<ProductViewModel>(products.Values);
        }
    }
}
