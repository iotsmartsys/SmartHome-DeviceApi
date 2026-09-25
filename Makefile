api:
	clear
	clear
# 	dotnet run --project src/Api/Api.csproj

	set -a; [ -f .env ] && . ./.env; set +a; ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Api/Api.csproj

svc:
	dotnet run --project src/Incomming.Services/IncommingService.csproj


build:
	./build.sh

deploy:
# docker network create --driver overlay --attachable overlay-swarm
	docker run -d -p 8081:80 centraliot.azurecr.io/devices-api:latest --name devices-api

TEST_API_URL ?=
TEST_STATE ?= tests/Api.IntegrationTests/.runs/dashboard.json
TEST_CAPABILITY_ID ?=
TEST_TIMEOUT_SECONDS ?= 30
export TEST_API_URL TEST_STATE TEST_CAPABILITY_ID TEST_TIMEOUT_SECONDS

.PHONY: run-test clear-test

run-test:
	@: "$${TEST_API_URL:?Informe TEST_API_URL com a URL raiz da API}"; \
	set -- --base-url "$$TEST_API_URL" --state "$$TEST_STATE" --timeout-seconds "$$TEST_TIMEOUT_SECONDS"; \
	if [ -n "$$TEST_CAPABILITY_ID" ]; then set -- "$$@" --capability-id "$$TEST_CAPABILITY_ID"; fi; \
	dotnet run --project tests/Api.IntegrationTests -- run "$$@"

clear-test:
	dotnet run --project tests/Api.IntegrationTests -- cleanup --state "$$TEST_STATE" --timeout-seconds "$$TEST_TIMEOUT_SECONDS"
