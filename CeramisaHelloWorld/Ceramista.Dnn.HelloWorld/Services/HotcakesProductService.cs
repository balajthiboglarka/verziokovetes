using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
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

        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return null;
            var text = Regex.Replace(html, "<[^>]+>", " ");
            text = System.Net.WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s{2,}", " ").Trim();
            return text.Length == 0 ? null : text;
        }

        // Returns the first column name that exists in the given table, or null if none match.
        private static string FindColumn(string tableName, params string[] candidates)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @t AND COLUMN_NAME = @c";

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                foreach (var col in candidates)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@t", tableName);
                        cmd.Parameters.AddWithValue("@c", col);
                        if ((int)cmd.ExecuteScalar() > 0) return col;
                    }
                }
            }
            return null;
        }

        public List<ProductViewModel> GetAllProducts()
        {
            // HotCakes description column name varies by version
            string descCol      = FindColumn("hcc_ProductTranslations",
                                      "Description", "LongDescription", "ShortDescription",
                                      "ProductLongDescription", "ProductShortDescription");
            string listPriceCol = FindColumn("hcc_Product", "ListPrice");

            var descSelect      = descCol      != null ? string.Format(", pt.{0}", descCol)  : "";
            var listPriceSelect = listPriceCol != null ? string.Format(", p.{0}",  listPriceCol) : "";

            var productSql = string.Format(@"
                SELECT DISTINCT
                    p.bvin,
                    pt.ProductName
                    {0}
                    , p.SitePrice
                    {1}
                    , p.ImageFileSmall
                    , p.Featured
                FROM hcc_Product p
                JOIN hcc_ProductTranslations pt ON pt.ProductId = p.bvin
                WHERE p.Status = 1
                  AND p.StoreId = 1
                  AND pt.Culture = 'hu-HU'
                ORDER BY pt.ProductName",
                descSelect,
                listPriceSelect);

            var products = new Dictionary<string, ProductViewModel>(StringComparer.OrdinalIgnoreCase);

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd  = new SqlCommand(productSql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id        = reader["bvin"].ToString();
                        var imageFile = reader["ImageFileSmall"] as string;

                        string desc = null;
                        if (descCol != null)
                            desc = StripHtml(reader[descCol] as string);

                        decimal listPrice = 0m;
                        if (listPriceCol != null)
                        {
                            var raw = reader[listPriceCol];
                            if (raw != DBNull.Value) listPrice = Convert.ToDecimal(raw);
                        }

                        products[id] = new ProductViewModel
                        {
                            Id          = id,
                            Name        = reader["ProductName"].ToString(),
                            Description = desc,
                            Price       = Convert.ToDecimal(reader["SitePrice"]),
                            ListPrice   = listPrice,
                            ImageUrl    = !string.IsNullOrWhiteSpace(imageFile)
                                ? string.Format("/Portals/0/Hotcakes/Data/products/{0}/small/{1}", id, imageFile)
                                : null,
                            IsFeatured  = reader["Featured"] != DBNull.Value && Convert.ToBoolean(reader["Featured"])
                        };
                    }
                }
            }

            if (products.Count == 0)
                return new List<ProductViewModel>();

            string catNameCol = FindColumn("hcc_CategoryTranslations",
                                   "Name", "CategoryName", "DisplayName");

            string catSql;
            if (catNameCol != null)
            {
                // Prefer hu-HU name; fall back to any available translation
                catSql = string.Format(@"
                    SELECT pxc.ProductId, pxc.CategoryId,
                           COALESCE(cthu.{0}, ctany.{0}) AS CategoryName
                    FROM hcc_ProductXCategory pxc
                    INNER JOIN hcc_Product p ON p.bvin = pxc.ProductId
                    LEFT JOIN hcc_CategoryTranslations cthu
                        ON cthu.CategoryId = pxc.CategoryId AND cthu.Culture = 'hu-HU'
                    LEFT JOIN (
                        SELECT CategoryId, MIN({0}) AS {0}
                        FROM hcc_CategoryTranslations
                        GROUP BY CategoryId
                    ) ctany ON ctany.CategoryId = pxc.CategoryId
                    WHERE p.Status = 1 AND p.StoreId = 1", catNameCol);
            }
            else
            {
                catSql = @"
                    SELECT pxc.ProductId, pxc.CategoryId, NULL AS CategoryName
                    FROM hcc_ProductXCategory pxc
                    INNER JOIN hcc_Product p ON p.bvin = pxc.ProductId
                    WHERE p.Status = 1 AND p.StoreId = 1";
            }

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd  = new SqlCommand(catSql, conn))
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
                            if (product.CategoryName == null)
                            {
                                var catName = reader["CategoryName"] as string;
                                if (!string.IsNullOrWhiteSpace(catName))
                                    product.CategoryName = catName.Trim();
                            }
                        }
                    }
                }
            }

            return new List<ProductViewModel>(products.Values);
        }
    }
}
