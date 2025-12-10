#!/bin/bash

# Local CI script - runs the same build, test, and coverage process as GitHub Actions
set -e

echo "🏗️  Building solution..."
dotnet build CleanArchitecture.slnx --configuration Release

echo "🧪 Running tests with coverage..."
rm -rf coverage
dotnet test CleanArchitecture.slnx \
  --configuration Release \
  --no-build \
  --verbosity normal \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage

echo "📊 Installing ReportGenerator (if not already installed)..."
dotnet tool install -g dotnet-reportgenerator-globaltool --ignore-failed-sources || true

echo "📈 Generating coverage reports..."
reportgenerator \
  -reports:"coverage/**/coverage.cobertura.xml" \
  -targetdir:"coverage/report" \
  -reporttypes:"Html;Cobertura;MarkdownSummary" \
  -verbosity:Info

echo "✅ Build, test, and coverage completed successfully!"
echo "📁 Coverage report available at: coverage/report/index.html"

# Open coverage report if on macOS or if xdg-open is available
if command -v open &> /dev/null; then
    echo "🌐 Opening coverage report..."
    open coverage/report/index.html
elif command -v xdg-open &> /dev/null; then
    echo "🌐 Opening coverage report..."
    xdg-open coverage/report/index.html
else
    echo "💡 Open coverage/report/index.html in your browser to view the coverage report"
fi