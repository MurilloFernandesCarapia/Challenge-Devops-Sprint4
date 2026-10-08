set -e

source "$(dirname "$0")/00-variaveis.sh"

echo ""
echo ">>> [1/3] Criando a conta do Cosmos DB for MongoDB (free tier)..."
echo "    Essa etapa leva de 5 a 10 minutos."
az cosmosdb create \
  --name "$COSMOS_NAME" \
  --resource-group "$RG_NAME" \
  --kind MongoDB \
  --server-version 4.2 \
  --enable-free-tier true \
  --default-consistency-level Session \
  --locations regionName="$LOCATION" failoverPriority=0 isZoneRedundant=false \
  --output none

echo ""
echo ">>> [2/3] Criando o banco $MONGO_DATABASE..."
az cosmosdb mongodb database create \
  --account-name "$COSMOS_NAME" \
  --resource-group "$RG_NAME" \
  --name "$MONGO_DATABASE" \
  --output none

echo ""
echo ">>> [3/3] Criando a colecao $MONGO_COLLECTION com indice por data..."
az cosmosdb mongodb collection create \
  --account-name "$COSMOS_NAME" \
  --resource-group "$RG_NAME" \
  --database-name "$MONGO_DATABASE" \
  --name "$MONGO_COLLECTION" \
  --throughput 400 \
  --idx '[{"key":{"keys":["_id"]}},{"key":{"keys":["DataHora"]}}]' \
  --output none

echo ""
echo "OK. Cosmos DB pronto."
echo "A connection string vai como variavel secreta no Azure DevOps. Para consultar:"
echo "  az cosmosdb keys list --type connection-strings --name $COSMOS_NAME --resource-group $RG_NAME --query \"connectionStrings[0].connectionString\" -o tsv"