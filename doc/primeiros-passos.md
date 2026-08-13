# Primeiros passos — rodando do zero

Guia para rodar o projeto em um ambiente novo (primeira vez ou depois de perder tudo).

## Pré-requisitos

- **.NET SDK 11** (`dotnet --version`)
- **Node.js + npm** (versão compatível com Angular 22)
- **SQL Server LocalDB** (instância `(localdb)\MSSQLLocalDB`)

## Ordem de execução

### 1. Banco de dados

```bash
dotnet run --project tools/DbSetup
```

Cria o banco `PortalFinanceiro` no LocalDB e aplica `scripts/sqlserver` (schema + seed).
O seed cria o admin e as categorias fiscais `CNPJ → DAS`.

> Os scripts são "from scratch" — para banco novo apenas. Não rodar sobre banco já migrado.

### 2. Backend (API)

```bash
dotnet run --project src/PortalFinanceiro.API
```

- API em `http://localhost:5178`
- Swagger em `http://localhost:5178/swagger`

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

1. Banco criado: `dotnet run --project tools/DbSetup` termina com sucesso e sem erros
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

Caso precise recriar o banco LocalDB:

```powershell
# Listar instâncias
sqllocaldb info

# Apagar o banco pelo SQL (ex.: sqlcmd) e rodar o DbSetup de novo
dotnet run --project tools/DbSetup
```

## Detalhes por área

- Backend: [back.md](back.md)
- Frontend: [front.md](front.md)
- Banco: [banco.md](banco.md)
- Infra: [infra.md](infra.md)
