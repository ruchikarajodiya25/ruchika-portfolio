# Clean EF Migration Reset Script
# Removes TenantId1 shadow FK and ensures PurchaseOrders FKs are NoAction
# Run from backend/ folder

Write-Host "=== STEP 1: Drop LocalDB Database ===" -ForegroundColor Cyan
dotnet ef database drop --force `
    --project ./ServiceHubPro.Infrastructure/ServiceHubPro.Infrastructure.csproj `
    --startup-project ./ServiceHubPro.API/ServiceHubPro.API.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Host "Warning: Database drop may have failed (database might not exist)" -ForegroundColor Yellow
}

Write-Host "`n=== STEP 2: Delete Migrations Folder ===" -ForegroundColor Cyan
if (Test-Path "./ServiceHubPro.Infrastructure/Migrations") {
    Remove-Item -Path "./ServiceHubPro.Infrastructure/Migrations" -Recurse -Force
    Write-Host "Migrations folder deleted successfully" -ForegroundColor Green
} else {
    Write-Host "Migrations folder does not exist" -ForegroundColor Yellow
}

Write-Host "`n=== STEP 3: Create New Migration ===" -ForegroundColor Cyan
dotnet ef migrations add InitialCreate `
    --project ./ServiceHubPro.Infrastructure/ServiceHubPro.Infrastructure.csproj `
    --startup-project ./ServiceHubPro.API/ServiceHubPro.API.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Migration creation failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== STEP 4: Update Database ===" -ForegroundColor Cyan
dotnet ef database update `
    --project ./ServiceHubPro.Infrastructure/ServiceHubPro.Infrastructure.csproj `
    --startup-project ./ServiceHubPro.API/ServiceHubPro.API.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Database update failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== STEP 5: Verify TenantId1 Does NOT Exist ===" -ForegroundColor Cyan
$tenantId1Found = $false

# Check all migration .cs files
$migrationFiles = Get-ChildItem -Path "./ServiceHubPro.Infrastructure/Migrations/*.cs" -Exclude "*ModelSnapshot.cs"
foreach ($file in $migrationFiles) {
    $matches = Select-String -Path $file.FullName -Pattern "TenantId1" -Quiet
    if ($matches) {
        Write-Host "ERROR: Found TenantId1 in $($file.Name)" -ForegroundColor Red
        $tenantId1Found = $true
    }
}

# Check snapshot file
$snapshotFile = "./ServiceHubPro.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs"
if (Test-Path $snapshotFile) {
    $matches = Select-String -Path $snapshotFile -Pattern "TenantId1" -Quiet
    if ($matches) {
        Write-Host "ERROR: Found TenantId1 in ApplicationDbContextModelSnapshot.cs" -ForegroundColor Red
        $tenantId1Found = $true
    }
}

if (-not $tenantId1Found) {
    Write-Host "SUCCESS: No TenantId1 found in migrations or snapshot" -ForegroundColor Green
} else {
    Write-Host "FAILED: TenantId1 still exists in migration files!" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== STEP 6: Verify PurchaseOrders FKs are NoAction ===" -ForegroundColor Cyan
$migrationFile = Get-ChildItem -Path "./ServiceHubPro.Infrastructure/Migrations/*InitialCreate.cs" | Select-Object -First 1

if ($migrationFile) {
    $content = Get-Content $migrationFile.FullName -Raw
    
    # Check PurchaseOrders table creation
    if ($content -match "CreateTable\(`"PurchaseOrders`"") {
        # Extract the PurchaseOrders table section
        $purchaseOrdersSection = $content -replace '(?s).*CreateTable\(`"PurchaseOrders`"(.*?)CreateTable\(`".*?`"|.*', '$1'
        
        # Check for TenantId FK
        if ($purchaseOrdersSection -match "column:\s*`"TenantId`".*?onDelete:\s*ReferentialAction\.(NoAction|Restrict)") {
            Write-Host "SUCCESS: PurchaseOrders.TenantId FK is NoAction/Restrict" -ForegroundColor Green
        } elseif ($purchaseOrdersSection -match "column:\s*`"TenantId`".*?onDelete:\s*ReferentialAction\.Cascade") {
            Write-Host "ERROR: PurchaseOrders.TenantId FK is CASCADE (should be NoAction)" -ForegroundColor Red
            exit 1
        }
        
        # Check for VendorId FK
        if ($purchaseOrdersSection -match "column:\s*`"VendorId`".*?onDelete:\s*ReferentialAction\.(NoAction|Restrict)") {
            Write-Host "SUCCESS: PurchaseOrders.VendorId FK is NoAction/Restrict" -ForegroundColor Green
        } elseif ($purchaseOrdersSection -match "column:\s*`"VendorId`".*?onDelete:\s*ReferentialAction\.Cascade") {
            Write-Host "ERROR: PurchaseOrders.VendorId FK is CASCADE (should be NoAction)" -ForegroundColor Red
            exit 1
        }
        
        # Check for LocationId FK
        if ($purchaseOrdersSection -match "column:\s*`"LocationId`".*?onDelete:\s*ReferentialAction\.(NoAction|Restrict)") {
            Write-Host "SUCCESS: PurchaseOrders.LocationId FK is NoAction/Restrict" -ForegroundColor Green
        } elseif ($purchaseOrdersSection -match "column:\s*`"LocationId`".*?onDelete:\s*ReferentialAction\.Cascade") {
            Write-Host "ERROR: PurchaseOrders.LocationId FK is CASCADE (should be NoAction)" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "WARNING: Could not find PurchaseOrders table in migration" -ForegroundColor Yellow
    }
    
    # Also check foreign key constraints section
    $fkMatches = Select-String -Path $migrationFile.FullName -Pattern "FK_PurchaseOrders.*?onDelete:\s*ReferentialAction\.(NoAction|Restrict|Cascade)" -AllMatches
    if ($fkMatches) {
        foreach ($match in $fkMatches.Matches) {
            if ($match.Value -match "Cascade") {
                Write-Host "ERROR: Found CASCADE in PurchaseOrders FK: $($match.Value)" -ForegroundColor Red
                exit 1
            }
        }
        Write-Host "SUCCESS: All PurchaseOrders FKs verified as NoAction/Restrict" -ForegroundColor Green
    }
}

Write-Host "`n=== Migration Reset Complete ===" -ForegroundColor Green
Write-Host "All verifications passed!" -ForegroundColor Green
