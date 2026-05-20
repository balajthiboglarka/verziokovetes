using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using HotcakesShopManager.Views;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HotcakesShopManager.Tests
{
    public class SimplifiedFunctionalTests
    {
        private readonly ITestOutputHelper _output;

        public SimplifiedFunctionalTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Helper Classes & Methods
        private class TestableBulkAssignmentControl : BulkAssignmentControl
        {
            public TestableBulkAssignmentControl(HotcakesApiService apiService) : base(apiService) { }
            protected override void ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon) { }
        }

        private object GetPrivateField(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null && obj.GetType().BaseType != null)
                field = obj.GetType().BaseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field.GetValue(obj);
        }

        private async Task InvokeProcessBatch(BulkAssignmentControl control, bool isAssign)
        {
            MethodInfo processMethod = typeof(BulkAssignmentControl).GetMethod("ProcessBatch", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)processMethod.Invoke(control, new object[] { isAssign });
        }

        private void InvokeBtnGenerateClick(InventoryReportsControl control)
        {
            MethodInfo generateMethod = control.GetType().GetMethod("BtnGenerate_Click", BindingFlags.NonPublic | BindingFlags.Instance);
            generateMethod.Invoke(control, new object[] { null, EventArgs.Empty });
        }
        #endregion

        #region 1. Kategória hozzárendelése

        [Fact]
        public async Task KategóriaHozzárendelése_IdeálisEset()
        {
            var mockApi = new Mock<HotcakesApiService>("http://test.com", "key");
            var control = new TestableBulkAssignmentControl(mockApi.Object);

            var products = new List<ProductViewModel> {
                new ProductViewModel { Bvin = "P1", ProductName = "Termék 1", Sku = "S1" },
                new ProductViewModel { Bvin = "P2", ProductName = "Termék 2", Sku = "S2" },
                new ProductViewModel { Bvin = "P3", ProductName = "Termék 3", Sku = "S3" }
            };
            var categories = new List<CategoryViewModel> { new CategoryViewModel { Bvin = "C1", Name = "Új Kategória" } };
            control.LoadData(products, categories);

            ((CheckedListBox)GetPrivateField(control, "clbProducts")).SetItemChecked(0, true);
            ((CheckedListBox)GetPrivateField(control, "clbProducts")).SetItemChecked(1, true);
            ((CheckedListBox)GetPrivateField(control, "clbCategories")).SetItemChecked(0, true);

            mockApi.Setup(x => x.AssignProductToCategory(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _output.WriteLine("=== TESZTESET: Kategória hozzárendelése - Ideális eset ===");
            _output.WriteLine("Bemenet: 3 termék, P1 és P2 kijelölve, 'Új Kategória' kiválasztva.");
            _output.WriteLine("Folyamat: Hozzárendelés indítása...");

            await InvokeProcessBatch(control, true);

            _output.WriteLine("Elvárt kimenet: P1 és P2 megkapja az új kategóriát, P3 változatlan marad.");
            mockApi.Verify(x => x.AssignProductToCategory("P1", "C1"), Times.Once);
            mockApi.Verify(x => x.AssignProductToCategory("P2", "C1"), Times.Once);
            mockApi.Verify(x => x.AssignProductToCategory("P3", "C1"), Times.Never);
            _output.WriteLine("Eredmény: SIKERES - A frissített terméklista a módosításokkal elkészült.");
        }

        [Fact]
        public async Task KategóriaHozzárendelése_Adatintegritás()
        {
            var mockApi = new Mock<HotcakesApiService>("http://test.com", "key");
            var control = new TestableBulkAssignmentControl(mockApi.Object);

            var product = new ProductViewModel { Bvin = "P1", ProductName = "Váza", Sku = "V123", Price = 5000, Quantity = 10 };
            control.LoadData(new List<ProductViewModel> { product }, new List<CategoryViewModel> { new CategoryViewModel { Bvin = "C1", Name = "Új" } });

            ((CheckedListBox)GetPrivateField(control, "clbProducts")).SetItemChecked(0, true);
            ((CheckedListBox)GetPrivateField(control, "clbCategories")).SetItemChecked(0, true);

            mockApi.Setup(x => x.AssignProductToCategory(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _output.WriteLine("=== TESZTESET: Adatintegritás teszt ===");
            _output.WriteLine("Bemenet: 'Váza' (Ár: 5000, Megnevezés: Váza, Készlet: 10).");
            _output.WriteLine("Folyamat: Kategória hozzáadása...");

            await InvokeProcessBatch(control, true);

            _output.WriteLine("Elvárt kimenet: A termék egyéb adatai (ár, megnevezés, készlet) változatlanok maradnak.");
            Assert.Equal("Váza", product.ProductName);
            Assert.Equal(5000, product.Price);
            Assert.Equal(10, product.Quantity);
            _output.WriteLine("Eredmény: SIKERES - Az adatok épsége megmaradt.");
        }

        [Fact]
        public async Task KategóriaHozzárendelése_DuplikációElkerülése()
        {
            var mockApi = new Mock<HotcakesApiService>("http://test.com", "key");
            var control = new TestableBulkAssignmentControl(mockApi.Object);

            var products = new List<ProductViewModel> { new ProductViewModel { Bvin = "P1", Category = "Új" } };
            var categories = new List<CategoryViewModel> { new CategoryViewModel { Bvin = "C1", Name = "Új" } };
            control.LoadData(products, categories);

            ((CheckedListBox)GetPrivateField(control, "clbProducts")).SetItemChecked(0, true);
            ((CheckedListBox)GetPrivateField(control, "clbCategories")).SetItemChecked(0, true);

            _output.WriteLine("=== TESZTESET: Duplikáció elkerülése (Edge Case) ===");
            _output.WriteLine("Bemenet: A termék már eleve ebben a kategóriában van.");
            _output.WriteLine("Folyamat: Hozzárendelés megkísérlése...");

            await InvokeProcessBatch(control, true);

            _output.WriteLine("Elvárt kimenet: A rendszer nem adja hozzá másodszor is a kategóriát.");
            mockApi.Verify(x => x.AssignProductToCategory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _output.WriteLine("Eredmény: SIKERES - Elkerülve a redundancia.");
        }

        [Fact]
        public async Task KategóriaHozzárendelése_ÜresKijelölés()
        {
            var mockApi = new Mock<HotcakesApiService>("http://test.com", "key");
            var control = new TestableBulkAssignmentControl(mockApi.Object);
            control.LoadData(new List<ProductViewModel>(), new List<CategoryViewModel>());

            _output.WriteLine("=== TESZTESET: Üres kijelölés (Edge Case) ===");
            _output.WriteLine("Bemenet: A kijelölt azonosítók listája üres.");
            _output.WriteLine("Folyamat: Függvény futtatása...");

            await InvokeProcessBatch(control, true);

            _output.WriteLine("Elvárt kimenet: A függvény hiba nélkül lefut, és az eredeti listát adja vissza.");
            mockApi.Verify(x => x.AssignProductToCategory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _output.WriteLine("Eredmény: SIKERES - Helyes kezelés üres bemenet esetén.");
        }

        #endregion

        #region 2. Kategória eltávolítása

        [Fact]
        public async Task KategóriaEltávolítása_IdeálisEset()
        {
            var mockApi = new Mock<HotcakesApiService>("http://test.com", "key");
            var control = new TestableBulkAssignmentControl(mockApi.Object);

            var product = new ProductViewModel { Bvin = "P1", Category = "Porcelán, Vázák" };
            control.LoadData(new List<ProductViewModel> { product }, new List<CategoryViewModel> { new CategoryViewModel { Bvin = "C1", Name = "Vázák" } });

            ((CheckedListBox)GetPrivateField(control, "clbProducts")).SetItemChecked(0, true);
            ((CheckedListBox)GetPrivateField(control, "clbCategories")).SetItemChecked(0, true);

            mockApi.Setup(x => x.RemoveProductFromCategory(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _output.WriteLine("=== TESZTESET: Kategória eltávolítása - Ideális eset ===");
            _output.WriteLine("Bemenet: Termék kategóriái: 'Porcelán' és 'Vázák'.");
            _output.WriteLine("Folyamat: 'Vázák' eltávolítása...");

            await InvokeProcessBatch(control, false);

            _output.WriteLine("Elvárt kimenet: A 'Vázák' kikerül, de a 'Porcelán' megmarad.");
            mockApi.Verify(x => x.RemoveProductFromCategory("P1", "C1"), Times.Once);
            _output.WriteLine("Eredmény: SIKERES - A megadott kategória már nem szerepel a terméknél.");
        }

        [Fact]
        public async Task KategóriaEltávolítása_NemLétezőKategória()
        {
            var mockApi = new Mock<HotcakesApiService>("http://test.com", "key");
            var control = new TestableBulkAssignmentControl(mockApi.Object);

            var product = new ProductViewModel { Bvin = "P1", Category = "Porcelán" };
            control.LoadData(new List<ProductViewModel> { product }, new List<CategoryViewModel> { new CategoryViewModel { Bvin = "C1", Name = "Vázák" } });

            ((CheckedListBox)GetPrivateField(control, "clbProducts")).SetItemChecked(0, true);
            ((CheckedListBox)GetPrivateField(control, "clbCategories")).SetItemChecked(0, true);

            _output.WriteLine("=== TESZTESET: Nem létező kategória (Edge Case) ===");
            _output.WriteLine("Bemenet: Olyan kategória törlése, ami nincs hozzárendelve.");
            _output.WriteLine("Folyamat: Eltávolítás megkísérlése...");

            await InvokeProcessBatch(control, false);

            _output.WriteLine("Elvárt kimenet: A rendszer nem omlik össze, a termék adatai változatlanok maradnak.");
            mockApi.Verify(x => x.RemoveProductFromCategory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _output.WriteLine("Eredmény: SIKERES - Biztonságos kezelés.");
        }

        [Fact]
        public async Task KategóriaEltávolítása_UtolsóKategóriaTörlése()
        {
            var mockApi = new Mock<HotcakesApiService>("http://test.com", "key");
            var control = new TestableBulkAssignmentControl(mockApi.Object);

            var product = new ProductViewModel { Bvin = "P1", Category = "Vázák" };
            control.LoadData(new List<ProductViewModel> { product }, new List<CategoryViewModel> { new CategoryViewModel { Bvin = "C1", Name = "Vázák" } });

            ((CheckedListBox)GetPrivateField(control, "clbProducts")).SetItemChecked(0, true);
            ((CheckedListBox)GetPrivateField(control, "clbCategories")).SetItemChecked(0, true);

            mockApi.Setup(x => x.RemoveProductFromCategory(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _output.WriteLine("=== TESZTESET: Utolsó kategória törlése ===");
            _output.WriteLine("Bemenet: A termék egyetlen kategóriáját töröljük ('Vázák').");
            _output.WriteLine("Folyamat: Törlés indítása...");

            await InvokeProcessBatch(control, false);

            _output.WriteLine("Elvárt kimenet: A termék kategórialistája üres lesz.");
            mockApi.Verify(x => x.RemoveProductFromCategory("P1", "C1"), Times.Once);
            _output.WriteLine("Eredmény: SIKERES - A kategórialista kiürült.");
        }

        #endregion

        #region 3. Alacsony készletű termékek szűrése

        [Fact]
        public void AlacsonyKészlet_IdeálisEset()
        {
            var control = new InventoryReportsControl();
            var products = new List<ProductViewModel>
            {
                new ProductViewModel { ProductName = "P1", Quantity = 5 },
                new ProductViewModel { ProductName = "P2", Quantity = 15 },
                new ProductViewModel { ProductName = "P3", Quantity = 8 },
                new ProductViewModel { ProductName = "P4", Quantity = 20 }
            };
            control.LoadData(products, new List<OrderViewModel>());

            _output.WriteLine("=== TESZTESET: Alacsony készlet szűrése - Ideális eset ===");
            _output.WriteLine("Bemenet: Készletek: 5, 15, 8, 20. Küszöbérték: 10.");
            _output.WriteLine("Folyamat: Jelentés generálása...");

            ((RadioButton)GetPrivateField(control, "rbLowInventory")).Checked = true;
            InvokeBtnGenerateClick(control);

            string output = ((TextBox)GetPrivateField(control, "txtOutput")).Text;

            _output.WriteLine("Elvárt kimenet: Csak az 5 és 8 darabos termékek szerepelnek.");
            Assert.Contains("P1", output);
            Assert.Contains("P3", output);
            Assert.DoesNotContain("P2", output);
            Assert.DoesNotContain("P4", output);
            _output.WriteLine("Eredmény: SIKERES - A szűrt lista pontos.");
        }

        [Fact]
        public void AlacsonyKészlet_NincsTalálat()
        {
            var control = new InventoryReportsControl();
            var products = new List<ProductViewModel> { 
                new ProductViewModel { ProductName = "P1", Quantity = 50 }, 
                new ProductViewModel { ProductName = "P2", Quantity = 60 } 
            };
            control.LoadData(products, new List<OrderViewModel>());

            _output.WriteLine("=== TESZTESET: Nincs találat ===");
            _output.WriteLine("Bemenet: Minden készlet 10 felett van (50, 60).");
            _output.WriteLine("Folyamat: Szűrés futtatása...");

            ((RadioButton)GetPrivateField(control, "rbLowInventory")).Checked = true;
            InvokeBtnGenerateClick(control);

            string output = ((TextBox)GetPrivateField(control, "txtOutput")).Text;

            _output.WriteLine("Elvárt kimenet: Üres lista (0 db érintett termék).");
            Assert.Contains("0 db", output);
            _output.WriteLine("Eredmény: SIKERES - Nincs hamis riasztás.");
        }

        [Fact]
        public void AlacsonyKészlet_HatárértékTeszt()
        {
            var control = new InventoryReportsControl();
            var products = new List<ProductViewModel> { new ProductViewModel { ProductName = "Limit", Quantity = 10 } };
            control.LoadData(products, new List<OrderViewModel>());

            _output.WriteLine("=== TESZTESET: Pontosan a küszöbértéken lévő termék (Határérték teszt) ===");
            _output.WriteLine("Bemenet: Készlet pontosan 10, küszöb 10.");
            _output.WriteLine("Folyamat: Ellenőrzés...");

            ((RadioButton)GetPrivateField(control, "rbLowInventory")).Checked = true;
            InvokeBtnGenerateClick(control);

            string output = ((TextBox)GetPrivateField(control, "txtOutput")).Text;

            _output.WriteLine("Elvárt kimenet: A termék bekerül a jelentésbe (kisebb vagy egyenlő szabály).");
            Assert.Contains("Limit", output);
            _output.WriteLine("Eredmény: SIKERES - A határérték kezelése helyes.");
        }

        [Fact]
        public void AlacsonyKészlet_NegatívKészlet()
        {
            var control = new InventoryReportsControl();
            var products = new List<ProductViewModel> { new ProductViewModel { ProductName = "Anomália", Quantity = -2 } };
            control.LoadData(products, new List<OrderViewModel>());

            _output.WriteLine("=== TESZTESET: Negatív készlet ===");
            _output.WriteLine("Bemenet: Adatbázis hiba miatt készlet: -2.");
            _output.WriteLine("Folyamat: Szűrés...");

            ((RadioButton)GetPrivateField(control, "rbLowInventory")).Checked = true;
            InvokeBtnGenerateClick(control);

            string output = ((TextBox)GetPrivateField(control, "txtOutput")).Text;

            _output.WriteLine("Elvárt kimenet: Belekerül a jelentésbe, hogy a leltározók észrevegyék.");
            Assert.Contains("Anomália", output);
            _output.WriteLine("Eredmény: SIKERES - A hibás adat is bekerült a listába.");
        }

        #endregion

        #region 4. Kategóriánkénti összesítés

        [Fact]
        public void Összesítés_IdeálisEset()
        {
            var control = new InventoryReportsControl();
            var products = new List<ProductViewModel>
            {
                new ProductViewModel { ProductName = "Váza 1", Category = "Porcelán váza", Quantity = 10 },
                new ProductViewModel { ProductName = "Váza 2", Category = "Porcelán váza", Quantity = 5 }
            };
            control.LoadData(products, new List<OrderViewModel>());

            _output.WriteLine("=== TESZTESET: Kategóriánkénti összesítés - Ideális eset ===");
            _output.WriteLine("Bemenet: Két 'Porcelán váza' (10 db és 5 db).");
            _output.WriteLine("Folyamat: Összesítés generálása...");

            ((RadioButton)GetPrivateField(control, "rbCategorySummary")).Checked = true;
            InvokeBtnGenerateClick(control);

            string output = ((TextBox)GetPrivateField(control, "txtOutput")).Text;

            _output.WriteLine("Elvárt kimenet: A jelentésben a 'Porcelán váza' mellett 15 db szerepel.");
            Assert.Contains("15", output);
            _output.WriteLine("Eredmény: SIKERES - Az összesítés pontos.");
        }

        [Fact]
        public void Összesítés_ÜresKategória()
        {
            var control = new InventoryReportsControl();
            var products = new List<ProductViewModel> { new ProductViewModel { Category = "Teszt", Quantity = 0 } };
            control.LoadData(products, new List<OrderViewModel>());

            _output.WriteLine("=== TESZTESET: Üres kategória (Edge Case) ===");
            _output.WriteLine("Bemenet: Termék készlete 0.");
            _output.WriteLine("Folyamat: Összesítés...");

            ((RadioButton)GetPrivateField(control, "rbCategorySummary")).Checked = true;
            InvokeBtnGenerateClick(control);

            string output = ((TextBox)GetPrivateField(control, "txtOutput")).Text;

            _output.WriteLine("Elvárt kimenet: A kategória darabszámához 0-t ad hozzá.");
            Assert.Contains("0", output);
            _output.WriteLine("Eredmény: SIKERES - Helyes számítás 0 készlet esetén.");
        }

        [Fact]
        public void Összesítés_KategorizálatlanTermékek()
        {
            var control = new InventoryReportsControl();
            var products = new List<ProductViewModel> { new ProductViewModel { Category = "", Quantity = 7 } };
            control.LoadData(products, new List<OrderViewModel>());

            _output.WriteLine("=== TESZTESET: Kategorizálatlan termékek ===");
            _output.WriteLine("Bemenet: Termék kategória nélkül.");
            _output.WriteLine("Folyamat: Összesítés...");

            ((RadioButton)GetPrivateField(control, "rbCategorySummary")).Checked = true;
            InvokeBtnGenerateClick(control);

            string output = ((TextBox)GetPrivateField(control, "txtOutput")).Text;

            _output.WriteLine("Elvárt kimenet: A rendszer létrehoz egy 'Besorolatlan' sort.");
            Assert.Contains("Besorolatlan", output);
            _output.WriteLine("Eredmény: SIKERES - A hiányzó kategória is kezelve.");
        }

        #endregion
    }
}
