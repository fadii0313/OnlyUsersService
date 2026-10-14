@echo off
title OnlyUsersService - Web API
echo ======================================================================
echo Starting OnlyUsersService Microservice on http://localhost:5200...
echo Swagger UI will be available at: http://localhost:5200/swagger
echo Web SPA Dashboard will be available at: http://localhost:5200/
echo ======================================================================
dotnet run --project OnlyUsersService.API --urls "http://localhost:5200"
pause
