@echo off
REM Local CI script - runs the same build, test, and coverage process as GitHub Actions
cd /d "%~dp0.."

echo 🏗️  Building solution...
dotnet build CleanArchitecture.slnx --configuration Release
if errorlevel 1 exit /b 1

echo 🧪 Running tests with coverage...
if exist coverage rmdir /s /q coverage
dotnet test CleanArchitecture.slnx --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage" --results-directory ./coverage
if errorlevel 1 exit /b 1

echo 📊 Installing ReportGenerator (if not already installed)...
dotnet tool install -g dotnet-reportgenerator-globaltool --ignore-failed-sources >nul 2>&1

echo 📈 Generating coverage reports...
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html;Cobertura;MarkdownSummary" -verbosity:Info
if errorlevel 1 exit /b 1

echo ✅ Build, test, and coverage completed successfully!
echo 📁 Coverage report available at: coverage/report/index.html

REM Open coverage report
echo 🌐 Opening coverage report...
start coverage\report\index.html

pause