# WorkWell API - Guia de Deploy

## 📦 Pré-requisitos de Infraestrutura

### Banco de Dados Oracle

**Opção 1: Oracle Cloud (Recomendado)**

1. Crie uma conta no Oracle Cloud Free Tier
2. Provisione um Autonomous Database
3. Configure o wallet para conexão segura
4. Execute os scripts SQL de criação das tabelas

**Opção 2: Oracle XE Local**

```bash
# Docker
docker run -d -p 1521:1521 -e ORACLE_PASSWORD=oracle gvenzl/oracle-xe:latest

# Aguarde alguns minutos para inicialização
docker logs -f <container_id>
```

### MongoDB

**Opção 1: MongoDB Atlas (Recomendado)**

1. Crie uma conta gratuita em mongodb.com/atlas
2. Crie um cluster gratuito
3. Adicione seu IP à whitelist
4. Obtenha a connection string

**Opção 2: MongoDB Local**

```bash
docker run -d -p 27017:27017 --name mongodb mongo:latest
```

### Redis

**Opção 1: Azure Redis Cache (Recomendado para produção)**

1. Crie um Azure Redis Cache no portal
2. Obtenha a connection string

**Opção 2: Redis Local**

```bash
docker run -d -p 6379:6379 --name redis redis:alpine
```

## 🚀 Deploy em Azure App Service

### Passo 1: Criar Recursos no Azure

```bash
# Login no Azure
az login

# Criar Resource Group
az group create --name rg-workwell --location brazilsouth

# Criar App Service Plan (Free ou Basic)
az appservice plan create \
  --name plan-workwell \
  --resource-group rg-workwell \
  --sku B1 \
  --is-linux

# Criar Web App
az webapp create \
  --name workwell-api-fiap \
  --resource-group rg-workwell \
  --plan plan-workwell \
  --runtime "DOTNETCORE:8.0"
```

### Passo 2: Configurar Variáveis de Ambiente

```bash
az webapp config appsettings set \
  --name workwell-api-fiap \
  --resource-group rg-workwell \
  --settings \
    ConnectionStrings__OracleConnection="User Id=admin;Password=xxx;Data Source=..." \
    ConnectionStrings__MongoDbConnection="mongodb+srv://user:pass@cluster.mongodb.net" \
    ConnectionStrings__RedisConnection="workwell.redis.cache.windows.net:6380,password=xxx,ssl=True" \
    Jwt__SecretKey="sua-chave-super-secreta-aqui-min-32-chars" \
    Jwt__Issuer="WorkWellAPI" \
    Jwt__Audience="WorkWellClient" \
    Gemini__ApiKey="sua-chave-api-gemini"
```

### Passo 3: Deploy da Aplicação

**Opção A: Via Visual Studio**

1. Clique com o botão direito no projeto WorkWell.API
2. Selecione "Publish"
3. Escolha "Azure" -> "Azure App Service (Linux)"
4. Selecione sua subscription e o App Service criado
5. Clique em "Publish"

**Opção B: Via CLI**

```bash
# Na pasta raiz do projeto
dotnet publish WorkWell.API/WorkWell.API.csproj -c Release -o ./publish

# Criar zip
cd publish
zip -r ../workwell.zip .
cd ..

# Deploy
az webapp deployment source config-zip \
  --resource-group rg-workwell \
  --name workwell-api-fiap \
  --src workwell.zip
```

### Passo 4: Executar Migrations

```bash
# Localmente, apontando para o banco de produção
dotnet ef database update --project WorkWell.Infrastructure --startup-project WorkWell.API

# Ou via connection string direta
dotnet ef database update --connection "User Id=admin;Password=xxx;Data Source=..." --project WorkWell.Infrastructure
```

### Passo 5: Verificar Deployment

```bash
# Health Check
curl https://workwell-api-fiap.azurewebsites.net/health

# Swagger
https://workwell-api-fiap.azurewebsites.net/swagger
```

## 🐳 Deploy com Docker

### Dockerfile

Já criado na raiz do projeto. Para build:

```bash
docker build -t workwell-api:latest .
docker tag workwell-api:latest yourregistry/workwell-api:latest
docker push yourregistry/workwell-api:latest
```

### Docker Compose (Desenvolvimento)

```yaml
version: '3.8'

services:
  oracle:
    image: gvenzl/oracle-xe:latest
    environment:
      - ORACLE_PASSWORD=oracle
    ports:
      - "1521:1521"
    volumes:
      - oracle-data:/opt/oracle/oradata

  mongodb:
    image: mongo:latest
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db

  redis:
    image: redis:alpine
    ports:
      - "6379:6379"

  api:
    build: .
    ports:
      - "8080:80"
    environment:
      - ConnectionStrings__OracleConnection=User Id=system;Password=oracle;Data Source=oracle:1521/XE
      - ConnectionStrings__MongoDbConnection=mongodb://mongodb:27017
      - ConnectionStrings__RedisConnection=redis:6379
      - Jwt__SecretKey=your-super-secret-key-here-min-32-chars
      - Gemini__ApiKey=your-gemini-api-key
    depends_on:
      - oracle
      - mongodb
      - redis

volumes:
  oracle-data:
  mongo-data:
```

Executar:

