# Deploy local — tudo em um link

> Suba **todo o projeto** (PostgreSQL + API + Frontend) em containers Docker e acesse
> por **um único link**: `http://localhost:8080`. Nada de instalar .NET, Node ou
> PostgreSQL na sua máquina.

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
POSTGRES_USER=postgres
POSTGRES_PASSWORD=TroqueEstaSenha@2026
POSTGRES_DB=portal_financeiro
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
| 1 | `db` | Sobe o PostgreSQL 17 (porta `5432`) |
| 2 | `api` | Sobe o backend .NET (porta interna `8080`) |
| 3 | `web` | Sobe o Angular servido por Nginx, com proxy `/api → api` |

> O `db` inicia vazio: para schema + dados, rode os scripts de `scripts/postgres/`
> (em banco novo: `001` → `003` → `004` → `005` → `006` → `100` → `103` → `104` → `105` → `106`)
> ou use `sincronizar-banco.ps1` para sincronizar do PostgreSQL local para o Neon.

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

1. `docker compose -f docker-compose.local.yml ps` — todos os serviços `Up`
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

O Nginx (`web`) serve o build Angular para containers, compilado com `npm run build:docker`
(configuração `docker` em `angular.json`, `apiUrl: '/api'` via `environment.docker.ts`).
O Nginx repassa `/api/...` para o container `api:8080`:

```
navegador → http://localhost:8080          → arquivos Angular (SPA)
          → http://localhost:8080/api/...  → proxy → api:8080/api/...
```

Config em `src/PortalFinanceiro.Web/nginx.conf`.

## Troubleshooting

| Problema | Solução |
|----------|---------|
| `port is already allocated` | Porta `8080` em uso — troque `"8080:80"` em `docker-compose.local.yml` |
| Primeiro build demora | Normal: baixa imagens e compila .NET + Angular (5–15 min) |
| `db` sem schema | Aplique `scripts/postgres/001..106` ou rode `sincronizar-banco.ps1` |
| Quer recriar o banco | `down -v` e `up -d` novamente |

## Sem Docker? Sem problemas

Se preferir rodar com as ferramentas locais (PostgreSQL local + `dotnet` + `npm`), veja
[primeiros-passos.md](primeiros-passos.md).
