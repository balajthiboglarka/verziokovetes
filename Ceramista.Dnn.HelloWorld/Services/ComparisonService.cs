using System;
using System.Collections.Generic;
using System.Linq;
using Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Models;

namespace Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Services
{
    /// <summary>
    /// Termék-összehasonlító üzleti logika: hozzáadás, törlés, ajánlások.
    /// Nincs DNN / adatbázis függőség — unit tesztelhető.
    /// </summary>
    public class ComparisonService
    {
        private const int MaxComparisonSlots = 4;
        private const decimal MaxPriceMultiplier = 1.5m;

        // Megvizsgálja, hogy az új termék bekerülhet-e az összehasonlítóba.
        // false: ha a lista már tele van (>= 4), vagy a termék már benne van (duplikáció).
        public bool CanAddProduct(List<ProductViewModel> currentList, ProductViewModel newProduct)
        {
            if (currentList == null) throw new ArgumentNullException("currentList");
            if (newProduct == null) throw new ArgumentNullException("newProduct");

            if (currentList.Count >= MaxComparisonSlots)
                return false;

            if (currentList.Any(p => p.Id == newProduct.Id))
                return false;

            return true;
        }

        // Eltávolítja a megadott azonosítójú terméket a listából.
        // Ha az ID nem található, a lista változatlanul visszakerül.
        public List<ProductViewModel> RemoveProduct(List<ProductViewModel> currentList, string id)
        {
            if (currentList == null) throw new ArgumentNullException("currentList");
            if (id == null) throw new ArgumentNullException("id");

            return currentList.Where(p => p.Id != id).ToList();
        }

        // Legfeljebb 4 ajánlott terméket ad vissza a megtekintett termék alapján.
        // Szabályok:
        //   - A megtekintett termék kizárva
        //   - Csak stock > 0 termékek
        //   - Ár legfeljebb 50%-kal lehet magasabb a megtekintett termék áránál
        //   - Kategória egyezés esetén magasabb prioritás (+3 pont)
        public List<ProductViewModel> GetRecommendations(ProductViewModel currentProduct, List<ProductViewModel> allProducts)
        {
            if (currentProduct == null) throw new ArgumentNullException("currentProduct");
            if (allProducts == null) throw new ArgumentNullException("allProducts");

            decimal priceLimit = currentProduct.Price * MaxPriceMultiplier;

            return allProducts
                .Where(p => p.Id != currentProduct.Id)
                .Where(p => p.Stock > 0)
                .Where(p => p.Price <= priceLimit)
                .Select(p => new { Product = p, Score = Score(p, currentProduct) })
                .OrderByDescending(x => x.Score)
                .Take(4)
                .Select(x => x.Product)
                .ToList();
        }

        private static int Score(ProductViewModel candidate, ProductViewModel current)
        {
            if (candidate.CategoryIds == null || current.CategoryIds == null)
                return 0;

            return candidate.CategoryIds.Intersect(current.CategoryIds).Any() ? 3 : 0;
        }
    }
}
