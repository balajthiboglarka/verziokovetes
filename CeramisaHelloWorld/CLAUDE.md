# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project context

This is the **Ceramista product-comparison DNN module** — a university course project (Budapesti Corvinus Egyetem, "Rendszerfejlesztés és IT projektmenedzsment", Phase 3). The module is a DNN MVC extension that lets visitors compare 2–4 ceramic products side by side. It reads product data from HotCakes Commerce and requires no custom database tables.

The module was bootstrapped from the DNN 8/9 C# DAL2 MVC Module template. The Hello World scaffolding (Item CRUD, ItemManager, the `Ceramista.Dnn.HelloWorld_Items` table) will be replaced with comparison logic — do not add to or extend that scaffolding.

---

## Environment

| Item | Value |
|------|-------|
| DNN root | `C:\inetpub\wwwroot\dnndev\` |
| Module folder | `DesktopModules\module-helloworld\` |
| Installed at (DNN) | `DesktopModules\MVC\Ceramista.Dnn.HelloWorld` |
| Local dev URL | `http://dnndev.me` |
| Public URL | `http://40.67.241.74` (Azure VM — only reachable when VM is running) |
| Framework | .NET 4.7.2, ASP.NET MVC 5, DNN 9.13 |
| HotCakes DLLs | `C:\inetpub\wwwroot\dnndev\bin\Hotcakes.Commerce.dll`, `Hotcakes.Commerce.Dnn.dll` |

---

## Build & deploy cycle

**This is not a standard `dotnet build` project.** Build is done through Visual Studio 2022 (run as Administrator).

### Debug build (fast iteration — C# changes only)
1. Build → **Debug** configuration in Visual Studio.
2. The DLL is output directly to `C:\inetpub\wwwroot\dnndev\bin\`.
3. IIS detects the new DLL and restarts the app pool automatically.
4. For `.cshtml`, `.css`, `.js` changes: just refresh the browser — no rebuild needed.

### Release build (required for structural changes)
1. Build → **Release** configuration in Visual Studio.
2. This triggers `BuildScripts\ModulePackage.targets`, which:
   - Packages the module into `install\Ceramista.Dnn.HelloWorld_XX.XX.XX_Install.zip`
3. Go to DNN Admin → Extensions → find the module → Upload/Install the ZIP.
4. **Required after any change to:** the `.dnn` manifest, `SqlDataProvider` scripts, or new controller routes.

### When to reinstall vs. just rebuild
| Change type | Action needed |
|---|---|
| C# logic | Debug build only |
| `.cshtml` / `.css` / `.js` | Browser refresh only |
| New MVC route (`.dnn` file) | Release build + reinstall ZIP |
| SQL schema change | Release build + reinstall ZIP |

### Debugging
Standard VS debugger attach does not work inside DNN. To break into the debugger from code:
```csharp
System.Diagnostics.Debugger.Launch();
```
This prompts you to attach Visual Studio when the code path is hit.

### Common build problems
- **"Application Error" after build:** IIS created a virtual directory for the module — delete it in IIS Manager under the DNN site.
- **"Does not compile" / missing DotNetNuke.dll:** Check `HintPath` in the `.csproj` — all DNN assembly references must be relative paths pointing to `..\..\..\.bin\`.
- **Changes not visible:** Stop and Start the DNN website in IIS Manager.

---

## Architecture

### DNN MVC module structure
DNN calls what we call a "module" an "extension". This extension contains one module definition registered in `Ceramista.Dnn.HelloWorld.dnn`.

```
Controllers/          ← DnnController subclasses; one per logical area
  ItemController.cs   ← (scaffold — will be replaced by ComparisonController)
  SettingsController.cs
Models/               ← Plain C# model classes; POCOs passed to Views
Views/
  Item/               ← Razor views matched to controller/action by convention
    Index.cshtml      ← Entry point rendered when module is placed on a DNN page
  Shared/
    _Layout.cshtml    ← Wraps all views in <div id="mvcContainer-{moduleId}">
Components/
  FeatureController.cs  ← Registered as BusinessController in .dnn; hooks DNN lifecycle
  ItemManager.cs        ← DAL2 data access (scaffold — not used in comparison module)
Providers/DataProviders/SqlDataProvider/
  00.00.01.SqlDataProvider  ← Runs on install; DDL for any custom tables
  Uninstall.SqlDataProvider ← Runs on uninstall; drops custom tables
