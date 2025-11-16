Challenge FIAP: API WorkWell
Mostrar Imagem
Mostrar Imagem
Mostrar Imagem
Diagrama da solução
Mostrar Imagem
📋 1. Visão Geral da Solução
A API WorkWell é o backend central para monitoramento de saúde mental e produtividade em ambientes de trabalho híbrido. Desenvolvida em .NET 8 com uma arquitetura limpa, a solução oferece endpoints RESTful para gerir empresas, usuários, check-ins diários e predição de burnout. O projeto foi implementado na nuvem da Microsoft Azure para garantir alta disponibilidade, escalabilidade e segurança.
Principais Benefícios

Prevenção de Burnout: Sistema preditivo com Machine Learning que identifica padrões de risco antecipadamente.
Centralização da Informação: Um único ponto de verdade para todos os dados de bem-estar corporativo.
Otimização da Saúde Mental: Reduz a complexidade do monitoramento diário de colaboradores.
Escalabilidade e Integração: Preparada para crescer e ser consumida por diversas aplicações clientes (Mobile, Web, BI).


Vídeo explicativo da solução:

[Link do vídeo] (adicionar após gravação)


🏗️ 2. Arquitetura da Infraestrutura
A solução foi implementada utilizando o modelo PaaS (Plataforma como Serviço) da Azure, com todos os recursos provisionados via Azure CLI para garantir a automação e a rastreabilidade (Infraestrutura como Código).
mermaidgraph TD;
    A[<B>GitHub Repository</B><br>seu-usuario/workwell-dotnet] -->|1. Código-Fonte| B(
        <B>Azure App Service</B>
        <br>
        <i>webapp-workwell-prod</i>
        <br>
        Hospeda a API .NET
    );
    B -->|2. Lê/Escreve Dados| C(
        <B>Oracle Database</B>
        <br>
        <i>workwelldb</i>
        <br>
        Armazena dados relacionais
    );
    B -->|3. Cache| D(
        <B>Azure Cache for Redis</B>
        <br>
        <i>redis-workwell</i>
        <br>
        Cache distribuído
    );
    B -->|4. NoSQL| E(
        <B>MongoDB Atlas</B>
        <br>
        <i>workwell-cluster</i>
        <br>
        Conversas IA
    );
    F{Utilizador / App Cliente} -->|5. Requisições HTTPS| B;

GitHub Repository: Contém o código-fonte da aplicação .NET.
Azure App Service: Serviço PaaS que compila e hospeda a API. Está configurado para fazer o deploy automático a partir da branch main.
Oracle Database: Banco de dados relacional que armazena empresas, usuários, check-ins e alertas de burnout.
Azure Cache for Redis: Cache distribuído para otimização de performance.
MongoDB Atlas: Banco NoSQL para armazenamento de conversas do chatbot.


🚀 3. Como Realizar o Deploy
O processo de deploy está totalmente automatizado através de um único script.
Pré-requisitos

Azure CLI instalado e autenticado (az login).
Permissões para criar recursos na sua subscrição Azure.

Passos

Clone este repositório.
Abra o ficheiro deploy.sh e preencha as variáveis:

ORACLE_CONNECTION
MONGODB_CONNECTION
REDIS_CONNECTION
JWT_SECRET
GEMINI_API_KEY


Execute o script no seu terminal:

bash    chmod +x deploy.sh
    ./deploy.sh

Aguarde a finalização. A URL da sua API será exibida no final.


📚 4. Documentação da API (Endpoints)
A API expõe os seguintes endpoints principais. A URL base é https://webapp-workwell-prod.azurewebsites.net/api/v1.
Autenticação (/auth)

POST /auth/register: Registra um novo usuário.
POST /auth/login: Realiza login e retorna token JWT.
POST /auth/refresh: Renova o token de acesso.
Exemplo de Body (Login):

json    {
      "email": "usuario@empresa.com",
      "senha": "SenhaSegura123"
    }
Check-ins Diários (/checkins)

GET /checkins/me: Lista meus check-ins (com paginação).
POST /checkins: Cria um novo check-in diário.
GET /checkins/{id}: Obtém um check-in específico.
Exemplo de Body:

json    {
      "nivelStress": 6,
      "horasTrabalhadas": 9.5,
      "horasSono": 6.5,
      "sentimento": "Cansado",
      "observacoes": "Dia muito corrido"
    }
Predição de Burnout (/burnout)

GET /burnout/predict/me: Analisa meu risco de burnout.
GET /burnout/predict/{id}: Analisa risco de um usuário (Admin).
Exemplo de Resposta:

json    {
      "usuarioId": 123,
      "scoreRisco": 0.72,
      "nivelRisco": "ALTO",
      "fatoresContribuintes": [
        {
          "fator": "privacao_sono",
          "importancia": 0.34
        }
      ],
      "recomendacoes": [
        {
          "acao": "melhorar_higiene_sono",
          "prioridade": "alta"
        }
      ]
    }
IA Generativa (/aiassistant)

POST /aiassistant/chat: Conversa com o assistente virtual.
POST /aiassistant/recommendations: Obtém recomendações personalizadas.
Exemplo de Body:

json    {
      "mensagem": "Estou me sentindo sobrecarregado",
      "sessionId": "session-abc-123"
    }
Health Checks (/health)

GET /health: Status completo da aplicação.
GET /health/live: Liveness probe.
GET /health/ready: Readiness probe.


📝 5. Scripts de Entrega

deploy.sh: Script de criação da infraestrutura e deploy.
script_bd.sql: Script DDL para a criação da estrutura do banco de dados Oracle.