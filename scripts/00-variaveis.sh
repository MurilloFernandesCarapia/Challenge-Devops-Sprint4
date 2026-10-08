export MSYS_NO_PATHCONV=1

export LOCATION="southafricanorth"
export RG_NAME="rg-petcare360-sprint4"

export PLAN_NAME="plan-petcare360-sprint4"
export WEBAPP_NAME="petcare360-rm564969"
export WEBAPP_RUNTIME="DOTNETCORE:10.0"

export COSMOS_NAME="cosmos-petcare360-rm564969"
export MONGO_DATABASE="petcare360"
export MONGO_COLLECTION="auditoria"

echo "Variaveis carregadas."
echo "  Regiao          : $LOCATION"
echo "  Resource Group  : $RG_NAME"
echo "  Web App         : $WEBAPP_NAME"
echo "  Cosmos DB       : $COSMOS_NAME"