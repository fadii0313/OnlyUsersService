param (
    [string]$BaseUrl = "http://localhost:5200"
)

Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host "  OnlyUsersService End-to-End Automated Verification" -ForegroundColor Cyan
Write-Host "  Target: $BaseUrl" -ForegroundColor Cyan
Write-Host "===========================================================" -ForegroundColor Cyan

try {
    # 1. Swagger Check
    Write-Host "`n[1/9] Testing Swagger / OpenAPI endpoint..." -NoNewline
    $swagger = Invoke-RestMethod -Uri "$BaseUrl/swagger/v1/swagger.json" -Method Get
    Write-Host " OK! (Title: $($swagger.info.title), Version: $($swagger.info.version))" -ForegroundColor Green

    # 2. SHA-256 Hashing Service
    Write-Host "[2/9] Testing SHA-256 Hash endpoint (/api/users/hash)..." -NoNewline
    $hashPayload = @{ text = "Antigravity2026"; salt = "secure_salt" } | ConvertTo-Json
    $hashRes = Invoke-RestMethod -Uri "$BaseUrl/api/users/hash" -Method Post -ContentType "application/json" -Body $hashPayload
    Write-Host " OK! (Algorithm: $($hashRes.data.algorithm), Hash: $($hashRes.data.sha256Hash.Substring(0, 16))...)" -ForegroundColor Green

    # 3. Admin Login
    Write-Host "[3/9] Testing Admin Login (/api/users/login)..." -NoNewline
    $adminLogin = @{ username = "admin"; password = "Admin@123456" } | ConvertTo-Json
    $adminRes = Invoke-RestMethod -Uri "$BaseUrl/api/users/login" -Method Post -ContentType "application/json" -Body $adminLogin
    $adminToken = $adminRes.data.token
    Write-Host " OK! (Token generated, Roles: $($adminRes.data.roles -join ', '))" -ForegroundColor Green

    # 4. User Registration
    $testUser = "testuser_" + (Get-Random -Minimum 1000 -Maximum 9999)
    Write-Host "[4/9] Testing User Registration with SHA-256 (/api/users/register) for '$testUser'..." -NoNewline
    $regPayload = @{
        username = $testUser
        email = "$testUser@onlyusers.local"
        password = "UserPassword@123"
        firstName = "Automated"
        lastName = "Tester"
        phoneNumber = "+1-555-0188"
        department = "Quality Assurance"
        designation = "SDET"
        city = "Austin"
        country = "USA"
    } | ConvertTo-Json
    $regRes = Invoke-RestMethod -Uri "$BaseUrl/api/users/register" -Method Post -ContentType "application/json" -Body $regPayload
    $newUserId = $regRes.data.userId
    $newUserToken = $regRes.data.token
    Write-Host " OK! (Created User ID: $newUserId)" -ForegroundColor Green

    # 5. User Profile View (No user id or office id passed)
    Write-Host "[5/9] Testing Current User Profile without params (/api/users/profile)..." -NoNewline
    $profileRes = Invoke-RestMethod -Uri "$BaseUrl/api/users/profile" -Method Get -Headers @{ Authorization = "Bearer $newUserToken" }
    Write-Host " OK! (Name: $($profileRes.data.fullName), Dept: $($profileRes.data.profile.department))" -ForegroundColor Green

    # 6. User Roles View (No user id or office id passed)
    Write-Host "[6/9] Testing My Roles without params (/api/users/my-roles)..." -NoNewline
    $myRolesRes = Invoke-RestMethod -Uri "$BaseUrl/api/users/my-roles" -Method Get -Headers @{ Authorization = "Bearer $newUserToken" }
    Write-Host " OK! (Roles: $($myRolesRes.data.roles -join ', '))" -ForegroundColor Green

    # 7. Admin List All Users
    Write-Host "[7/9] Testing Admin User Directory (/api/users)..." -NoNewline
    $allUsersRes = Invoke-RestMethod -Uri "$BaseUrl/api/users" -Method Get -Headers @{ Authorization = "Bearer $adminToken" }
    Write-Host " OK! (Total records: $($allUsersRes.data.pagination.totalRecords))" -ForegroundColor Green

    # 8. Admin Role Assignment
    Write-Host "[8/9] Testing Role Assignment (/api/users/roles)..." -NoNewline
    $rolePayload = @{
        userId = $newUserId
        roles = @("Admin", "User")
        replaceExisting = $true
    } | ConvertTo-Json
    $roleRes = Invoke-RestMethod -Uri "$BaseUrl/api/users/roles" -Method Post -Headers @{ Authorization = "Bearer $adminToken" } -ContentType "application/json" -Body $rolePayload
    Write-Host " OK! (New roles: $($roleRes.data.roles -join ', '))" -ForegroundColor Green

    # 9. Admin Delete User
    Write-Host "[9/9] Testing User Deletion (/api/users/{id})..." -NoNewline
    $delRes = Invoke-RestMethod -Uri "$BaseUrl/api/users/$newUserId" -Method Delete -Headers @{ Authorization = "Bearer $adminToken" }
    Write-Host " OK! ($($delRes.responseMessage))" -ForegroundColor Green

    Write-Host "`n===========================================================" -ForegroundColor Green
    Write-Host "  ALL 9 E2E TESTS PASSED SUCCESSFULLY! Service is healthy." -ForegroundColor Green
    Write-Host "===========================================================" -ForegroundColor Cyan
}
catch {
    Write-Host "`nFAILED: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        Write-Host "Response Body: $($reader.ReadToEnd())" -ForegroundColor Yellow
    }
}
