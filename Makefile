.PHONY: recreate-identity

recreate-identity:
	@echo "Recreating Identity project..."
	cd apps && rm -rf identity && cookiecutter ../templates/minimal-api/ --no-input project_name="identity"

test-identity:
	@echo "Running Identity tests..."
	cd apps/identity && dotnet test --project tests/WebApi.Identity.Tests/WebApi.Identity.Tests.csproj