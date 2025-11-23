#!/bin/bash

set -e
    
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' 

echo -e "${YELLOW}Etapa 1: Configurando variáveis...${NC}"

# Variáveis específicas do projeto WorkWell
RESOURCE_GROUP="rg-workwell-fiap-558424"
LOCATION="brazilsouth"
DB_SERVER_NAME="sql-workwell-558424"
DB_NAME="workwelldb"
APP_SERVICE_PLAN="asp-workwell-fiap-558424"
WEB_APP_NAME="webapp-workwell-558424"
DB_ADMIN_USER="admin_cloud"
DB_ADMIN_PASSWORD="Senhaforte123!"

# Variáveis para o projeto .NET
API_PROJECT_PATH="WorkWell.Api/WorkWell.Api.csproj"
INFRA_PROJECT_PATH="WorkWell.Infrastructure/WorkWell.Infrastructure.csproj"

if [ -z "$DB_ADMIN_USER" ] || [ -z "$DB_ADMIN_PASSWORD" ]; then
    echo -e "${YELLOW}Erro: DB_ADMIN_USER e DB_ADMIN_PASSWORD devem ser definidos como variáveis de ambiente${NC}"
    echo "Exemplo: export DB_ADMIN_USER='seu_usuario' && export DB_ADMIN_PASSWORD='sua_senha'"
    exit 1
fi

echo -e "${GREEN}✓ Variáveis configuradas${NC}\n"

# --- Registrar Resource Providers ---

echo -e "${YELLOW}Etapa 2: Verificando Resource Providers...${NC}"

wait_for_provider() {
    local namespace=$1
    local name=$2
    
    if ! az provider show --namespace "$namespace" --query "registrationState" -o tsv 2>/dev/null | grep -q "Registered"; then
        echo "Registrando $name (isso pode demorar alguns minutos)..."
        az provider register --namespace "$namespace"
        
        echo "Aguardando registro de $name..."
        local max_attempts=30
        local attempt=0
        while [ $attempt -lt $max_attempts ]; do
            if az provider show --namespace "$namespace" --query "registrationState" -o tsv 2>/dev/null | grep -q "Registered"; then
                echo "$name registrado com sucesso"
                return 0
            fi
            attempt=$((attempt + 1))
            sleep 10
        done
        echo -e "${YELLOW}Aviso: $name ainda não está registrado após 5 minutos, continuando...${NC}"
    else
        echo "$name já está registrado"
    fi
}

wait_for_provider "Microsoft.Sql" "Microsoft.Sql"
wait_for_provider "Microsoft.Web" "Microsoft.Web"
wait_for_provider "Microsoft.Storage" "Microsoft.Storage"

echo -e "${GREEN}✓ Resource Providers verificados${NC}\n"

# --- Criar Infraestrutura ---

echo -e "${YELLOW}Etapa 3: Criando Grupo de Recursos...${NC}"
az group create \
    --name $RESOURCE_GROUP \
    --location $LOCATION \
    --output none
echo -e "${GREEN}✓ Grupo de Recursos criado${NC}\n"

echo -e "${YELLOW}Etapa 4: Criando Servidor SQL...${NC}"
az sql server create \
    --name $DB_SERVER_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $DB_ADMIN_USER \
    --admin-password $DB_ADMIN_PASSWORD \
    --output none
echo -e "${GREEN}✓ Servidor SQL criado${NC}\n"

echo -e "${YELLOW}Etapa 5: Configurando Firewall...${NC}"
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $DB_SERVER_NAME \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0 \
    --output none
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $DB_SERVER_NAME \
    --name AllowAll \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 255.255.255.255 \
    --output none
echo -e "${GREEN}✓ Firewall configurado${NC}\n"

echo -e "${YELLOW}Etapa 6: Criando Banco de Dados...${NC}"
az sql db create \
    --resource-group $RESOURCE_GROUP \
    --server $DB_SERVER_NAME \
    --name $DB_NAME \
    --service-objective S0 \
    --backup-storage-redundancy Local \
    --output none
echo -e "${GREEN}✓ Banco de Dados criado${NC}\n"

echo -e "${YELLOW}Etapa 7: Criando App Service Plan...${NC}"
az appservice plan create \
    --name $APP_SERVICE_PLAN \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku B1 \
    --is-linux \
    --output none
echo -e "${GREEN}✓ App Service Plan criado${NC}\n"

echo -e "${YELLOW}Etapa 8: Criando Web App...${NC}"
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $WEB_APP_NAME \
    --runtime "DOTNETCORE:8.0" \
    --output none
