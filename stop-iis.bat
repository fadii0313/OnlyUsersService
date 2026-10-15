@echo off
echo ===================================================
echo  Stopping OnlyUsersService IIS Site and AppPool...
echo ===================================================
%systemroot%\system32\inetsrv\appcmd.exe stop site /site.name:"OnlyUsersService_API"
%systemroot%\system32\inetsrv\appcmd.exe stop apppool /apppool.name:"OnlyUsersService_AppPool"
echo.
echo OnlyUsersService has been stopped.
pause
