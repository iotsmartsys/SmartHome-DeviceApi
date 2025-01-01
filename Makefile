api:
	dotnet run --project src/Api/Api.csproj


build:
	./build.sh
# docker buildx build --platform=linux/arm64/v8 -f src/Api/Dockerfile -t centraliot.azurecr.io/smarthome-api:v1.0.3 ./src
# docker push centraliot.azurecr.io/smarthome-api:v1.0.3

deploy:
	docker network create --driver overlay --attachable overlay-swarm
	docker run -d -p 8081:8080 centraliot.azurecr.io/smarthome-api:v1.0.3 --name smarthome-api