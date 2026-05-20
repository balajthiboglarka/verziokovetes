using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Models;
using Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Services;

namespace Ceramista.Dnn.HelloWorld.Tests
{
    [TestFixture]
    public class ComparisonServiceTests
    {
        private ComparisonService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new ComparisonService();
        }

        // Segédmetódus: egyszerű tesztterméket hoz létre
        private static ProductViewModel MakeProduct(
            string id,
            string categoryId = "cat-vaza",
            decimal price = 10000m,
            int stock = 5)
        {
            return new ProductViewModel
            {
                Id = id,
                Name = "Teszt termék " + id,
                Price = price,
                Stock = stock,
                CategoryIds = categoryId != null
                    ? new List<string> { categoryId }
                    : new List<string>()
            };
        }

        // ================================================================
        // 1. Termék összehasonlítás — CanAddProduct
        // ================================================================

        [Test]
        public void CanAddProduct_UresListahoz_VisszaadTrue()
        {
            var lista = new List<ProductViewModel>();
            Assert.IsTrue(_service.CanAddProduct(lista, MakeProduct("p1")));
        }

        [Test]
        public void CanAddProduct_EgyElemesListahoz_VisszaadTrue()
        {
            var lista = new List<ProductViewModel> { MakeProduct("p1") };
            Assert.IsTrue(_service.CanAddProduct(lista, MakeProduct("p2")));
        }

        [Test]
        public void CanAddProduct_KetElemesListahoz_VisszaadTrue()
        {
            var lista = new List<ProductViewModel> { MakeProduct("p1"), MakeProduct("p2") };
            Assert.IsTrue(_service.CanAddProduct(lista, MakeProduct("p3")));
        }

        [Test]
        public void CanAddProduct_HaromElemesListahoz_VisszaadTrue()
        {
            var lista = new List<ProductViewModel>
            {
                MakeProduct("p1"), MakeProduct("p2"), MakeProduct("p3")
            };
            Assert.IsTrue(_service.CanAddProduct(lista, MakeProduct("p4")));
        }

        [Test]
        public void CanAddProduct_NegyElemesListahoz_OtodikElem_VisszaadFalse()
        {
            var lista = new List<ProductViewModel>
            {
                MakeProduct("p1"), MakeProduct("p2"), MakeProduct("p3"), MakeProduct("p4")
            };
            Assert.IsFalse(_service.CanAddProduct(lista, MakeProduct("p5")),
                "Teli lista (4 elem) esetén újabb termék nem adható hozzá.");
        }

        [Test]
        public void CanAddProduct_DuplaID_VisszaadFalse()
        {
            var lista = new List<ProductViewModel> { MakeProduct("p1"), MakeProduct("p2") };
            Assert.IsFalse(_service.CanAddProduct(lista, MakeProduct("p1")),
                "Már listában lévő azonosítójú termék nem adható hozzá újra.");
        }

        // ================================================================
        // 2. Termék törlése — RemoveProduct
        // ================================================================

        [Test]
        public void RemoveProduct_LetezoID_ListaHossza1velCsokkent()
        {
            var lista = new List<ProductViewModel>
            {
                MakeProduct("p1"), MakeProduct("p2"), MakeProduct("p3")
            };
            var eredmeny = _service.RemoveProduct(lista, "p2");
            Assert.AreEqual(2, eredmeny.Count);
        }

        [Test]
        public void RemoveProduct_LetezoID_ElemKikerultAListabol()
        {
            var lista = new List<ProductViewModel> { MakeProduct("p1"), MakeProduct("p2") };
            var eredmeny = _service.RemoveProduct(lista, "p1");
            Assert.IsFalse(eredmeny.Any(p => p.Id == "p1"),
                "A törölt termék nem szerepelhet az eredményben.");
        }

        [Test]
        public void RemoveProduct_NemLetezoID_ListaValtozatlan()
        {
            var lista = new List<ProductViewModel> { MakeProduct("p1"), MakeProduct("p2") };
            var eredmeny = _service.RemoveProduct(lista, "p999");
            Assert.AreEqual(2, eredmeny.Count,
                "Ismeretlen ID esetén a lista elemszáma nem változhat.");
        }

        [Test]
        public void RemoveProduct_UtolsoElem_VisszaadUresLista()
        {
            var lista = new List<ProductViewModel> { MakeProduct("p1") };
            var eredmeny = _service.RemoveProduct(lista, "p1");
            Assert.AreEqual(0, eredmeny.Count,
                "Az utolsó elem törlése után üres listát kell visszaadni.");
        }

