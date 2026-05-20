# Task List — Ceramista Termék-összehasonlító Modul

## Git setup
- [x] Git identity configured (Hektor Salvador, salvador.vh05@gmail.com)
- [x] Repo initialized in `module-helloworld/`
- [x] Remote added: https://github.com/balajthiboglarka/verziokovetes
- [x] Feature branch created: `feature/osszehasonlito-modul`
- [x] Initial commit made (Hello World scaffold)
- [x] Push to GitHub — branch fenn van: `feature/osszehasonlito-modul`
- [x] Pull Request #1 nyitva — https://github.com/balajthiboglarka/verziokovetes/pull/1
- [ ] PR jóváhagyása és merge → `main` (balajthiboglarka feladata)

---

## Module development

### 1. Controller
- [x] `ItemController.cs` átírva összehasonlítóvá
  - `Index()` — betölti a terméklistát service-en keresztül, JSON-ként átadja ViewBag-ben
  - Minden Hello World CRUD akció eltávolítva
  - ItemController név megmaradva → nincs .dnn manifeszt változás → nincs reinstall

### 2. Data / Service layer
- [x] `Services/HotcakesProductService.cs` létrehozva
  - SQL alapú lekérdezés (`System.Data.SqlClient`) — nincs HotCakes DLL reference szükséges
  - `GetAllProducts()` — termékek lekérése `hcc_Product` + `hcc_ProductTranslations` (hu-HU)
  - Kategória-hozzárendelések: `hcc_ProductXCategory` — utólag rendeli a termékekhez
  - Kép URL: `/Portals/0/Hotcakes/Data/products/{bvin}/small/{ImageFileSmall}`
  - Connection string: `SiteSqlServer` a DNN web.config-ból

### 3. Models
- [x] `Models/ProductViewModel.cs` létrehozva — { Id, Name, Price, ImageUrl, CategoryIds }

### 4. View
- [x] `Views/Item/Index.cshtml` átírva — teljes összehasonlító UI
  - Selector zóna: `<select>` + "Hozzáadás" gomb (4 kártyánál inaktívvá válik)
  - 4 kártyaslot: teli vagy üres (szaggatott) állapot
  - Ajánlások szekció: 2+ kiválasztott terméknél jelenik meg
  - jQuery logika: addProduct, removeProduct, renderCards, renderRecommendations, renderSelector
  - Ajánlás algoritmus: kategória egyezés +3pt, ±25% ár +1pt, top 4
  - XSS-biztos: escHtml() segédfüggvény
  - Ár formátum: "12 500 Ft" (magyar)

### 5. Styling
- [x] `module.css` megírva
  - Cream háttér (#f8f5f0), navy akcentus (#1a2060)
  - CSS Grid: 4 oszlop → 2 oszlop 900px-nél → 1 oszlop 500px-nél
  - Teli kártyák: solid kék keret; üres slotok: dashed szürke
  - X gomb: piros hoverre vált

### 6. Manifest (.dnn file)
- [x] Nem szükséges módosítás — `ItemController` neve és az `Item/Index.mvc` route megmaradt

### 7. Build javítások
- [x] `Controllers/ItemController.cs` — JavaScript névütközés javítva (teljesen minősített DNN namespace)
- [x] `Web.config` — assembly binding redirectek hozzáadva (Newtonsoft.Json, AngleSharp, System.Runtime.CompilerServices.Unsafe)

### 8. .csproj
- [x] `Ceramista.Dnn.HelloWorld.csproj` — `<Compile>` bejegyzések hozzáadva ProductViewModel.cs-hez és HotcakesProductService.cs-hez

---

## Build & deploy
- [x] Debug build — DLL fenn van `dnndev/bin/`-ben
- [x] Tesztelve: http://dnndev.me — alap funkciók működnek

---

## Feltolásra váró commitok

- [x] **Minden commit fenn van** — `feature/osszehasonlito-modul` origin szinkronban
  - [x] `3696363` — modell: IsFeatured mező hozzáadva a ProductViewModel-hez
  - [x] `34941fd` — adat: Featured mező kiolvasása a termék SQL lekérdezésből
  - [x] `8ff1a47` — funkció: ajánlott termékek oldalbetöltéskor - kiemelt termékek, fallback első 4

---

## Testing
- [x] Debug build lefutott — C# változások aktívak
- [x] Manual test: kereshető dropdown — gépelésre szűr, klikk kiválaszt, Hozzáadás aktívvá válik
- [x] Manual test: 1 termék hozzáadása → kártya megjelenik, 3 üres slot marad
- [x] Manual test: 4 termék hozzáadása → input + gomb inaktívvá válik
- [x] Manual test: X gomb → slot visszaáll placeholderré
- [x] Manual test: ajánlások megjelennek oldalbetöltéskor (featured/fallback)
- [x] Manual test: ajánlások frissülnek termék hozzáadásakor/eltávolításakor
- [x] Manual test: már kiválasztott termék nem szerepel az ajánlottak között

---

## Extra funkciók (opcionális — prezentációhoz)
- [ ] PDF/nyomtat export (CSS @media print alapú, könyvtár nélkül, ~2 óra)
- [ ] Mentett összehasonlítások (saját DB tábla + admin felület, ~3 óra) — kurzus "admin felület + DB tervezés" követelmény
- [ ] HotCakes kosár-integráció gomb (~3 óra)