echo -e "${GREEN}✓ Web App criado${NC}\n"

echo -e "${YELLOW}Etapa 9: Configurando Connection String...${NC}"
CONNECTION_STRING="Server=tcp:${DB_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${DB_NAME};Persist Security Info=False;User ID=${DB_ADMIN_USER};Password=${DB_ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configurar via App Settings ao invés de Connection Strings (mais compatível com .NET no Linux)
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --settings \
        "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
    --output none

echo -e "${GREEN}✓ Connection String configurada${NC}\n"

echo -e "${YELLOW}Etapa 10: Configurando App Settings...${NC}"
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --settings \
        ASPNETCORE_ENVIRONMENT="Production" \
        WEBSITE_RUN_FROM_PACKAGE="0" \
        SCM_DO_BUILD_DURING_DEPLOYMENT="false" \
        WEBSITES_PORT="8080" \
    --output none

# Configurar o startup command separadamente (mais confiável)
az webapp config set \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --startup-file "dotnet WorkWell.API.dll" \
    --output none

echo -e "${GREEN}✓ App Settings configurado${NC}\n"

# --- Deploy da Aplicação ---

echo -e "${YELLOW}Etapa 11: Preparando projeto .NET...${NC}"

# Navegar para o diretório workwell-dotnet para que o dotnet publish funcione corretamente
cd workwell-dotnet
if [ $? -ne 0 ]; then
    echo -e "${YELLOW}Erro: Não foi possível entrar no diretório workwell-dotnet.${NC}"
    exit 1
fi

# A busca deve ser feita a partir do diretório atual (workwell-dotnet)
API_PROJECT=$(find . -name "WorkWell.API.csproj" | head -n 1)

if [ -z "$API_PROJECT" ]; then
    echo -e "${YELLOW}Erro: Não foi possível encontrar o projeto WorkWell.API.csproj.${NC}"
    exit 1
fi

echo "Projeto da API encontrado: $API_PROJECT"

echo -e "${YELLOW}Compilando aplicação...${NC}"
echo "Publicando APENAS o projeto da API (não a solution inteira): $API_PROJECT"

# Limpar builds anteriores
echo "Limpando builds anteriores..."
dotnet clean "$API_PROJECT" --configuration Release

# Restaurar pacotes com força
echo "Restaurando pacotes..."
dotnet restore "$API_PROJECT" --force

# IMPORTANTE: Publicar apenas o projeto da API com todas as dependências
# --self-contained false garante que use o runtime do Azure
# -p: é a sintaxe correta para propriedades MSBuild
echo "Publicando projeto..."
dotnet publish "$API_PROJECT" \
    -c Release \
    -o ./publish \
    --no-restore \
    --self-contained false \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=false \
    -p:CopyRefAssembliesToPublishDirectory=true
echo -e "${GREEN}✓ Aplicação compilada${NC}\n"

# Verificar se a pasta publish tem conteúdo
echo -e "${YELLOW}Verificando conteúdo da pasta publish...${NC}"
if [ ! -d "./publish" ]; then
    echo -e "${YELLOW}Erro: Pasta publish não foi criada!${NC}"
    exit 1
fi

FILE_COUNT=$(find ./publish -type f | wc -l)
echo "Arquivos encontrados na pasta publish: $FILE_COUNT"

if [ "$FILE_COUNT" -lt 5 ]; then
    echo -e "${YELLOW}Erro: Pasta publish está vazia ou com poucos arquivos!${NC}"
    echo "Conteúdo da pasta publish:"
    ls -la ./publish
    exit 1
fi

echo -e "${GREEN}✓ Pasta publish contém $FILE_COUNT arquivos${NC}\n"

echo -e "${YELLOW}Etapa 12: Fazendo deploy para Azure...${NC}"

# Criar ZIP temporário para deploy
echo -e "${YELLOW}Criando pacote para deploy...${NC}"
python3 - <<'PYTHON'
import os
import zipfile

with zipfile.ZipFile('deploy.zip', 'w', zipfile.ZIP_DEFLATED) as archive:
    for root, _, files in os.walk('publish'):
        for name in files:
            path = os.path.join(root, name)
            archive.write(path, os.path.relpath(path, 'publish'))
PYTHON

if [ ! -f "deploy.zip" ]; then
    echo -e "${YELLOW}Erro: deploy.zip não foi criado!${NC}"
    exit 1
fi

ZIP_SIZE=$(stat -f%z "deploy.zip" 2>/dev/null || stat -c%s "deploy.zip" 2>/dev/null)
echo "Tamanho do deploy.zip: ${ZIP_SIZE} bytes"
echo -e "${GREEN}✓ Pacote criado${NC}"

