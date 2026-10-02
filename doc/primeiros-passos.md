# Primeiros passos — rodando do zero

Guia para rodar o projeto em um ambiente novo (primeira vez ou depois de perder tudo).

## Pré-requisitos

- **.NET SDK 11** (`dotnet --version`)
- **Node.js + npm** (versão compatível com Angular 22)
- **PostgreSQL 16+** — local via réplica portátil (`D:\projetos\postgres-replica`) ou Docker (`docker-compose.local.yml`)

## Ordem de execução

### 1. Banco de dados

Suba o PostgreSQL local e crie o schema + dados:

```powershell
# 1) Subir o servidor (auto-start no logon, ou manualmente)
D:\projetos\postgres-replica\iniciar.bat

# 2) Banco novo — aplicar scripts/postgres/ na ordem:
#    001, 003, 004, 005, 006, 100, 103, 104, 105, 106
psql -U postgres -p 5432 -d portal_financeiro -f scripts/postgres/001_CriarTabelas.sql
# ... (aplicar os demais)

# 3) OU sincronizar do PostgreSQL local para o Neon (produção)
.\sincronizar-banco.ps1 -DryRun   # revisar antes de aplicar
```

> Se o banco `portal_financeiro` já existe na réplica local (vinda do `sincronizar-banco.ps1`),
> ele já tem schema + dados e basta subir o servidor.

### 2. Backend (API)

Defina a chave secreta JWT antes de iniciar (não há segredo versionado no `appsettings.json`):

```powershell
$env:Auth__Secret = "um-segredo-aleatorio-forte"
dotnet run --project src/PortalFinanceiro.API
```

- API em `http://localhost:5178`
- Swagger em `http://localhost:5178/swagger`

> Em docker (`docker-compose*.yml`), a chave vem da variável `JWT_SECRET` do `.env` e é injetada como `Auth__Secret`. Para rodar via `dotnet run` (PostgreSQL local), defina `Auth__Secret` no ambiente.

### 3. Frontend

```bash
cd src/PortalFinanceiro.Web
npm install       # só na primeira vez
npm start         # ng serve → http://localhost:4200
```

## Login padrão (dev)

| Campo | Valor |
|-------|-------|
| Email | `admin@portal.com` |
| Senha | `senhasenha` |

## Verificando se está tudo pronto

1. Banco de pé: `psql -U postgres -p 5432 -d portal_financeiro -c "SELECT 1"` responde
2. API de pé: Swagger abre em `http://localhost:5178/swagger`
3. Frontend no ar: `http://localhost:4200` abre o login
4. Login com admin → dashboard carrega sem erros no console

## Build e testes

```bash
# Backend
dotnet build PortalFinanceiro.API.slnx

# Frontend
cd src/PortalFinanceiro.Web
npm run build
npm test
```

## Recomeçar o banco do zero (opcional)

Caso precise recriar o banco PostgreSQL local:

```powershell
# Parar a réplica, apagar a pasta de dados e reiniciar (initdb), ou
# simplesmente apagar o banco e recriar aplicando os scripts do passo 1:
psql -U postgres -p 5432 -d postgres -c "DROP DATABASE portal_financeiro"
psql -U postgres -p 5432 -d postgres -c "CREATE DATABASE portal_financeiro"
```

## Detalhes por área

- Backend: [back.md](back.md)
- Frontend: [front.md](front.md)
- Banco: [banco.md](banco.md)
- Infra: [infra.md](infra.md)