```bash
docker-compose up -d
```

## ☸️ Deploy em Kubernetes

### Manifests

**deployment.yaml**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: workwell-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: workwell-api
  template:
    metadata:
      labels:
        app: workwell-api
    spec:
      containers:
      - name: api
        image: yourregistry/workwell-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__OracleConnection
          valueFrom:
            secretKeyRef:
              name: workwell-secrets
              key: oracle-connection
        - name: ConnectionStrings__MongoDbConnection
          valueFrom:
            secretKeyRef:
              name: workwell-secrets
              key: mongo-connection
        - name: ConnectionStrings__RedisConnection
          valueFrom:
            secretKeyRef:
              name: workwell-secrets
              key: redis-connection
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: workwell-api
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 80
  selector:
    app: workwell-api
```

**secrets.yaml**

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: workwell-secrets
type: Opaque
stringData:
  oracle-connection: "User Id=admin;Password=xxx;Data Source=..."
  mongo-connection: "mongodb+srv://user:pass@cluster.mongodb.net"
  redis-connection: "redis:6379"
  jwt-secret: "your-super-secret-key-here"
  gemini-api-key: "your-gemini-key"
```

Aplicar:

```bash
kubectl apply -f secrets.yaml
kubectl apply -f deployment.yaml
```

## 📊 Monitoramento em Produção

### Application Insights (Azure)

Adicionar pacote:

```bash
dotnet add WorkWell.API package Microsoft.ApplicationInsights.AspNetCore
```

No `Program.cs`:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Prometheus + Grafana

Adicionar pacote:

```bash
dotnet add WorkWell.API package prometheus-net.AspNetCore
```

No `Program.cs`:

```csharp
app.UseHttpMetrics();
app.MapMetrics();
```

## 🔐 Segurança em Produção

### Checklist

- [ ] Usar HTTPS obrigatório
- [ ] Configurar HSTS
- [ ] Habilitar security headers
- [ ] Usar Azure Key Vault para secrets
- [ ] Configurar CORS restritivamente
- [ ] Habilitar rate limiting
- [ ] Configurar WAF (Web Application Firewall)
- [ ] Implementar log de auditoria
- [ ] Configurar alertas de segurança

### Azure Key Vault

```bash
# Criar Key Vault
az keyvault create \
  --name kv-workwell \
  --resource-group rg-workwell \
  --location brazilsouth

# Adicionar secrets
az keyvault secret set \
  --vault-name kv-workwell \
  --name "OracleConnection" \
  --value "User Id=admin;Password=xxx;..."

# Dar permissão ao App Service
az webapp identity assign \
  --resource-group rg-workwell \
  --name workwell-api-fiap

# Configurar acesso
az keyvault set-policy \
  --name kv-workwell \
  --object-id <app-identity-id> \
  --secret-permissions get list
```

## 📈 Performance e Escalabilidade

### Auto-scaling no Azure

```bash
az monitor autoscale create \
  --resource-group rg-workwell \
  --resource workwell-api-fiap \
  --resource-type Microsoft.Web/serverfarms \
  --name autoscale-workwell \
  --min-count 1 \
  --max-count 5 \
  --count 2

az monitor autoscale rule create \
  --resource-group rg-workwell \
  --autoscale-name autoscale-workwell \
  --condition "CpuPercentage > 70 avg 5m" \
  --scale out 1
```

### CDN para Assets Estáticos

```bash
az cdn profile create \
  --resource-group rg-workwell \
  --name cdn-workwell \
  --sku Standard_Microsoft

az cdn endpoint create \
  --resource-group rg-workwell \
  --profile-name cdn-workwell \
  --name workwell-api \
  --origin workwell-api-fiap.azurewebsites.net
```

## 🔄 CI/CD

### GitHub Actions

Criar `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --no-restore --verbosity normal
    
    - name: Publish
      run: dotnet publish WorkWell.API/WorkWell.API.csproj -c Release -o ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'workwell-api-fiap'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

## 📝 Checklist de Deploy

- [ ] Banco de dados Oracle configurado e migrations executadas
- [ ] MongoDB configurado
- [ ] Redis configurado
- [ ] Variáveis de ambiente configuradas
- [ ] JWT Secret configurado
- [ ] Gemini API Key configurada
- [ ] CORS configurado para domínios corretos
- [ ] Health checks funcionando
- [ ] Logs configurados
- [ ] Backup configurado
- [ ] Monitoramento configurado
- [ ] SSL/TLS configurado
- [ ] Documentação Swagger acessível
- [ ] Testes executados com sucesso

## 🆘 Troubleshooting

### Erro de Conexão Oracle

```
ORA-12154: TNS:could not resolve the connect identifier specified
```

**Solução**: Verificar connection string e disponibilidade do banco

### Erro de Autenticação MongoDB

```
MongoAuthenticationException: Unable to authenticate
```

**Solução**: Verificar usuário, senha e whitelist de IPs

### Health Check Failing

```bash
# Verificar logs
az webapp log tail --name workwell-api-fiap --resource-group rg-workwell

# Verificar configurações
az webapp config appsettings list --name workwell-api-fiap --resource-group rg-workwell
```

---

Para suporte adicional, consulte a documentação ou abra uma issue no repositório.

