@echo off
echo ===================================================
echo  Starting OnlyUsersService IIS Site and AppPool...
echo ===================================================
%systemroot%\system32\inetsrv\appcmd.exe start apppool /apppool.name:"OnlyUsersService_AppPool"
%systemroot%\system32\inetsrv\appcmd.exe start site /site.name:"OnlyUsersService_API"
echo.
echo OnlyUsersService is running at: http://localhost:5200/swagger
pause