# Usar config-zip (método mais confiável e antigo)
echo -e "${YELLOW}Fazendo upload para Azure Web App (método config-zip)...${NC}"
az webapp deployment source config-zip \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --src deploy.zip

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Deploy concluído com sucesso${NC}"
else
    echo -e "${YELLOW}⚠ Erro no deploy via config-zip${NC}"
    echo "Tentando verificar o status do Web App..."
    az webapp show --resource-group $RESOURCE_GROUP --name $WEB_APP_NAME --query "state" -o tsv
fi

# Limpar arquivo temporário
rm -f deploy.zip

echo -e "${GREEN}✓ Upload concluído${NC}\n"

echo -e "${YELLOW}Etapa 12.1: Aplicando migrations no banco de dados...${NC}"
echo "Verificando dotnet-ef..."
# Verifica se dotnet-ef está disponível, senão tenta instalar
if ! command -v dotnet-ef &> /dev/null; then
    echo "Instalando dotnet-ef..."
    dotnet tool install --global dotnet-ef 2>/dev/null || echo "dotnet-ef já está instalado"
else
    echo "dotnet-ef já está disponível"
fi

echo "Aplicando migrations no banco de dados Azure..."
CONNECTION_STRING_MIGRATION="Server=tcp:${DB_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${DB_NAME};Persist Security Info=False;User ID=${DB_ADMIN_USER};Password=${DB_ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Os caminhos INFRA_PROJECT_PATH e API_PROJECT_PATH precisam ser relativos ao diretório workwell-dotnet
INFRA_PROJECT_PATH="WorkWell.Infrastructure/WorkWell.Infrastructure.csproj"
API_PROJECT_PATH="WorkWell.API/WorkWell.API.csproj"

dotnet ef database update --connection "$CONNECTION_STRING_MIGRATION" --project "$INFRA_PROJECT_PATH" --startup-project "$API_PROJECT_PATH"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Migrations aplicadas com sucesso${NC}\n"
else
    echo -e "${YELLOW}⚠ Erro ao aplicar migrations. Continuando...${NC}\n"
fi

# Voltar para o diretório raiz do deploy.sh
cd ..
rm -rf ./publish

echo -e "${GREEN}✓ Banco de dados configurado${NC}\n"

echo -e "${YELLOW}Etapa 12.2: Configurando startup command (garantindo aplicação correta)...${NC}"
az webapp config set \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --startup-file "dotnet WorkWell.API.dll" \
    --output none
echo -e "${GREEN}✓ Startup command reconfigurado${NC}\n"

echo -e "${YELLOW}Etapa 13: Reiniciando Web App...${NC}"
az webapp restart \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --output none

sleep 10

echo -e "${GREEN}✓ Web App reiniciado${NC}\n"

# --- Verificação pós-deploy ---

echo -e "${YELLOW}Etapa 14: Verificando logs da aplicação...${NC}"
echo "Últimos logs do container:"
az webapp log tail \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --output none &

LOG_PID=$!
sleep 15
kill $LOG_PID 2>/dev/null

echo -e "${GREEN}✓ Verificação concluída${NC}\n"

# --- Finalização ---

WEBAPP_URL="https://$WEB_APP_NAME.azurewebsites.net"

echo -e "${GREEN}======================================================${NC}"
echo -e "${GREEN}DEPLOY CONCLUÍDO COM SUCESSO!${NC}"
echo -e "${GREEN}======================================================${NC}"
echo -e "URL da Aplicação: ${YELLOW}$WEBAPP_URL${NC}"
echo -e "URL Swagger: ${YELLOW}$WEBAPP_URL/swagger${NC}"
echo -e "Servidor SQL: ${YELLOW}${DB_SERVER_NAME}.database.windows.net${NC}"
echo -e "Banco de Dados: ${YELLOW}$DB_NAME${NC}"
echo -e "Usuário: ${YELLOW}$DB_ADMIN_USER${NC}"
echo ""
echo "Comandos úteis para debug:"
echo "  Ver logs: az webapp log tail --resource-group $RESOURCE_GROUP --name $WEB_APP_NAME"
echo "  SSH: az webapp ssh --resource-group $RESOURCE_GROUP --name $WEB_APP_NAME"
echo "  Status: az webapp show --resource-group $RESOURCE_GROUP --name $WEB_APP_NAME --query state"
echo ""
echo "Aguarde 2-3 minutos para a aplicação iniciar completamente."
echo -e "${GREEN}======================================================${NC}"