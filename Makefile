api:
	dotnet run --project src/Api/Api.csproj


build:
	./build.sh

deploy:
	docker network create --driver overlay --attachable overlay-swarm
	docker run -d -p 8081:8080 centraliot.azurecr.io/devices-api:v1.0.3 --name devices-api