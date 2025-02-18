api:
	dotnet run --project src/Api/Api.csproj


api-build:
	./build.sh

deploy:
# docker network create --driver overlay --attachable overlay-swarm
	docker run -d -p 8081:80 centraliot.azurecr.io/devices-api:latest --name devices-api