.PHONY: recreate-identity

dt-add-all:
	find . -name "*.csproj" -not -path "*/templates/*" -exec dotnet sln add {} +
dt-build:
	dotnet build
restore:
	dotnet restore
dev:
	$(eval LC_PROJECT := $(shell echo "$(project)" | tr '[:upper:]' '[:lower:]'))
	dotnet watch run \
  	--project apps/$(LC_PROJECT)/src/$(project).WebApi
ef-add:
	$(eval LC_PROJECT := $(shell echo "$(project)" | tr '[:upper:]' '[:lower:]'))
	dotnet ef migrations add $(name) \
  	--project apps/$(LC_PROJECT)/src/$(project).Infrastructure \
  	--startup-project apps/$(LC_PROJECT)/src/$(project).WebApi \
  	--output-dir Persistence/Migrations
ef-apply:
	$(eval LC_PROJECT := $(shell echo "$(project)" | tr '[:upper:]' '[:lower:]'))
	dotnet ef database update \
	--project apps/$(LC_PROJECT)/src/$(project).Infrastructure \
	--startup-project apps/$(LC_PROJECT)/src/$(project).WebApi
arch-diagrams:
	cd docs/architecture/diagrams
	uv sync 
	find . -name "*.py" | xargs -I {} uv run python {}
cert-rotate:
	openssl genpkey -algorithm EC -out ec_private.pem -pkeyopt ec_paramgen_curve:P-256
	openssl ec -pubout -in ec_private.pem -out ec_public.pem
create-min-api:
	@echo "Recreating Identity project..."
	cd apps && rm -rf orders && cookiecutter ../templates/minimal-api/ --no-input project_name="$(name)"
sln-add:
	
dkr-id:
	docker compose --profile id up -d
run-id:
	@echo "Running Identity project..."
	cd apps/identity && dotnet run --project src/WebApi.Identity/WebApi.Identity.csproj dotnet run --configuration Debug
run-ord:
	@echo "Running Orders project..."
	cd apps/orders  && dotnet run --project src/Orders.WebApi/Orders.WebApi.csproj dotnet run --configuration Debug
test-ord:
	@echo "Running Orders tests..."
	cd apps/orders && dotnet test --project tests/Orders.WebApi.Tests/Orders.WebApi.Tests.csproj

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
