# Futtatás.ps1
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "  Hotcakes Shop Manager - Egységtesztek Futtatása" -ForegroundColor Cyan
Write-Host "============================================================`n" -ForegroundColor Cyan

$totalTests = 0
$passedTests = 0

# Futtatás és élő feldolgozás
& dotnet test "HotcakesShopManager.Tests\HotcakesShopManager.Tests.csproj" --logger "console;verbosity=detailed" --nologo --RunConfiguration.MaxCpuCount=1 | ForEach-Object {
    $line = $_.Trim()
    
    if ($line -match "=== TESZTESET:") {
        Write-Host "`n$line" -ForegroundColor Yellow
        $totalTests++
    }
    elseif ($line -match "Bemenet:" -or $line -match "Folyamat:" -or $line -match "Elvárt kimenet:") {
        Write-Host "  $line"
    }
    elseif ($line -match "Eredmény: SIKERES") {
        Write-Host "  $line" -ForegroundColor Green
        $passedTests++
    }
}

Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "  ÖSSZESÍTŐ" -ForegroundColor Cyan
Write-Host "  Összes teszt: $totalTests"
Write-Host "  Sikeres:      $passedTests" -ForegroundColor Green
Write-Host "============================================================`n" -ForegroundColor Cyan

Write-Host "Nyomj Enter-t a kilépéshez..."
Read-Host
