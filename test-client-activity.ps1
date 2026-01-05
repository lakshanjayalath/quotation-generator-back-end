#!/usr/bin/env pwsh

$BaseUrl = "http://localhost:5264/api"

Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "       Testing Client Creation & Activity Tracking" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login
Write-Host "📝 Step 1: Authenticate Admin User" -ForegroundColor Yellow
$loginBody = @{
    email = "admin@example.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-WebRequest -Uri "$BaseUrl/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
$loginJson = $loginResponse.Content | ConvertFrom-Json
$token = $loginJson.token

Write-Host "✅ Logged in as: $($loginJson.user.email)" -ForegroundColor Green
Write-Host ""

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Step 2: Get initial clients count
Write-Host "📝 Step 2: Get Initial Clients (before creation)" -ForegroundColor Yellow
$clientsBefore = Invoke-WebRequest -Uri "$BaseUrl/clients" -Method Get -Headers $headers
$clientsBeforeJson = $clientsBefore.Content | ConvertFrom-Json
Write-Host "Current clients count: $($clientsBeforeJson.Count)" -ForegroundColor Green
Write-Host ""

# Step 3: Create a new client
Write-Host "📝 Step 3: Create New Client" -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$newClientBody = @{
    clientName = "New Client $timestamp"
    clientIdNumber = "NC-$timestamp"
    clientContactNumber = "+94711234567"
    clientAddress = "123 Main Street, Colombo"
    clientEmail = "newclient$timestamp@example.com"
    name = "New Client Pvt Ltd"
    number = "NC-123"
    group = "Group A"
    billingCity = "Colombo"
    billingCountry = "Sri Lanka"
    contacts = @(
        @{
            firstName = "John"
            lastName = "Doe"
            email = "john@newclient.com"
            phone = "+94771234567"
            addToInvoices = $true
        }
    )
} | ConvertTo-Json

$createResponse = Invoke-WebRequest -Uri "$BaseUrl/clients" -Method Post -Headers $headers -Body $newClientBody
$createdClient = $createResponse.Content | ConvertFrom-Json

Write-Host "✅ Client created!" -ForegroundColor Green
Write-Host "   - Client ID: $($createdClient.id)" -ForegroundColor Green
Write-Host "   - Client Name: $($createdClient.clientName)" -ForegroundColor Green
Write-Host "   - Assigned User: $($createdClient.assignedUser)" -ForegroundColor Green
Write-Host ""

# Step 4: Verify client appears in GET /api/clients
Write-Host "📝 Step 4: Verify Client in Clients List" -ForegroundColor Yellow
$clientsAfter = Invoke-WebRequest -Uri "$BaseUrl/clients" -Method Get -Headers $headers
$clientsAfterJson = $clientsAfter.Content | ConvertFrom-Json
Write-Host "Clients after creation: $($clientsAfterJson.Count)" -ForegroundColor Green
$newClientFromList = $clientsAfterJson | Where-Object { $_.clientId -eq $createdClient.id }
if ($newClientFromList) {
    Write-Host "✅ New client found in list!" -ForegroundColor Green
    Write-Host "   - Name: $($newClientFromList.name)" -ForegroundColor Green
    Write-Host "   - Email: $($newClientFromList.email)" -ForegroundColor Green
} else {
    Write-Host "❌ New client NOT found in list!" -ForegroundColor Red
}
Write-Host ""

# Step 5: Check recent activities
Write-Host "📝 Step 5: Check Recent Activities (my-recent)" -ForegroundColor Yellow
$activityResponse = Invoke-WebRequest -Uri "$BaseUrl/ActivityLogs/my-recent?limit=10" -Method Get -Headers $headers
$activityJson = $activityResponse.Content | ConvertFrom-Json
Write-Host "Recent activities count: $($activityJson.Count)" -ForegroundColor Green
if ($activityJson.Count -gt 0) {
    Write-Host "Recent activities:" -ForegroundColor Green
    $activityJson | Select-Object -First 3 | ForEach-Object {
        Write-Host "   [$($_.actionType)] $($_.entityName) #$($_.recordId) - $($_.description) at $($_.timestamp)" -ForegroundColor Green
    }
    
    # Check if client creation is in activities
    $clientCreationLog = $activityJson | Where-Object { $_.actionType -eq "Create" -and $_.entityName -eq "Client" -and $_.recordId -eq $createdClient.id }
    if ($clientCreationLog) {
        Write-Host "✅ Client creation logged in activities!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Client creation not found in recent activities (might be beyond limit)" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ No recent activities found!" -ForegroundColor Red
}
Write-Host ""

# Step 6: Check dashboard recent clients
Write-Host "📝 Step 6: Check Dashboard Recent Clients" -ForegroundColor Yellow
$dashboardClientsResponse = Invoke-WebRequest -Uri "$BaseUrl/Dashboard/recent-clients?limit=10" -Method Get -Headers $headers
$dashboardClientsJson = $dashboardClientsResponse.Content | ConvertFrom-Json
Write-Host "Dashboard recent clients count: $($dashboardClientsJson.Count)" -ForegroundColor Green
if ($dashboardClientsJson.Count -gt 0) {
    Write-Host "Recent clients from dashboard:" -ForegroundColor Green
    $dashboardClientsJson | Select-Object -First 5 | ForEach-Object {
        Write-Host "   - $($_.clientName) ($($_.clientEmail)) on $($_.createdDate)" -ForegroundColor Green
    }
    
    # Check if newly created client is in dashboard
    $newClientInDashboard = $dashboardClientsJson | Where-Object { $_.id -eq $createdClient.id }
    if ($newClientInDashboard) {
        Write-Host "✅ New client visible in dashboard!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  New client not found in dashboard recent clients" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ No dashboard recent clients found!" -ForegroundColor Red
}
Write-Host ""

# Step 7: Check dashboard recent activities
Write-Host "📝 Step 7: Check Dashboard Recent Activities" -ForegroundColor Yellow
$dashboardActivitiesResponse = Invoke-WebRequest -Uri "$BaseUrl/Dashboard/recent-activities?limit=10" -Method Get -Headers $headers
$dashboardActivitiesJson = $dashboardActivitiesResponse.Content | ConvertFrom-Json
Write-Host "Dashboard recent activities count: $($dashboardActivitiesJson.Count)" -ForegroundColor Green
if ($dashboardActivitiesJson.Count -gt 0) {
    Write-Host "Recent activities from dashboard:" -ForegroundColor Green
    $dashboardActivitiesJson | Select-Object -First 5 | ForEach-Object {
        Write-Host "   [$($_.actionType)] $($_.entityName) - $($_.description)" -ForegroundColor Green
    }
    
    # Check if client creation is in dashboard activities
    $clientCreationInDashboard = $dashboardActivitiesJson | Where-Object { $_.actionType -eq "Create" -and $_.entityName -eq "Client" -and $_.recordId -eq $createdClient.id }
    if ($clientCreationInDashboard) {
        Write-Host "✅ Client creation visible in dashboard!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Client creation not found in dashboard activities (might be beyond limit)" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ No dashboard recent activities found!" -ForegroundColor Red
}
Write-Host ""

Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "                     Test Complete!" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Cyan