        // ================================================================
        // 3. Ajánlórendszer — GetRecommendations
        // ================================================================

        [Test]
        public void GetRecommendations_MaxNegyAjanlatotAdVissza()
        {
            var current = MakeProduct("current", "cat-vaza", 10000m);
            var osszes = new List<ProductViewModel>
            {
                current,
                MakeProduct("p1", "cat-vaza", 9000m),
                MakeProduct("p2", "cat-vaza", 9000m),
                MakeProduct("p3", "cat-vaza", 9000m),
                MakeProduct("p4", "cat-vaza", 9000m),
                MakeProduct("p5", "cat-vaza", 9000m),
            };
            var eredmeny = _service.GetRecommendations(current, osszes);
            Assert.LessOrEqual(eredmeny.Count, 4, "Az ajánlások száma maximum 4 lehet.");
        }

        [Test]
        public void GetRecommendations_NemAjanljaSajaMatMagat()
        {
            var current = MakeProduct("current", "cat-vaza", 10000m);
            var osszes = new List<ProductViewModel>
            {
                current,
                MakeProduct("p1", "cat-vaza", 9000m),
                MakeProduct("p2", "cat-vaza", 9000m),
            };
            var eredmeny = _service.GetRecommendations(current, osszes);
            Assert.IsFalse(eredmeny.Any(p => p.Id == "current"),
                "A megtekintett termék nem szerepelhet saját ajánlásai között.");
        }

        [Test]
        public void GetRecommendations_NullKeszletuTermekKizarva()
        {
            var current = MakeProduct("current", "cat-vaza", 10000m);
            var nincsenKeszlet = MakeProduct("nincs", "cat-vaza", 10000m, stock: 0);
            var vanKeszlet = MakeProduct("van", "cat-vaza", 10000m, stock: 1);
            var osszes = new List<ProductViewModel> { current, nincsenKeszlet, vanKeszlet };

            var eredmeny = _service.GetRecommendations(current, osszes);

            Assert.IsFalse(eredmeny.Any(p => p.Id == "nincs"),
                "Nulla készletű termék nem ajánlható.");
            Assert.IsTrue(eredmeny.Any(p => p.Id == "van"),
                "Pozitív készletű terméknek benne kell lennie az ajánlásokban.");
        }

        [Test]
        public void GetRecommendations_TulDragaTermekKizarva()
        {
            // 15 000 Ft megtekintett termék → plafon: 22 500 Ft (50% feletti ár nem ajánlható)
            var current = MakeProduct("current", "cat-vaza", 15000m);
            var tulDraga = MakeProduct("draga", "cat-vaza", 22501m);
            var megfizetheto = MakeProduct("jo", "cat-vaza", 22500m);
            var osszes = new List<ProductViewModel> { current, tulDraga, megfizetheto };

            var eredmeny = _service.GetRecommendations(current, osszes);

            Assert.IsFalse(eredmeny.Any(p => p.Id == "draga"),
                "22 501 Ft > 50%-os plafon (22 500 Ft), ki kell zárni.");
            Assert.IsTrue(eredmeny.Any(p => p.Id == "jo"),
                "22 500 Ft pontosan a plafon, benne kell lennie.");
        }

        [Test]
        public void GetRecommendations_KategoriaEgyezesTobbseg()
        {
            // Az ajánlások legalább 75%-a (4-ből legalább 3) egyezzen meg a kategóriával
            var current = MakeProduct("current", "cat-vaza", 10000m);
            var osszes = new List<ProductViewModel>
            {
                current,
                MakeProduct("v1", "cat-vaza", 9000m),
                MakeProduct("v2", "cat-vaza", 9000m),
                MakeProduct("v3", "cat-vaza", 9000m),
                MakeProduct("v4", "cat-vaza", 9000m),
                MakeProduct("t1", "cat-tal",  9000m),
                MakeProduct("t2", "cat-tal",  9000m),
            };

            var eredmeny = _service.GetRecommendations(current, osszes);
            int kategoriaEgyezes = eredmeny.Count(p => p.CategoryIds.Contains("cat-vaza"));
            int elvart = (int)Math.Ceiling(eredmeny.Count * 0.75);

            Assert.GreaterOrEqual(kategoriaEgyezes, elvart,
                $"Az ajánlások legalább 75%-ának (elvárás: {elvart} db) azonos kategóriájúnak kell lennie.");
        }
    }
}
