set -e

source "$(dirname "$0")/00-variaveis.sh"

echo ""
echo ">>> [1/5] Criando o Resource Group..."
az group create \
  --name "$RG_NAME" \
  --location "$LOCATION" \
  --output table

echo ""
echo ">>> [2/5] Criando o plano do App Service (Linux, F1 gratuito)..."
az appservice plan create \
  --name "$PLAN_NAME" \
  --resource-group "$RG_NAME" \
  --location "$LOCATION" \
  --is-linux \
  --sku F1 \
  --output table

echo ""
echo ">>> [3/5] Criando o Web App com runtime .NET 10..."
az webapp create \
  --name "$WEBAPP_NAME" \
  --resource-group "$RG_NAME" \
  --plan "$PLAN_NAME" \
  --runtime "$WEBAPP_RUNTIME" \
  --output table

echo ""
echo ">>> [4/5] Configurando inicializacao, HTTPS e ambiente..."
az webapp config set \
  --name "$WEBAPP_NAME" \
  --resource-group "$RG_NAME" \
  --startup-file "dotnet PetCare360.API.dll" \
  --ftps-state Disabled \
  --output none

az webapp update \
  --name "$WEBAPP_NAME" \
  --resource-group "$RG_NAME" \
  --https-only true \
  --output none

az webapp config appsettings set \
  --name "$WEBAPP_NAME" \
  --resource-group "$RG_NAME" \
  --settings ASPNETCORE_ENVIRONMENT=Production \
  --output none

echo ""
echo ">>> [5/5] Endereco publico do Web App:"
HOST=$(az webapp show --name "$WEBAPP_NAME" --resource-group "$RG_NAME" --query "defaultHostName" -o tsv)

echo ""
echo "  Swagger : https://$HOST/swagger"
echo ""
echo "OK. O Web App esta criado e vazio. O codigo chega pela pipeline de CD."