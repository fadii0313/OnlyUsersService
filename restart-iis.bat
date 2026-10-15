@echo off
echo ===================================================
echo  Restarting OnlyUsersService IIS Site and AppPool...
echo ===================================================
%systemroot%\system32\inetsrv\appcmd.exe stop site /site.name:"OnlyUsersService_API"
%systemroot%\system32\inetsrv\appcmd.exe stop apppool /apppool.name:"OnlyUsersService_AppPool"
timeout /t 2 /nobreak > nul
%systemroot%\system32\inetsrv\appcmd.exe start apppool /apppool.name:"OnlyUsersService_AppPool"
%systemroot%\system32\inetsrv\appcmd.exe start site /site.name:"OnlyUsersService_API"
echo.
echo OnlyUsersService restarted successfully at: http://localhost:5200/swagger
pause
