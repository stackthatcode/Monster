CD C:\DEV\Monster

REM clean-up the output directories
rmdir /S /Q "C:\DEV\Monster\BSA01-Deployment"
mkdir "C:\DEV\Monster\BSA01-Deployment"
mkdir "C:\DEV\Monster\BSA01-Deployment\ConsoleApp"
mkdir "C:\DEV\Monster\BSA01-Deployment\WebApp"
pause

REM build the solution
devenv Monster.sln /rebuild "Release|Any CPU"
pause

REM this step publishes the Web App
msbuild C:\DEV\Monster\Monster.Web\Monster.Web.csproj  /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile

REM this step publishes (copies) the Console App
xcopy /e "C:\DEV\Monster\Monster.Console\bin\Release" "C:\DEV\Monster\BSA01-Deployment\ConsoleApp"

pause
