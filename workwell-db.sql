-- Script SQL para criação do esquema do banco de dados WorkWell (Azure SQL Database)
-- Baseado nas entidades definidas no WorkWellDbContext.cs

-- A aplicação .NET usa a convenção de nomenclatura Oracle (tabelas e colunas em maiúsculas)
-- O EF Core cuidará da criação das tabelas e migrations, mas este script serve como referência e para dados iniciais.

-- -----------------------------------------------------
-- Tabela EMPRESAS
-- -----------------------------------------------------
CREATE TABLE EMPRESAS (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    NOME NVARCHAR(200) NOT NULL,
    CNPJ NVARCHAR(14) NOT NULL UNIQUE,
    SETOR NVARCHAR(100),
    DATA_CADASTRO DATETIME2 NOT NULL,
    DATA_CRIACAO DATETIME2 NOT NULL,
    DATA_ATUALIZACAO DATETIME2
);

-- -----------------------------------------------------
-- Tabela DEPARTAMENTOS
-- -----------------------------------------------------
CREATE TABLE DEPARTAMENTOS (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    NOME NVARCHAR(100) NOT NULL,
    EMPRESA_ID INT NOT NULL,
    DATA_CRIACAO DATETIME2 NOT NULL,
    DATA_ATUALIZACAO DATETIME2,
    FOREIGN KEY (EMPRESA_ID) REFERENCES EMPRESAS(ID) ON DELETE CASCADE
);

-- -----------------------------------------------------
-- Tabela USUARIOS
-- -----------------------------------------------------
CREATE TABLE USUARIOS (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    NOME NVARCHAR(200) NOT NULL,
    EMAIL NVARCHAR(200) NOT NULL UNIQUE,
    SENHA_HASH NVARCHAR(MAX) NOT NULL,
    EMPRESA_ID INT NOT NULL,
    DEPARTAMENTO_ID INT,
    CARGO NVARCHAR(100),
    ROLE INT NOT NULL, -- 0: USER, 1: ADMIN, 2: SUPERADMIN
    ATIVO BIT NOT NULL,
    DATA_ULTIMO_ACESSO DATETIME2,
    DATA_CRIACAO DATETIME2 NOT NULL,
    DATA_ATUALIZACAO DATETIME2,
    FOREIGN KEY (EMPRESA_ID) REFERENCES EMPRESAS(ID) ON DELETE RESTRICT,
    FOREIGN KEY (DEPARTAMENTO_ID) REFERENCES DEPARTAMENTOS(ID) ON DELETE SET NULL
);

-- -----------------------------------------------------
-- Tabela CHECKINS_DIARIOS
-- -----------------------------------------------------
CREATE TABLE CHECKINS_DIARIOS (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    USUARIO_ID INT NOT NULL,
    DATA_CHECKIN DATE NOT NULL,
    HORAS_TRABALHADAS DECIMAL(5, 2) NOT NULL,
    HORAS_SONO DECIMAL(5, 2) NOT NULL,
    NIVEL_ESTRESSE INT NOT NULL, -- 1 a 5
    SCORE_BEM_ESTAR DECIMAL(5, 2) NOT NULL,
    OBSERVACOES NVARCHAR(MAX),
    DATA_CRIACAO DATETIME2 NOT NULL,
    DATA_ATUALIZACAO DATETIME2,
    FOREIGN KEY (USUARIO_ID) REFERENCES USUARIOS(ID) ON DELETE CASCADE,
    UNIQUE (USUARIO_ID, DATA_CHECKIN)
);

-- -----------------------------------------------------
-- Tabela METRICAS_SAUDE
-- -----------------------------------------------------
CREATE TABLE METRICAS_SAUDE (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    USUARIO_ID INT NOT NULL,
    DATA_REGISTRO DATE NOT NULL,
    LITROS_AGUA DECIMAL(4, 2) NOT NULL,
    PESO_KG DECIMAL(6, 2) NOT NULL,
    ATIVIDADE_FISICA_MIN INT NOT NULL,
    DATA_CRIACAO DATETIME2 NOT NULL,
    DATA_ATUALIZACAO DATETIME2,
    FOREIGN KEY (USUARIO_ID) REFERENCES USUARIOS(ID) ON DELETE CASCADE,
    UNIQUE (USUARIO_ID, DATA_REGISTRO)
);

