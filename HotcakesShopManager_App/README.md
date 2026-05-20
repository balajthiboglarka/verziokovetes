# Hotcakes Shop Manager

Ez egy Windows Forms alapú asztali alkalmazás, amely a Hotcakes Commerce API-t használja a webshop termékeinek, kategóriáinak és rendeléseinek kezeléséhez.

## Főbb funkciók

- **Terméklista:** Az összes termék áttekintése árakkal és készletinformációkkal.
- **Tömeges sorolás:** Termékek gyors hozzárendelése kategóriákhoz.
- **Kép gyorsnézet:** Termékképek aszinkron betöltése és megtekintése.
- **Leltár jelentések:** Készletállapot és rendelési statisztikák vizualizációja.

## Fejlesztői környezet

- **Keretrendszer:** .NET Framework 4.8
- **UI:** WinForms (SkiaSharp támogatással a grafikai elemekhez)
- **API:** Hotcakes Commerce DTO v1
- **Tesztek:** xUnit és Moq

## Használat

1. Nyisd meg a `HotcakesShopManager.sln` fájlt Visual Studio-ban.
2. Ellenőrizd a NuGet csomagok meglétét (SkiaSharp, Newtonsoft.Json).
3. A `MainForm.cs`-ben állítsd be a saját API URL-edet és kulcsodat.
4. Indítsd el az alkalmazást (F5).

## Tesztelés

A tesztek futtatásához használhatod a mellékelt `Tesztek_Futtatása.bat` vagy `Tesztek_Futtatása.ps1` szkripteket, vagy a Visual Studio Test Explorer-ét.
