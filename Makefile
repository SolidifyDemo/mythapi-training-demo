.PHONY: dev dev-backend dev-frontend help

help:
	@echo "MythAPI Development Commands"
	@echo ""
	@echo "Usage:"
	@echo "  make dev           - Run both backend and frontend concurrently"
	@echo "  make dev-backend   - Run only the backend API"
	@echo "  make dev-frontend  - Run only the frontend"
	@echo ""

dev-backend:
	dotnet run --project src/MythApi.csproj

dev-frontend:
	cd src/frontend && npm run dev

dev:
	@echo "Starting backend and frontend..."
	@echo "Backend will be available at http://localhost:5280"
	@echo "Frontend will be available at http://localhost:3000"
	@echo "Press Ctrl+C to stop both services"
	@echo ""
	@make -j2 dev-backend dev-frontend
