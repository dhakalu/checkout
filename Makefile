.PHONY: recreate-identity

recreate-id:
	@echo "Recreating Identity project..."
	cd apps && rm -rf identity && cookiecutter ../templates/minimal-api/ --no-input project_name="identity"

run-id:
	@echo "Running Identity project..."
	cd apps/identity && dotnet run --project src/WebApi.Identity/WebApi.Identity.csproj
test-id:
	@echo "Running Identity tests..."
	cd apps/identity && dotnet test --project tests/WebApi.Identity.Tests/WebApi.Identity.Tests.csproj