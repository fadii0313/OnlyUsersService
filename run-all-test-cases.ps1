param (
    [string]$BaseUrl = "http://localhost:5200"
)

$results = [System.Collections.Generic.List[PSObject]]::new()

function Record-Test {
    param (
        [string]$Id,
        [string]$Category,
        [string]$Endpoint,
        [string]$Method,
        [string]$Description,
        [int]$ExpectedStatus,
        [int]$ActualStatus,
        [bool]$Success,
        [string]$Details
    )

    $statusStr = if ($Success) { "PASS" } else { "FAIL" }
    $color = if ($Success) { "Green" } else { "Red" }
    Write-Host "[$statusStr] $Id | $Category | $Method $Endpoint -> $Details" -ForegroundColor $color

    $obj = [PSCustomObject]@{
        CaseId = $Id
        Category = $Category
        Method = $Method
        Endpoint = $Endpoint
        Description = $Description
        ExpectedStatus = $ExpectedStatus
        ActualStatus = $ActualStatus
        Result = $statusStr
        Details = $Details
    }
    $results.Add($obj)
}

Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "         OnlyUsersService Comprehensive Test Suite Execution" -ForegroundColor Cyan
Write-Host "         Target Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "================================================================================`n" -ForegroundColor Cyan

# Case 01: Swagger JSON
try {
    $sw = Invoke-RestMethod -Uri "$BaseUrl/swagger/v1/swagger.json" -Method Get
    $pass = ($sw.info.title -eq "OnlyUsersService API")
    Record-Test "TC-01" "OpenAPI/Docs" "/swagger/v1/swagger.json" "GET" "Verify Swagger OpenAPI v3 spec" 200 200 $pass "Title: $($sw.info.title), Version: $($sw.info.version)"
} catch {
    Record-Test "TC-01" "OpenAPI/Docs" "/swagger/v1/swagger.json" "GET" "Verify Swagger OpenAPI v3 spec" 200 500 $false $_.Exception.Message
}

# Case 02: Static Dashboard
try {
    $spa = Invoke-WebRequest -Uri "$BaseUrl/index.html" -Method Get -UseBasicParsing
    $pass = ($spa.StatusCode -eq 200 -and $spa.Content -match "OnlyUsersService")
    Record-Test "TC-02" "Web UI" "/index.html" "GET" "Verify Embedded SPA Dashboard is served" 200 $spa.StatusCode $pass "Length: $($spa.RawContentLength) bytes"
} catch {
    Record-Test "TC-02" "Web UI" "/index.html" "GET" "Verify Embedded SPA Dashboard is served" 200 500 $false $_.Exception.Message
}

# Case 03: SHA-256 Hashing
try {
    $body = @{ text = "Antigravity2026" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/hash" -Method Post -ContentType "application/json" -Body $body
    $pass = ($res.success -and $res.data.sha256Hash.Length -eq 64)
    Record-Test "TC-03" "Cryptography" "/api/users/hash" "POST" "Compute raw SHA-256 hash" 200 200 $pass "Hash: $($res.data.sha256Hash.Substring(0,16))..."
} catch {
    Record-Test "TC-03" "Cryptography" "/api/users/hash" "POST" "Compute raw SHA-256 hash" 200 500 $false $_.Exception.Message
}

# Case 04: SHA-256 Salted Hashing
try {
    $body = @{ text = "Antigravity2026"; salt = "SecureSalt!99" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/encrypt" -Method Post -ContentType "application/json" -Body $body
    $pass = ($res.success -and $res.data.sha256Hash.Length -eq 64)
    Record-Test "TC-04" "Cryptography" "/api/users/encrypt" "POST" "Compute salted SHA-256 hash" 200 200 $pass "Salted Hash: $($res.data.sha256Hash.Substring(0,16))..."
} catch {
    Record-Test "TC-04" "Cryptography" "/api/users/encrypt" "POST" "Compute salted SHA-256 hash" 200 500 $false $_.Exception.Message
}

# Case 05: User Registration
$dynamicUser = "user_" + (Get-Random -Minimum 10000 -Maximum 99999)
$regToken = ""
$regUserId = 0
try {
    $body = @{
        username = $dynamicUser
        email = "$dynamicUser@onlyusers.local"
        password = "UserPass@123"
        firstName = "Dynamic"
        lastName = "Tester"
        phoneNumber = "+1-555-0999"
        department = "R&D"
        designation = "Security Analyst"
        city = "New York"
        country = "USA"
    } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/register" -Method Post -ContentType "application/json" -Body $body
    $regUserId = $res.data.userId
    $regToken = $res.data.token
    $pass = ($res.success -and $regUserId -gt 0 -and $res.data.roles -contains "User")
    Record-Test "TC-05" "Registration" "/api/users/register" "POST" "Self-register account with SHA-256" 200 200 $pass "Created User ID: $regUserId, Roles: $($res.data.roles -join ',')"
} catch {
    Record-Test "TC-05" "Registration" "/api/users/register" "POST" "Self-register account with SHA-256" 200 500 $false $_.Exception.Message
}

# Case 06: Duplicate Username Validation
try {
    $body = @{
        username = $dynamicUser
        email = "other_$dynamicUser@onlyusers.local"
        password = "UserPass@123"
        firstName = "Duplicate"
        lastName = "User"
    } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/register" -Method Post -ContentType "application/json" -Body $body
    Record-Test "TC-06" "Validation" "/api/users/register" "POST" "Reject duplicate username" 400 200 $false "Expected rejection"
} catch {
    $status = [int]$_.Exception.Response.StatusCode
    $pass = ($status -eq 400)
    Record-Test "TC-06" "Validation" "/api/users/register" "POST" "Reject duplicate username" 400 $status $pass "Properly returned HTTP 400 Bad Request"
}

# Case 07: Invalid Email Validation
try {
    $body = @{
        username = "valid_user_" + (Get-Random -Minimum 1000 -Maximum 9999)
        email = "invalid-email-format"
        password = "UserPass@123"
        firstName = "Test"
        lastName = "User"
    } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/register" -Method Post -ContentType "application/json" -Body $body
    Record-Test "TC-07" "Validation" "/api/users/register" "POST" "Reject invalid email format" 400 200 $false "Expected rejection"
} catch {
    $status = [int]$_.Exception.Response.StatusCode
    $pass = ($status -eq 400)
    Record-Test "TC-07" "Validation" "/api/users/register" "POST" "Reject invalid email format" 400 $status $pass "Properly rejected malformed email"
}

# Case 08: Admin Authentication
$adminToken = ""
try {
    $body = @{ username = "admin"; password = "Admin@123456" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/login" -Method Post -ContentType "application/json" -Body $body
    $adminToken = $res.data.token
    $pass = ($res.success -and $res.data.roles -contains "Admin" -and $adminToken.Length -gt 50)
    Record-Test "TC-08" "Authentication" "/api/users/login" "POST" "Admin login & JWT issuance" 200 200 $pass "Token issued, Role: Admin, Exp: $($res.data.expiresAt)"
} catch {
    Record-Test "TC-08" "Authentication" "/api/users/login" "POST" "Admin login & JWT issuance" 200 500 $false $_.Exception.Message
}

# Case 09: Standard User Authentication
$userToken = ""
try {
    $body = @{ username = "john_doe"; password = "User@123456" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/login" -Method Post -ContentType "application/json" -Body $body
    $userToken = $res.data.token
    $pass = ($res.success -and $res.data.roles -contains "User" -and $userToken.Length -gt 50)
    Record-Test "TC-09" "Authentication" "/api/users/login" "POST" "User login & JWT issuance" 200 200 $pass "Token issued, Role: User, Username: john_doe"
} catch {
    Record-Test "TC-09" "Authentication" "/api/users/login" "POST" "User login & JWT issuance" 200 500 $false $_.Exception.Message
}

# Case 10: Failed Login (Wrong Password)
try {
    $body = @{ username = "admin"; password = "IncorrectPassword999!" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/login" -Method Post -ContentType "application/json" -Body $body
    Record-Test "TC-10" "Authentication" "/api/users/login" "POST" "Reject incorrect password" 401 200 $false "Expected rejection"
} catch {
    $status = [int]$_.Exception.Response.StatusCode
    $pass = ($status -eq 401)
    Record-Test "TC-10" "Authentication" "/api/users/login" "POST" "Reject incorrect password" 401 $status $pass "Properly returned HTTP 401 Unauthorized"
}

# Case 11: Failed Login (Non-existent Username)
try {
    $body = @{ username = "non_existent_ghost_user_999"; password = "Password@123" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/login" -Method Post -ContentType "application/json" -Body $body
    Record-Test "TC-11" "Authentication" "/api/users/login" "POST" "Reject unknown user" 401 200 $false "Expected rejection"
} catch {
    $status = [int]$_.Exception.Response.StatusCode
    $pass = ($status -eq 401)
    Record-Test "TC-11" "Authentication" "/api/users/login" "POST" "Reject unknown user" 401 $status $pass "Properly returned HTTP 401 Unauthorized"
}

# Case 12: Protected Endpoint Without Token
try {
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/profile" -Method Get
    Record-Test "TC-12" "Access Control" "/api/users/profile" "GET" "Reject anonymous request to protected route" 401 200 $false "Expected 401"
} catch {
    $status = [int]$_.Exception.Response.StatusCode
    $pass = ($status -eq 401)
    Record-Test "TC-12" "Access Control" "/api/users/profile" "GET" "Reject anonymous request to protected route" 401 $status $pass "Access denied with HTTP 401"
}

# Case 13: RBAC Enforcement (Standard User calling Admin Endpoint)
try {
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users" -Method Get -Headers @{ Authorization = "Bearer $userToken" }
    Record-Test "TC-13" "Authorization" "/api/users" "GET" "Forbid non-admin from listing all users" 403 200 $false "Expected 403"
} catch {
    $status = [int]$_.Exception.Response.StatusCode
    $pass = ($status -eq 403)
    Record-Test "TC-13" "Authorization" "/api/users" "GET" "Forbid non-admin from listing all users" 403 $status $pass "Access denied with HTTP 403 Forbidden"
}

# Case 14: Parameterless Profile Retrieval
try {
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/profile" -Method Get -Headers @{ Authorization = "Bearer $userToken" }
    $pass = ($res.success -and $res.data.username -eq "john_doe" -and $res.data.id -eq 2)
    Record-Test "TC-14" "Profile Mgmt" "/api/users/profile" "GET" "Read current profile (no ID params passed)" 200 200 $pass "Verified User: $($res.data.username), FullName: $($res.data.fullName)"
} catch {
    Record-Test "TC-14" "Profile Mgmt" "/api/users/profile" "GET" "Read current profile (no ID params passed)" 200 500 $false $_.Exception.Message
}

# Case 15: Parameterless Profile Update
try {
    $body = @{
        firstName = "Johnathan"
        lastName = "Doe"
        phoneNumber = "+1-555-8822"
        department = "Applied Cryptography"
        designation = "Lead Architect"
        city = "Boston"
        country = "USA"
        bio = "Updated parameterless profile via automated suite."
    } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/profile" -Method Put -Headers @{ Authorization = "Bearer $userToken" } -ContentType "application/json" -Body $body
    $pass = ($res.success -and $res.data.firstName -eq "Johnathan" -and $res.data.profile.department -eq "Applied Cryptography")
    Record-Test "TC-15" "Profile Mgmt" "/api/users/profile" "PUT" "Update current profile (no ID params in route/body)" 200 200 $pass "Updated Dept: $($res.data.profile.department), Name: $($res.data.fullName)"
} catch {
    Record-Test "TC-15" "Profile Mgmt" "/api/users/profile" "PUT" "Update current profile (no ID params in route/body)" 200 500 $false $_.Exception.Message
}

# Case 16: Parameterless Roles Retrieval
try {
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/my-roles" -Method Get -Headers @{ Authorization = "Bearer $userToken" }
    $pass = ($res.success -and $res.data.roles -contains "User")
    Record-Test "TC-16" "RBAC" "/api/users/my-roles" "GET" "Read caller's roles (no ID params passed)" 200 200 $pass "Retrieved Roles: $($res.data.roles -join ',')"
} catch {
    Record-Test "TC-16" "RBAC" "/api/users/my-roles" "GET" "Read caller's roles (no ID params passed)" 200 500 $false $_.Exception.Message
}

# Case 17: Admin List All Users
try {
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users?pageNumber=1&pageSize=10" -Method Get -Headers @{ Authorization = "Bearer $adminToken" }
    $pass = ($res.success -and $res.data.users.Count -ge 2 -and $res.data.pagination.totalRecords -ge 2)
    Record-Test "TC-17" "User CRUD" "/api/users" "GET" "Admin browse users with pagination" 200 200 $pass "Fetched: $($res.data.users.Count) users, Total: $($res.data.pagination.totalRecords)"
} catch {
    Record-Test "TC-17" "User CRUD" "/api/users" "GET" "Admin browse users with pagination" 200 500 $false $_.Exception.Message
}

# Case 18: Admin Create User
$adminCreatedId = 0
try {
    $body = @{
        username = "admin_created_" + (Get-Random -Minimum 1000 -Maximum 9999)
        email = "admin_created_" + (Get-Random -Minimum 1000 -Maximum 9999) + "@onlyusers.local"
        password = "AdminCreatedPass@123"
        firstName = "Provisioned"
        lastName = "Specialist"
        roles = @("User")
        department = "Operations"
    } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users" -Method Post -Headers @{ Authorization = "Bearer $adminToken" } -ContentType "application/json" -Body $body
    $adminCreatedId = $res.data.id
    $pass = ($res.success -and $adminCreatedId -gt 0)
    Record-Test "TC-18" "User CRUD" "/api/users" "POST" "Admin provision new user with roles" 200 200 $pass "Created User ID: $adminCreatedId, FullName: $($res.data.fullName)"
} catch {
    Record-Test "TC-18" "User CRUD" "/api/users" "POST" "Admin provision new user with roles" 200 500 $false $_.Exception.Message
}

# Case 19: Get User Details by ID
try {
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/$adminCreatedId" -Method Get -Headers @{ Authorization = "Bearer $adminToken" }
    $pass = ($res.success -and $res.data.id -eq $adminCreatedId)
    Record-Test "TC-19" "User CRUD" "/api/users/{id}" "GET" "Read specific user details by ID" 200 200 $pass "Fetched User: $($res.data.username), Dept: $($res.data.profile.department)"
} catch {
    Record-Test "TC-19" "User CRUD" "/api/users/{id}" "GET" "Read specific user details by ID" 200 500 $false $_.Exception.Message
}

# Case 20: Admin Assign Roles
try {
    $body = @{
        userId = $adminCreatedId
        roles = @("Admin", "User")
        replaceExisting = $true
    } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/roles" -Method Post -Headers @{ Authorization = "Bearer $adminToken" } -ContentType "application/json" -Body $body
    $pass = ($res.success -and $res.data.roles -contains "Admin" -and $res.data.roles -contains "User")
    Record-Test "TC-20" "RBAC" "/api/users/roles" "POST" "Elevate user roles to Admin & User" 200 200 $pass "Updated Roles: $($res.data.roles -join ',')"
} catch {
    Record-Test "TC-20" "RBAC" "/api/users/roles" "POST" "Elevate user roles to Admin & User" 200 500 $false $_.Exception.Message
}

# Case 21: Admin Remove Roles
try {
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/roles?userId=$adminCreatedId&roles=Admin" -Method Delete -Headers @{ Authorization = "Bearer $adminToken" }
    $pass = ($res.success -and $res.data.roles -notcontains "Admin" -and $res.data.roles -contains "User")
    Record-Test "TC-21" "RBAC" "/api/users/roles" "DELETE" "Revoke Admin role from user" 200 200 $pass "Remaining Roles: $($res.data.roles -join ',')"
} catch {
    Record-Test "TC-21" "RBAC" "/api/users/roles" "DELETE" "Revoke Admin role from user" 200 500 $false $_.Exception.Message
}

# Case 22: Admin Delete User
try {
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/$adminCreatedId" -Method Delete -Headers @{ Authorization = "Bearer $adminToken" }
    $pass = ($res.success -and $res.responseMessage -match "deleted successfully")
    Record-Test "TC-22" "User CRUD" "/api/users/{id}" "DELETE" "Permanently delete target user" 200 200 $pass "$($res.responseMessage)"
} catch {
    Record-Test "TC-22" "User CRUD" "/api/users/{id}" "DELETE" "Permanently delete target user" 200 500 $false $_.Exception.Message
}

# Case 23: Admin Self-Deletion Prevention
try {
    $res = Invoke-RestMethod -Uri "$BaseUrl/api/users/1" -Method Delete -Headers @{ Authorization = "Bearer $adminToken" }
    Record-Test "TC-23" "Security" "/api/users/{id}" "DELETE" "Prevent Admin from deleting their own account" 400 200 $false "Expected rejection"
} catch {
    $status = [int]$_.Exception.Response.StatusCode
    $pass = ($status -eq 400)
    Record-Test "TC-23" "Security" "/api/users/{id}" "DELETE" "Prevent Admin from deleting their own account" 400 $status $pass "Prevented self-deletion with HTTP 400 Bad Request"
}

# Summary
$passedCount = ($results | Where-Object { $_.Result -eq "PASS" }).Count
$failedCount = ($results | Where-Object { $_.Result -eq "FAIL" }).Count
$totalCount = $results.Count

Write-Host "`n================================================================================" -ForegroundColor Cyan
Write-Host "                       TEST EXECUTION SUMMARY" -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan
$failColor = if ($failedCount -gt 0) { "Red" } else { "Green" }
Write-Host "Total Test Cases Executed : $totalCount"
Write-Host "Cases Passed              : $passedCount" -ForegroundColor Green
Write-Host "Cases Failed              : $failedCount" -ForegroundColor $failColor
Write-Host "Pass Rate                 : $([math]::Round(($passedCount / $totalCount) * 100, 2))%" -ForegroundColor Green
Write-Host "================================================================================`n" -ForegroundColor Cyan

$results | Export-Csv -Path "D:\ONLYUSERSSERVICE\test-cases-report.csv" -NoTypeInformation
Write-Host "Full test report saved to D:\ONLYUSERSSERVICE\test-cases-report.csv" -ForegroundColor Yellow