```

### How DNN routes to the module
The `.dnn` manifest maps URL segments to controller actions:
```xml
<controlSrc>Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Controllers/Item/Index.mvc</controlSrc>
```
Format: `{RootNamespace}.Controllers/{ControllerName}/{ActionName}.mvc`

The default (empty `controlKey`) maps to `Index` and is what renders when the module is placed on a page.

### How the comparison module fetches product data
The module must **not** use a custom database table. It reads from HotCakes using the `HccApp` API (preferred) or falls back to direct SQL against `hcc_Product` and `hcc_ProductInventory`.

**HotCakes API approach (preferred):**
```csharp
// Reference: Hotcakes.Commerce.Dnn.dll + Hotcakes.Commerce.dll (already in DNN bin)
var hccApp = Hotcakes.Commerce.Dnn.DnnAppFactory.Create();
var products = hccApp.CatalogServices.Products.FindAll();
```
Add a reference to `Hotcakes.Commerce.Dnn.dll` and `Hotcakes.Commerce.dll` in the `.csproj` using relative `HintPath` pointing to `..\..\..\.bin\`.

**SQL fallback:**
Query `hcc_Product` (columns: `Bvin` as ID, `ProductName`, `SitePrice`, `ImageFileSmall`) joined to `hcc_ProductInventory` for stock status. Use `DotNetNuke.Data.DataContext` or `System.Data.SqlClient` with the DNN connection string from `web.config`.

### Returning JSON from a DNN MVC controller
For the comparison module, the `Index` action renders the Razor view; a separate action returns product data as JSON for jQuery calls:

```csharp
public ActionResult GetProducts()
{
    // ... fetch from HotCakes
    return Json(products, JsonRequestBehavior.AllowGet);
}
```
Register this route in the `.dnn` manifest with a unique `controlKey`.

### Registering JS/CSS in Razor views
Do not use plain `<script>` or `<link>` tags. Use the DNN client resource manager so DNN can deduplicate and order them:
```csharp
@using DotNetNuke.Web.Client.ClientResourceManagement
@{ ClientResourceManager.RegisterScript(Dnn.DnnPage, "~/DesktopModules/MVC/Ceramista.Dnn.HelloWorld/module.js"); }
```

---

## Module UI specification

The comparison module UI has three vertical zones (Figma mockup: `C:\inetpub\Figma\figma.png`):

1. **Product selector zone** — full-width search/dropdown (`select2`-style) + "Hozzáadás" button. Both go inactive (greyed) when 4 cards are already selected.
2. **Comparison area** — always shows 4 dashed-border card slots. Filled cards show: product image, name (bold, max 2 lines), price (format: `12 500 Ft`), and an X button (top-left) to remove. Empty slots remain as dashed placeholders.
3. **Recommendations section** — appears only when 2+ products are selected. Shows up to 4 cards with image, name, and "Hozzáadás" button. Recommendation logic: prioritise products in the same category as any selected product (higher weight), then products within ±25% price range of selected products (lower weight). Already-selected products are excluded.

Color scheme (match existing site): cream/beige background, dark navy (`#1a2060` approx.) for text, buttons, and borders.

---

## Version control

**Repository:** `https://github.com/balajthiboglarka/verziokovetes`  
**Strategy:** Feature branching — all work on a `feature/...` branch; PRs reviewed by `balajthiboglarka` (admin) before merge to `main`.

Step-by-step Git setup guide: `C:\inetpub\Context\github_setup_guide.md`

```bash
# Start any new piece of work
git checkout main && git pull origin main
git checkout -b feature/your-feature-name

# Save progress
git add .
git commit -m "feat: short description"
git push origin feature/your-feature-name
# Then open a Pull Request on GitHub for admin review
```

---

## DNN-specific gotchas

- **Namespace collision:** The project's `RootNamespace` in `.csproj` is `Ceramista.Dnn.Ceramista.Dnn.HelloWorld` (double-prefixed from the template). All `using` statements and `.dnn` `controlSrc` entries must use this full namespace.
- **No standalone debugging:** The module runs inside the DNN process; there is no way to `F5`-run it independently.
- **IIS restart is not graceful:** Editing any C# file triggers an app pool recycle. Visitors see a brief delay on the next request.
- **Structural changes need reinstall:** If you change the `.dnn` manifest (e.g. add a new route) or add a `SqlDataProvider` script, a full Release build + ZIP reinstall through DNN Admin is required — a Debug build alone will not apply the change.
