@echo off
chcp 65001 > nul
echo.
echo ============================================================
echo   Hotcakes Shop Manager - Egységtesztek Futtatása
echo ============================================================
echo.

dotnet test "HotcakesShopManager.Tests\HotcakesShopManager.Tests.csproj" --logger "console;verbosity=detailed" --nologo

echo.
echo ============================================================
echo.
pause