-- -----------------------------------------------------
-- Tabela ALERTAS_BURNOUT
-- -----------------------------------------------------
CREATE TABLE ALERTAS_BURNOUT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    USUARIO_ID INT NOT NULL,
    DATA_ALERTA DATETIME2 NOT NULL,
    NIVEL_RISCO INT NOT NULL, -- 0: BAIXO, 1: MEDIO, 2: ALTO
    SCORE_RISCO DECIMAL(5, 2) NOT NULL,
    MENSAGEM NVARCHAR(MAX) NOT NULL,
    LIDO BIT NOT NULL,
    DATA_CRIACAO DATETIME2 NOT NULL,
    DATA_ATUALIZACAO DATETIME2,
    FOREIGN KEY (USUARIO_ID) REFERENCES USUARIOS(ID) ON DELETE CASCADE
);

-- -----------------------------------------------------
-- Dados Iniciais (Opcional, para testes)
-- -----------------------------------------------------

-- Inserir Empresa
INSERT INTO EMPRESAS (NOME, CNPJ, SETOR, DATA_CADASTRO, DATA_CRIACAO)
VALUES ('WorkWell Tech Solutions', '12345678000190', 'Tecnologia', GETDATE(), GETDATE());

-- Inserir Departamentos
INSERT INTO DEPARTAMENTOS (NOME, EMPRESA_ID, DATA_CRIACAO)
VALUES ('Desenvolvimento', 1, GETDATE());

INSERT INTO DEPARTAMENTOS (NOME, EMPRESA_ID, DATA_CRIACAO)
VALUES ('Recursos Humanos', 1, GETDATE());

-- Inserir Usuários (SuperAdmin e User)
-- Senha: 'Senhaforte123!' (Hash deve ser gerado pela aplicação)
-- Como este é um script de schema, a senha será um placeholder.
INSERT INTO USUARIOS (NOME, EMAIL, SENHA_HASH, EMPRESA_ID, DEPARTAMENTO_ID, CARGO, ROLE, ATIVO, DATA_CRIACAO)
VALUES ('Admin WorkWell', 'admin@workwell.com', 'HASH_PLACEHOLDER_SUPERADMIN', 1, 2, 'CEO', 2, 1, GETDATE());

INSERT INTO USUARIOS (NOME, EMAIL, SENHA_HASH, EMPRESA_ID, DEPARTAMENTO_ID, CARGO, ROLE, ATIVO, DATA_CRIACAO)
VALUES ('Joao Desenvolvedor', 'joao@workwell.com', 'HASH_PLACEHOLDER_USER', 1, 1, 'Desenvolvedor Pleno', 0, 1, GETDATE());

-- Inserir Checkin Diário (Exemplo)
INSERT INTO CHECKINS_DIARIOS (USUARIO_ID, DATA_CHECKIN, HORAS_TRABALHADAS, HORAS_SONO, NIVEL_ESTRESSE, SCORE_BEM_ESTAR, DATA_CRIACAO)
VALUES (2, GETDATE(), 8.5, 7.0, 3, 4.2, GETDATE());

-- Inserir Métrica de Saúde (Exemplo)
INSERT INTO METRICAS_SAUDE (USUARIO_ID, DATA_REGISTRO, LITROS_AGUA, PESO_KG, ATIVIDADE_FISICA_MIN, DATA_CRIACAO)
VALUES (2, GETDATE(), 2.5, 75.5, 60, GETDATE());

-- Inserir Alerta de Burnout (Exemplo)
INSERT INTO ALERTAS_BURNOUT (USUARIO_ID, DATA_ALERTA, NIVEL_RISCO, SCORE_RISCO, MENSAGEM, LIDO, DATA_CRIACAO)
VALUES (2, GETDATE(), 1, 65.8, 'Risco médio de burnout detectado com base nos últimos checkins.', 0, GETDATE());