#!/bin/bash

# Script to scaffold Next.js 14+ project in src/frontend/
# This script should be run from the repository root

set -e  # Exit on error

echo "======================================"
echo "Scaffolding Next.js 14+ Project"
echo "======================================"
echo ""

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "Error: Node.js is not installed. Please install Node.js 18.17 or later."
    exit 1
fi

# Check Node.js version
NODE_VERSION=$(node -v | cut -d'v' -f2 | cut -d'.' -f1)
if [ "$NODE_VERSION" -lt 18 ]; then
    echo "Error: Node.js version 18 or later is required. Current version: $(node -v)"
    exit 1
fi

echo "Node.js version: $(node -v)"
echo "npm version: $(npm -v)"
echo ""

# Navigate to repository root (if not already there)
cd "$(dirname "$0")"

echo "Creating Next.js application in src/frontend/..."
echo ""

# Run create-next-app with all required options
npx create-next-app@latest src/frontend \
  --app \
  --ts \
  --tailwind \
  --eslint \
  --src-dir \
  --no-import-alias \
  --yes

echo ""
echo "======================================"
echo "Verifying installation..."
echo "======================================"
echo ""

# Check if key files exist
REQUIRED_FILES=(
  "src/frontend/package.json"
  "src/frontend/tsconfig.json"
  "src/frontend/tailwind.config.ts"
  "src/frontend/next.config.js"
  "src/frontend/.eslintrc.json"
  "src/frontend/src/app/layout.tsx"
  "src/frontend/src/app/page.tsx"
)

ALL_FILES_EXIST=true
for file in "${REQUIRED_FILES[@]}"; do
  if [ -f "$file" ]; then
    echo "✓ $file exists"
  else
    echo "✗ $file is missing"
    ALL_FILES_EXIST=false
  fi
done

echo ""

if [ "$ALL_FILES_EXIST" = false ]; then
  echo "Error: Some required files are missing. Scaffolding may have failed."
  exit 1
fi

echo "======================================"
echo "Testing development server..."
echo "======================================"
echo ""

cd src/frontend

# Install dependencies (if not already installed by create-next-app)
if [ ! -d "node_modules" ]; then
  echo "Installing dependencies..."
  npm install
fi

echo ""
echo "Starting dev server for 5 seconds to verify it works..."
echo ""

# Start dev server in background and capture PID
timeout 10s npm run dev &
DEV_PID=$!

# Wait a bit for server to start
sleep 5

# Check if process is still running
if ps -p $DEV_PID > /dev/null; then
  echo "✓ Dev server started successfully"
  # Kill the dev server
  kill $DEV_PID 2>/dev/null || true
  wait $DEV_PID 2>/dev/null || true
else
  echo "✗ Dev server failed to start"
  exit 1
fi

echo ""
echo "======================================"
echo "Testing production build..."
echo "======================================"
echo ""

npm run build

if [ $? -eq 0 ]; then
  echo ""
  echo "✓ Production build completed successfully"
else
  echo ""
  echo "✗ Production build failed"
  exit 1
fi

cd ../..

echo ""
echo "======================================"
echo "✅ Next.js scaffolding completed successfully!"
echo "======================================"
echo ""
echo "To start development:"
echo "  cd src/frontend"
echo "  npm run dev"
echo ""
echo "Then open http://localhost:3000 in your browser"
echo ""
