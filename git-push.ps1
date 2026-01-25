# Git Push Script - Run this in PowerShell
# This script will clean up git state, restore critical files, and push to GitHub

Write-Host "🔧 Cleaning up Git state..." -ForegroundColor Cyan

cd "C:\Users\ruchi\OneDrive\Desktop\my_project\ruchika-portfolio"

# Stop any processes that might lock files
Write-Host "Stopping processes that might lock files..." -ForegroundColor Yellow
Get-Process -Name "dotnet","git","node" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Remove lock files with retry
$maxRetries = 5
for ($i = 1; $i -le $maxRetries; $i++) {
    try {
        Remove-Item -Path ".git\index.lock" -Force -ErrorAction Stop
        Write-Host "✅ Removed index.lock" -ForegroundColor Green
        break
    } catch {
        if ($i -lt $maxRetries) {
            Write-Host "⚠️ Attempt $i failed, retrying..." -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        } else {
            Write-Host "❌ Could not remove lock file. Please close Cursor/VSCode and try again." -ForegroundColor Red
            exit 1
        }
    }
}

# Clean up rebase state
Write-Host "Cleaning up rebase state..." -ForegroundColor Yellow
Remove-Item -Path ".git\rebase-merge" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ".git\rebase-apply" -Recurse -Force -ErrorAction SilentlyContinue

# Abort any ongoing rebase
Write-Host "Aborting rebase..." -ForegroundColor Yellow
git rebase --abort 2>$null

# Check status
Write-Host "`n📊 Current Git Status:" -ForegroundColor Cyan
git status

Write-Host "`n✅ Git state cleaned. Now proceeding with add, commit, and push..." -ForegroundColor Green

# Restore critical deleted files first
Write-Host "`n🔧 Restoring critical files..." -ForegroundColor Cyan

# Restore launchSettings.json
$propsDir = "backend\ServiceHubPro.API\Properties"
if (-not (Test-Path $propsDir)) {
    New-Item -ItemType Directory -Path $propsDir -Force | Out-Null
}

$launchSettings = @'
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:1671",
      "sslPort": 44322
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:7000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7001;http://localhost:7000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
'@

$launchSettings | Out-File -FilePath "$propsDir\launchSettings.json" -Encoding UTF8 -Force
Write-Host "✅ Restored launchSettings.json" -ForegroundColor Green

# Restore TaxRateHelper.cs
$helpersDir = "backend\ServiceHubPro.Application\Common\Helpers"
if (-not (Test-Path $helpersDir)) {
    New-Item -ItemType Directory -Path $helpersDir -Force | Out-Null
}

$taxRateHelper = @'
namespace ServiceHubPro.Application.Common.Helpers;

/// <summary>
/// Helper class for tax rate calculations.
/// Tax rates are stored as percent (0-100), e.g., 8 for 8%.
/// </summary>
public static class TaxRateHelper
{
    /// <summary>
    /// Converts tax rate from percent (0-100) to decimal fraction (0-1).
    /// Example: 8 (percent) -> 0.08 (decimal fraction)
    /// </summary>
    public static decimal GetDecimalFraction(decimal taxRatePercent)
    {
        return taxRatePercent / 100m;
    }

    /// <summary>
    /// Calculates the total amount for an item including tax.
    /// Formula: (quantity * unitPrice) * (1 + taxRate/100)
    /// </summary>
    public static decimal CalculateItemTotal(decimal quantity, decimal unitPrice, decimal taxRatePercent)
    {
        var taxDecimal = GetDecimalFraction(taxRatePercent);
        return quantity * unitPrice * (1m + taxDecimal);
    }

    /// <summary>
    /// Calculates the tax amount for an item.
    /// Formula: (quantity * unitPrice) * (taxRate/100)
    /// </summary>
    public static decimal CalculateTaxAmount(decimal quantity, decimal unitPrice, decimal taxRatePercent)
    {
        var taxDecimal = GetDecimalFraction(taxRatePercent);
        return quantity * unitPrice * taxDecimal;
    }
}
'@

$taxRateHelper | Out-File -FilePath "$helpersDir\TaxRateHelper.cs" -Encoding UTF8 -Force
Write-Host "✅ Restored TaxRateHelper.cs" -ForegroundColor Green

# Add all changes
Write-Host "`n📦 Adding all changes..." -ForegroundColor Cyan
git add -A

# Check what will be committed
Write-Host "`n📋 Changes to be committed:" -ForegroundColor Cyan
git status --short

# Commit changes
Write-Host "`n💾 Committing changes..." -ForegroundColor Cyan
$commitMessage = "Update project documentation and restore critical files

- Updated PROJECT_README.md with comprehensive setup instructions
- Restored launchSettings.json with correct port configuration
- Restored TaxRateHelper.cs for tax calculations
- Cleaned up temporary documentation files
- Added author attribution (Ruchika Rajodiya)"

git commit -m $commitMessage

# Check current branch
$currentBranch = git branch --show-current
Write-Host "`n🌿 Current branch: $currentBranch" -ForegroundColor Cyan

# Push to origin
Write-Host "`n🚀 Pushing to GitHub..." -ForegroundColor Cyan
try {
    git push -u origin $currentBranch
    Write-Host "`n✅ Successfully pushed to GitHub!" -ForegroundColor Green
} catch {
    Write-Host "`n❌ Push failed. Error: $_" -ForegroundColor Red
    Write-Host "`nTrying to pull first and then push..." -ForegroundColor Yellow
    
    # If push fails due to divergence, try pull with rebase
    git pull --rebase origin $currentBranch
    git push -u origin $currentBranch
}

Write-Host "`n✅ Done!" -ForegroundColor Green
