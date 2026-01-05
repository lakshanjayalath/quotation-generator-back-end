#!/usr/bin/env pwsh

$BaseUrl = "http://localhost:5264/api"

Write-Host "=== Testing Quotation Generator API ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login
Write-Host "1️⃣  STEP 1: Login as Admin" -ForegroundColor Yellow
$loginBody = @{
    email = "admin@example.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "$BaseUrl/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
    $loginJson = $loginResponse.Content | ConvertFrom-Json
    $token = $loginJson.token
    Write-Host "✅ Login successful!" -ForegroundColor Green
    Write-Host "Token obtained (first 50 chars): $($token.Substring(0, 50))..." -ForegroundColor Green
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Test GET /api/clients (should be empty initially)
Write-Host "2️⃣  STEP 2: Test GET /api/clients" -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $clientsResponse = Invoke-WebRequest -Uri "$BaseUrl/clients" -Method Get -Headers $headers
    $clientsJson = $clientsResponse.Content | ConvertFrom-Json
    Write-Host "✅ GET /api/clients successful!" -ForegroundColor Green
    Write-Host "Current clients count: $($clientsJson.Count)" -ForegroundColor Green
    $clientsJson | ConvertTo-Json | Write-Host
} catch {
    Write-Host "❌ GET /api/clients failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Step 3: Create a new client
Write-Host "3️⃣  STEP 3: Create a new client" -ForegroundColor Yellow
$newClientBody = @{
    clientName = "Test Client Company"
    clientIdNumber = "TC-001"
    clientContactNumber = "+94711234567"
    clientAddress = "123 Main Street, Colombo"
    clientEmail = "testclient@example.com"
    name = "Test Pvt Ltd"
    number = "TCO-123"
    group = "Group A"
    assignedUser = "admin@example.com"
    billingCity = "Colombo"
    billingCountry = "Sri Lanka"
    contacts = @(
        @{
            firstName = "John"
            lastName = "Doe"
            email = "john@testclient.com"
            phone = "+94771234567"
            addToInvoices = $true
        }
    )
} | ConvertTo-Json

try {
    $createResponse = Invoke-WebRequest -Uri "$BaseUrl/clients" -Method Post -Headers $headers -Body $newClientBody
    $createdClient = $createResponse.Content | ConvertFrom-Json
    Write-Host "✅ Client created successfully!" -ForegroundColor Green
    Write-Host "Client ID: $($createdClient.id)" -ForegroundColor Green
    Write-Host "Client Name: $($createdClient.clientName)" -ForegroundColor Green
    $global:clientId = $createdClient.id
} catch {
    Write-Host "❌ Create client failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response.Content)" -ForegroundColor Red
}

Write-Host ""

# Step 4: Test GET /api/clients again (should show the new client)
Write-Host "4️⃣  STEP 4: Test GET /api/clients (after creation)" -ForegroundColor Yellow
try {
    $clientsResponse2 = Invoke-WebRequest -Uri "$BaseUrl/clients" -Method Get -Headers $headers
    $clientsJson2 = $clientsResponse2.Content | ConvertFrom-Json
    Write-Host "✅ GET /api/clients successful!" -ForegroundColor Green
    Write-Host "Current clients count: $($clientsJson2.Count)" -ForegroundColor Green
    if ($clientsJson2.Count -gt 0) {
        Write-Host "Clients:" -ForegroundColor Green
        $clientsJson2 | ForEach-Object {
            Write-Host "  - $($_.name) ($($_.email))"
        }
    }
} catch {
    Write-Host "❌ GET /api/clients failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Step 5: Test GET /api/ActivityLogs/my-recent (check if creation activity is logged)
Write-Host "5️⃣  STEP 5: Check recent activity logs" -ForegroundColor Yellow
try {
    $activityResponse = Invoke-WebRequest -Uri "$BaseUrl/ActivityLogs/my-recent?limit=5" -Method Get -Headers $headers
    $activityJson = $activityResponse.Content | ConvertFrom-Json
    Write-Host "✅ GET /api/ActivityLogs/my-recent successful!" -ForegroundColor Green
    Write-Host "Recent activities count: $($activityJson.Count)" -ForegroundColor Green
    if ($activityJson.Count -gt 0) {
        Write-Host "Recent activities:" -ForegroundColor Green
        $activityJson | ForEach-Object {
            Write-Host "  - [$($_.actionType)] $($_.description) at $($_.timestamp)"
        }
    }
} catch {
    Write-Host "❌ GET /api/ActivityLogs/my-recent failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Step 6: Test GET /api/Dashboard/recent-activities
Write-Host "6️⃣  STEP 6: Check dashboard recent activities" -ForegroundColor Yellow
try {
    $dashboardResponse = Invoke-WebRequest -Uri "$BaseUrl/Dashboard/recent-activities?limit=5" -Method Get -Headers $headers
    $dashboardJson = $dashboardResponse.Content | ConvertFrom-Json
    Write-Host "✅ GET /api/Dashboard/recent-activities successful!" -ForegroundColor Green
    Write-Host "Dashboard activities count: $($dashboardJson.Count)" -ForegroundColor Green
    if ($dashboardJson.Count -gt 0) {
        Write-Host "Dashboard activities:" -ForegroundColor Green
        $dashboardJson | ForEach-Object {
            Write-Host "  - [$($_.actionType)] $($_.entityName) - $($_.description)"
        }
    }
} catch {
    Write-Host "❌ GET /api/Dashboard/recent-activities failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Testing Complete ===" -ForegroundColor Cyan
