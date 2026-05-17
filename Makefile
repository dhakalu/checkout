.PHONY: recreate-identity

recreate-id:
	@echo "Recreating Identity project..."
	cd apps && rm -rf identity && cookiecutter ../templates/minimal-api/ --no-input project_name="identity"
dkr-id:
	docker compose --profile id up -d
run-id:
	@echo "Running Identity project..."
	cd apps/identity && dotnet run --project src/WebApi.Identity/WebApi.Identity.csproj dotnet run --configuration Debug
test-id:
	@echo "Running Identity tests..."
	cd apps/identity && dotnet test --project tests/WebApi.Identity.Tests/WebApi.Identity.Tests.csproj
nuget-add-id:
ifndef package
	$(error Missing required parameter 'package'. Usage: make nuget-add-id package=Name [version=X.X.X])
endif
	@echo "Adding NuGet package '$(package)' to Identity project..."
	cd apps/identity && dotnet add src/WebApi.Identity/WebApi.Identity.csproj package $(package) $(if $(version),--version $(version),)
ef-apply-id:
	@echo "Applying EF Core migrations for Identity project..."
	cd apps/identity && dotnet ef database update --project src/WebApi.Identity/WebApi.Identity.csproj
ef-add-id:
	@echo "Adding EF Core migration for Identity project..."
	cd apps/identity && dotnet ef migrations add $(name) --project src/WebApi.Identity/WebApi.Identity.csproj

dkr-db-id:
	@echo "Stopping and removing existing 'id-db' container if it exists..."
	-docker stop id-db 2>/dev/null || true
	-docker rm id-db 2>/dev/null || true
	@echo "Starting a fresh passwordless PostgreSQL container..."
	docker run --name id-db -e POSTGRES_USER=identity -e POSTGRES_HOST_AUTH_METHOD=trust -e POSTGRES_DB=identity -p 5432:5432 -d postgres:18.3-alpine3.23
