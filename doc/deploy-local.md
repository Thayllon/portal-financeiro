# Deploy local — tudo em um link

> Suba **todo o projeto** (SQL Server + API + Frontend) em containers Docker e acesse
> por **um único link**: `http://localhost:8080`. Nada de instalar .NET, Node ou SQL
> Server na sua máquina.

É o caminho mais rápido para validar o sistema de ponta a ponta antes de partir para
o deploy em nuvem ([infra.md](infra.md)).

## Pré-requisitos

- **Docker Desktop** (Windows/Mac) ou Docker Engine (Linux) — `docker --version`

## Passo a passo

### 1. Prepare o arquivo de variáveis

O projeto não versiona segredos. Copie o modelo e preencha as senhas:

```powershell
Copy-Item .env.example .env
```

Edite o `.env`:

```dotenv
# Deploy local
SA_PASSWORD=TroqueEstaSenha@2026
JWT_SECRET=troque-este-segredo-por-um-texto-longo-e-aleatorio
```

> O `.env` está no `.gitignore` — nunca é commitado.

### 2. Suba a stack

```powershell
docker compose -f docker-compose.local.yml up -d --build
```

O Compose executa na ordem:

| Ordem | Serviço | O que faz |
|-------|---------|-----------|
| 1 | `sqlserver` | Sobe o SQL Server 2022 (porta `1433`) |
| 2 | `migrate` | Aplica `scripts/sqlserver` (schema + seed) e encerra |
| 3 | `api` | Sobe o backend .NET (porta interna `8080`) |
| 4 | `web` | Sobe o Angular servido por Nginx, com proxy `/api → api` |

### 3. Acesse

- **Frontend:** `http://localhost:8080`
- **API (Swagger):** `http://localhost:8080/api` via proxy — para ver o Swagger direto,
  rode a API localmente conforme [primeiros-passos.md](primeiros-passos.md)

## Login padrão

| Campo | Valor |
|-------|-------|
| Email | `admin@portal.com` |
| Senha | `senhasenha` |

## Verificação

1. `docker compose -f docker-compose.local.yml ps` — todos os serviços `Up` (migrate fica `Exited (0)`)
2. `http://localhost:8080` abre o login
3. Login com admin → dashboard carrega sem erros no console do navegador
4. Logs da API: `docker compose -f docker-compose.local.yml logs -f api`

## Comandos úteis

```powershell
# Ver logs de um serviço
docker compose -f docker-compose.local.yml logs -f api

# Parar (mantém o banco)
docker compose -f docker-compose.local.yml stop

# Remover containers (mantém o volume do banco)
docker compose -f docker-compose.local.yml down

# Remover containers + apagar o banco (recomeçar do zero)
docker compose -f docker-compose.local.yml down -v
```

## Como funciona o proxy

O Nginx (`web`) serve o build de produção do Angular, que aponta para `apiUrl: '/api'`
(`environment.prod.ts`). O Nginx repassa `/api/...` para o container `api:8080`:

```
navegador → http://localhost:8080          → arquivos Angular (SPA)
          → http://localhost:8080/api/...  → proxy → api:8080/api/...
```

Config em `src/PortalFinanceiro.Web/nginx.conf`.

## Troubleshooting

| Problema | Solução |
|----------|---------|
| `port is already allocated` | Porta `8080` em uso — troque `"8080:80"` em `docker-compose.local.yml` |
| SQL Server não inicia no macOS/ARM | A imagem `mssql/server:2022` exige x86_64; use um Mac Intel/Windows |
| Primeiro build demora | Normal: baixa imagens e compila .NET + Angular (5–15 min) |
| `migrate` com erro | `docker compose -f docker-compose.local.yml logs migrate` |
| Quer recriar o banco | `down -v` e `up -d` novamente |

## Sem Docker? Sem problemas

Se preferir rodar com as ferramentas locais (LocalDB + `dotnet` + `npm`), veja
[primeiros-passos.md](primeiros-passos.md).
