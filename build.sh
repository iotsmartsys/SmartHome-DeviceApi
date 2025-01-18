#!/usr/bin/env bash

# Script: build_and_push.sh
# Objetivo: Fazer o build de uma imagem Docker com duas tags:
#           - v1.0.<YYYYMMDDHHMM>
#           - latest
# Além disso, faz push das duas tags para um registry (opcional).

# Usar "set -e" faz com que o script pare imediatamente se qualquer comando falhar
set -e

# -------------------------
# Ajuste estas variáveis:
# -------------------------
REGISTRY="centraliot.azurecr.io"  # ou "docker.io/seu-usuario"
IMAGE_NAME="devices-api"                         # nome da sua imagem
VERSION="v1.0"                           # sua versão base

# Gera a string de data/tempo no formato YYYYMMDDHHMM (Ex: 202412312043)
DATE_TAG=$(date +%Y%m%d%H%M)

# Concatena tudo na tag final
UNIQUE_TAG="$VERSION.$DATE_TAG"  # Ex: v1.0.202412312043

# Monta o nome completo da imagem (com registry)
FULL_TAG="$REGISTRY/$IMAGE_NAME:$UNIQUE_TAG"  # Ex: seu-registry.com/seu-usuario/api:v1.0.202412312043
LATEST_TAG="$REGISTRY/$IMAGE_NAME:latest"     # Ex: seu-registry.com/seu-usuario/api:latest

echo "-------------------------------------------------------"
echo "     Iniciando build da imagem Docker"
echo "-------------------------------------------------------"
echo "Usando tags:"
echo "  1) $FULL_TAG"
echo "  2) $LATEST_TAG"
echo

# Faz o build da imagem com as duas tags
docker buildx build --platform=linux/arm64/v8 -f src/Api/Dockerfile -t "$FULL_TAG" -t "$LATEST_TAG" ./src

echo
echo "-------------------------------------------------------"
echo "     Realizando push da imagem Docker"
echo "-------------------------------------------------------"
# Push da imagem com a tag única
docker push "$FULL_TAG"
# Push da imagem com a tag latest
docker push "$LATEST_TAG"

echo
echo "-------------------------------------------------------"
echo " Build e push finalizados com sucesso!"
echo "-------------------------------------------------------"
echo "Imagem gerada com tag única: $FULL_TAG"
echo "Imagem gerada com tag latest: $LATEST_TAG"

# Substitui "latest" por UNIQUE_TAG no arquivo docker-compose.swarm.yaml VERSÃO MAC_OS
# sed -i '' "s/latest/${UNIQUE_TAG}/g" docker-compose.swarm.yaml
# echo "Substituição em docker-compose.swarm.yaml concluída: 'latest' -> '${UNIQUE_TAG}'"