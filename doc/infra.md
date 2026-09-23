# Infraestrutura — Portal Financeiro

> Produção atual: **Neon** (PostgreSQL) + **Render** (API .NET) + **Vercel** (Angular).
> Publicado em `https://portal-financeiro-alpha.vercel.app/`. Veja os 3 ambientes abaixo.

## Ambientes publicados — Vercel + Render + Neon

Stack real em produção: **Neon** (banco) + **Render** (backend) + **Vercel** (frontend).

### Ambientes

| Ambiente | Front (Vercel) | Back (Render) | Banco (Neon) | Branch git | Uso |
|----------|----------------|---------------|--------------|------------|-----|
| **alpha** | portal-financeiro-alpha.vercel.app | portal-financeiro-p57h.onrender.com | **banco prod** | `main` | pessoal (thayllon e alana) |
| **bravo** | portal-financeiro-bravo.vercel.app | portal-financeiro-bravo.onrender.com | **banco separado** (isolado) | `main` | portfólio (joão e maria) |
| **charlie** | portal-financeiro-charlie.vercel.app | portal-financeiro-e0xw.onrender.com | **banco prod** | `develop` | dev |

> **Banco:** alpha e charlie usam o **banco prod**; **bravo usa um projeto Neon separado**
> (isolado, mais seguro para o portfólio). Frontend: **cada ambiente tem o próprio build**
> (a URL do backend é embutida por arquivo de ambiente):
> - alpha → `environment.prod.ts` (`npm run build`)
> - bravo → `environment.bravo.ts` (`npm run build:bravo`)
> - charlie → `environment.charlie.ts` (`npm run build:charlie`)

### Custo real (planos free, 2026)

| Plataforma | Plano | Limites | Notas |
|------------|-------|---------|-------|
| **Vercel** | Hobby ($0) | 200 projetos, 25 projetos/repo, 100 deploys/dia | Uso pessoal/não-comercial; domínio `.vercel.app` derivado do nome do projeto (escolhível, único global) |
| **Render** | Hobby ($0) | até 25 serviços; **750h/mês por workspace** | Free dorme após 15min de inatividade; horas compartilhadas entre **todos** os serviços free do workspace |
| **Neon** | Free ($0) | 100 projetos, 10 branches/projeto, 0,5 GB/projeto, 100 CU-h/mês | Escala a zero após 5min; storage/compute são por projeto |

⚠️ **Render: as 750h/mês são compartilhadas por workspace** — os 3 backends free gastam do
mesmo balde de horas. **Não** manter keep-alive pingando os 3 serviços (devora o balde e pode
suspender todos). Uso esparso (demo/portfólio) fica perto de R$ 0; cold start após dormir ~30–60s.

### Passo a passo — criar cada ambiente

Pré-requisito comum: o repositório publicado no GitHub e as contas em **Neon**, **Render** e
**Vercel** já criadas.

#### Ambiente alpha (pessoal — thayllon e alana)

1. **Banco (Neon):** usar o **projeto prod já existente** — nada a criar.
2. **Backend (Render):**
   - Dashboard → **New → Web Service** (ou **Blueprint** com `render.yaml`) → conectar o repo.
   - Branch: **`main`** · Runtime: **Docker** (`Dockerfile`).
   - Env vars: `ConnectionStrings__DefaultConnection` = conn prod, `Auth__Secret` = segredo 1,
     `Cors__AllowedOrigins` = `https://portal-financeiro-alpha.vercel.app`,
     `Database__Provider` = `Postgres`, `ASPNETCORE_ENVIRONMENT` = `Production`.
   - Anotar a URL (ex.: `https://portal-financeiro-p57h.onrender.com`).
3. **Frontend (Vercel):**
   - **Add New → Project** → importar o repo → nome **`portal-financeiro-alpha`**.
   - Root Directory: **`src/PortalFinanceiro.Web`** · Production Branch: **`main`**.
   - Build Command: **`npm run build`** (usa `environment.prod.ts`).
   - Conferir se `environment.prod.ts` aponta para a URL do backend alpha.
4. Testar login e o fluxo.

#### Ambiente bravo (portfólio)

> Banco **separado** (isolado) — criar um projeto Neon próprio.

1. **Banco (Neon):** criar um **novo projeto Neon** (limite free: 100) → anotar a connection
   string. Aplicar schema/seed nele: `001_CriarTabelas`, `099_SeedBase` (usuários reais
   criados via app).
2. **Backend (Render):** novo Web Service → branch **`main`** → Docker.
   - Env vars: `ConnectionStrings__DefaultConnection` = conn do banco bravo,
     `Auth__Secret` = segredo 2, `Cors__AllowedOrigins` = `https://portal-financeiro-bravo.vercel.app`.
3. **Frontend (Vercel):** novo projeto **`portal-financeiro-bravo`** → Root Directory
   `src/PortalFinanceiro.Web` → Production Branch `main` → Build Command
   **`npm run build:bravo`** (usa `environment.bravo.ts` — ajuste o `apiUrl` para o backend bravo).
4. Testar com as credenciais do portfólio (maria/joão).

#### Ambiente charlie (dev — espelha develop)

1. **Banco (Neon):** usar o **mesmo banco prod** que o alpha (ou criar um **branch** Neon
   p/ não poluir os dados pessoais).
2. **Backend (Render):** novo Web Service → branch **`develop`** → Docker.
   - `ConnectionStrings__DefaultConnection` = conn prod; `Auth__Secret` = segredo 3;
     `Cors__AllowedOrigins` = `https://portal-financeiro-charlie.vercel.app`.
3. **Frontend (Vercel):** novo projeto **`portal-financeiro-charlie`** → Root Directory
   `src/PortalFinanceiro.Web` → Production Branch **`develop`** → Build Command
   **`npm run build:charlie`** (usa `environment.charlie.ts` — ajuste o `apiUrl` para o backend charlie).

### Variáveis por ambiente (Render)

| Chave | alpha | bravo | charlie |
|-------|-------|-------|---------|
| `Database__Provider` | `Postgres` | `Postgres` | `Postgres` |
| `ConnectionStrings__DefaultConnection` | conn prod | **conn bravo (separada)** | conn prod |
| `Auth__Secret` | segredo 1 | segredo 2 | segredo 3 |
| `Cors__AllowedOrigins` | `https://portal-financeiro-alpha.vercel.app` | `https://portal-financeiro-bravo.vercel.app` | `https://portal-financeiro-charlie.vercel.app` |

> Frontend — `apiUrl` compilado no build (um arquivo por ambiente):
> - alpha → `src/environments/environment.prod.ts`
> - bravo → `src/environments/environment.bravo.ts`
> - charlie → `src/environments/environment.charlie.ts`
> Atualize o arquivo do ambiente com a URL do backend antes de publicar cada projeto.

### Configuração Vercel (por projeto — resumo final)

| Projeto | Root Directory | Build Command | Output Directory | Production Branch |
|---------|----------------|---------------|------------------|-------------------|
| **portal-financeiro-alpha** | `src/PortalFinanceiro.Web` | `npm run build` | `dist/portal-financeiro/browser` | `main` |
| **portal-financeiro-bravo** | `src/PortalFinanceiro.Web` | `npm run build:bravo` | `dist/portal-financeiro/browser` | `main` |
| **portal-financeiro-charlie** | `src/PortalFinanceiro.Web` | `npm run build:charlie` | `dist/portal-financeiro/browser` | `develop` |

> O `vercel.json` da **raiz** foi removido — a config válida é a de `src/PortalFinanceiro.Web/vercel.json`
> (output `dist/portal-financeiro/browser` + rewrite de SPA). Manter o Root Directory = `src/PortalFinanceiro.Web`
> em todos os projetos; usar Root = repo raiz quebra o output/rewrite.

### Atualização (deploy por branch)

- Push em `main` → alpha e bravo (front + back) fazem redeploy automático.
- Push em `develop` → charlie faz redeploy automático.
- **Só alteração nas branches REMOTAS `main`/`develop` dispara deploy** (push direto ou merge
  de PR) — mudança em branch local não dispara nada; commit local só tem efeito após o push
  para o GitHub.
- Migração/seed em banco existente é **manual** (psql / SQL editor do Neon).

### Segurança

- Segredos (connection string, JWT) só em env vars das plataformas — nunca no repo.
- Banco **bravo isolado** garante que o portfólio não vaza dados pessoais (alpha/charlie
  compartilham o banco prod; charlie pode usar um branch Neon se quiser isolar o dev).
- CORS por ambiente restringe qual front pode chamar a API.
- Vercel Hobby é **uso pessoal/não-comercial** — portfólio para clientes fica no limite da regra.

---

## Alternativa self-hosted na Oracle Cloud (legado)

> Deploy completo **na Oracle Cloud** (Always Free — **R$ 0/mês**) com
> Frontend + Backend + PostgreSQL **no mesmo servidor**, via Docker Compose.

## Visão geral

Tudo roda em **uma única VM** (1 servidor) com 3 containers:

```
                        ┌─────────────────────────────┐
  usuário  ──HTTPS──▶  │  Oracle Cloud VM (Linux)     │
                        │                             │
                        │  web (Nginx)  portas 80/443 │
                        │   ├── /      → Angular (SPA)│
                        │   └── /api/  → proxy        │
                        │                 │           │
                        │  api (.NET)   porta 8080    │
                        │   └── conectar             │
                        │                 ▼           │
                        │  db (PostgreSQL)  porta 5432│
                        │                             │
                        └─────────────────────────────┘
```

| Container | Imagem | Função |
|-----------|--------|--------|
| `web` | `nginx:alpine` | Serve o build do Angular + proxy `/api` |
| `api` | .NET 11 (Dockerfile) | API ASP.NET Core, JWT, Dapper |
| `db` | `postgres:17-alpine` | Banco de dados |

### Custo

| Item | Custo |
|------|-------|
| VM Oracle Cloud (Always Free) | **R$ 0** |
| PostgreSQL / Docker / Nginx | R$ 0 (open source) |
| Domínio | R$ 0 (usamos URL/IP da Oracle — ver [Acesso](#acesso-sem-domínio)) |
| **Total** | **R$ 0/mês** |

## Pré-requisitos

1. Conta na **Oracle Cloud Infrastructure** (OCI) — o Always Free exige cartão de crédito para validação, **não cobra nada**
2. **Docker** instalado na sua máquina (para o passo de build local, opcional)
3. Testar local primeiro: **[deploy-local.md](deploy-local.md)**

## ⚠️ Pré-requisito de código: suporte a PostgreSQL

> **Leia antes.** Hoje o backend está **acoplado a SQL Server**: `Program.cs` força
> `SqlServerDialect`, o `SqlBaseRepository` usa `SqlConnection`/`SqlException` e a
> connection factory cria `SqlConnection`. O `docker-compose.yml` de produção usa
> **PostgreSQL** — para o deploy funcionar, o código precisa primeiro ganhar suporte
> a `Npgsql`.

Checklist da migração (fora do escopo desta doc, mas necessário):

- [ ] Adicionar pacote `Npgsql` em `PortalFinanceiro.Infrastructure`
- [ ] Criar `PostgresDialect` (`ISqlDialect`) com `SchemaPrefix => ""`
- [ ] Criar `PostgresConnectionFactory` (implementa `IDatabaseConnectionFactory`)
- [ ] Desacoplar `SqlBaseRepository`: Polly de `SqlException` → `NpgsqlException`/`DbException`, remover cast `(SqlConnection)`
- [ ] Trocar o provider em `DependencyInjectionConfiguration` e `Program.cs`
- [ ] Configurar CORS para o domínio/URL de produção (`ConfigureCors.cs` hoje só libera `localhost:4200`)
- [ ] Ajustar sintaxe das queries, se necessário (já há scripts Postgres em `scripts/postgres/`)

> Até concluir o checklist, use o **[deploy-local.md](deploy-local.md)** (SQL Server),
> que funciona com o código atual.

---

## Passo a passo

### 1. Criar a VM na Oracle Cloud

1. Acesse o console da OCI → **Compute → Instances → Create instance**
2. Dê um nome (ex.: `portal-financeiro`)
3. **Image**: Oracle Linux 8 (ou Ubuntu 22.04+)
4. **Shape** (o importante — **Always Free**):
   - Opção **ARM (recomendada):** `VM.Standard.A1.Flex` — até **4 OCPUs / 24 GB RAM**
     (escolha 4 OCPUs e 24 GB; cabe tudo com folga)
   - Opção x86: `VM.Standard.E2.1.Micro` — 1 OCPU / 1 GB (apertado)
5. **Networking**: marque **"Assign a public IPv4 address"**
6. **Add SSH keys**: gere/cole sua chave pública
7. **Create**

> Se aparecer "Out of capacity", o shape ARM está saturado — mude o *Availability Domain*
> ou tente o shape E2.1.Micro (1GB também roda, só mais apertado).

### 2. Liberar portas no firewall (Security List)

No console: **Virtual Cloud Networks → sua VCN → Subnet → Security List → Edit/Add rules**:

| Direction | Protocol | Source | Ports | Motivo |
|-----------|----------|--------|-------|--------|
| Ingress | TCP | `0.0.0.0/0` | `22` | SSH |
| Ingress | TCP | `0.0.0.0/0` | `80` | HTTP |
| Ingress | TCP | `0.0.0.0/0` | `443` | HTTPS |

**Não** abra `5432` (PostgreSQL) nem `8080` (API) — ficam só internos à rede Docker.

### 3. Conectar por SSH

```bash
ssh -i ~/.ssh/sua_chave.pem opc@<IP_PUBLICO>
```

(Oracle Linux usa usuário `opc`; Ubuntu usa `ubuntu`.)

### 4. Instalar Docker + Compose na VM

```bash
sudo dnf -y install dnf-utils
sudo dnf -y config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
sudo dnf -y install docker-ce docker-ce-cli containerd.io docker-compose-plugin
sudo systemctl enable --now docker
sudo usermod -aG docker opc
```

Saia e entre de novo (ou `newgrp docker`) para usar o Docker sem `sudo`.

### 5. Enviar o projeto para a VM

Do seu computador (na pasta do projeto):

```bash
# Opção A: clone do GitHub (recomendado)
git clone git@github.com:SEU_USUARIO/portal-financeiro.git
cd portal-financeiro

# Opção B: enviar via rsync
rsync -av --exclude node_modules --exclude dist --exclude bin --exclude obj ./ opc@<IP>:~/portal-financeiro/
```

### 6. Criar o `.env` na VM

```bash
cp .env.example .env
nano .env
```

Preencha com **senhas fortes**:

```dotenv
POSTGRES_USER=portal
POSTGRES_PASSWORD=UMA-SENHA-FORTE-AQUI
POSTGRES_DB=portal_financeiro
JWT_SECRET=UM-SEGREDO-LONGO-E-ALEATORIO
```

### 7. Subir a stack

```bash
docker compose up -d --build
```

O primeiro build demora (compila .NET + Angular). Depois:

```bash
docker compose ps          # tudo Up?
docker compose logs -f api # acompanhar a API
```

---

## Acesso sem domínio

Sem comprar domínio, há duas formas:

### Opção A — IP público (mais simples, HTTP)

`http://<IP_PUBLICO>` — a Oracle mostra o IP na página da instância.

- ✅ Grátis e imediato
- ⚠️ Sem HTTPS (tráfego sem criptografia)
- ⚠️ IP pode mudar se a instância for recriada

### Opção B — Cloudflare Tunnel (HTTPS grátis, sem comprar domínio)

O Cloudflare Tunnel cria um túnel HTTPS **sem expor IP e sem domínio próprio**:

1. Instale o `cloudflared` na VM (ou rode como container `cloudflare/cloudflared`)
2. `cloudflared tunnel --url http://localhost:80` → gera uma URL pública `https://xxxxx.trycloudflare.com`
3. Use essa URL para acessar o sistema com HTTPS

> Para HTTPS com **domínio próprio** (recomendado para produção real), o caminho é:
> comprar domínio (~R$ 40/ano) → apontar registro DNS para o IP → usar **Caddy** ou
> **Nginx + Let's Encrypt** (certificado gratuito). O Nginx do `web` já escuta em 443.

---

## Migrations (banco)

O portal usa **scripts "from scratch"** via DbUp (`tools/DbSetup`). Em produção
(PostgreSQL), o schema deve ser aplicado uma única vez:

**Opção A — script no compose:** monte `scripts/postgres` como volume no container
`api` e aplique na inicialização (recomendado para a doc futura de Postgres).

**Opção B — uma vez manualmente:**

```bash
# Executar do host (na VM) ou via container temporário com os scripts montados
psql "postgres://${POSTGRES_USER}:${POSTGRES_PASSWORD}@localhost:5432/${POSTGRES_DB}" \
  -f scripts/postgres/001_CriarTabelas.sql
psql "postgres://${POSTGRES_USER}:${POSTGRES_PASSWORD}@localhost:5432/${POSTGRES_DB}" \
  -f scripts/postgres/099_SeedBase.sql
```

> Em desenvolvimento (SQL Server), rode `dotnet run --project tools/DbSetup` — veja
> [banco.md](banco.md) e [deploy-local.md](deploy-local.md).

---

## Backup e restauração do banco

### Backup (no cron da VM)

```bash
# Criar script /home/opc/backup.sh
#!/bin/bash
docker compose exec -T db pg_dump -U portal portal_financeiro \
  | gzip > ~/backups/portal-$(date +%Y%m%d-%H%M).sql.gz
find ~/backups -name "*.sql.gz" -mtime +14 -delete   # mantém 14 dias
```

```bash
chmod +x /home/opc/backup.sh
mkdir -p ~/backups

# Agendar todo dia 02h00
crontab -e
# adicione:
# 0 2 * * * /home/opc/backup.sh
```

### Restauração

```bash
gunzip -c ~/backups/portal-YYYYMMDD-HHMM.sql.gz \
  | docker compose exec -T db psql -U portal portal_financeiro
```

---

## Atualização do sistema

```bash
cd ~/portal-financeiro
git pull                       # ou rsync do novo código
docker compose up -d --build   # reconstrói só o que mudou
docker compose ps              # verifica saúde
```

---

## Segurança (repo é público)

- `.env` **nunca** é commitado (está no `.gitignore`) — só o `.env.example` vai ao repositório, sem segredos reais
- O `appsettings.json` versionado tem apenas valores de desenvolvimento; em produção tudo vem de variáveis de ambiente (`ConnectionStrings__DefaultConnection`, `Auth__Secret`)
- Portas `5432`/`8080` **não** expostas ao mundo (só dentro da rede Docker)
- Troque a senha do admin em produção e gere um `JWT_SECRET` forte
- Mantenha a VM atualizada: `sudo dnf -y update`

---

## Troubleshooting

| Problema | Solução |
|----------|---------|
| Site não abre na porta 80 | Liberou a porta na Security List? Após liberar, espere 1–2 min |
| `Out of capacity` ao criar VM | Mude o Availability Domain ou use shape `VM.Standard.E2.1.Micro` |
| API dá erro de conexão ao banco | `docker compose logs api`; confira senha no `.env` |
| Docker sem permissão | `newgrp docker` ou use `sudo docker compose ...` |
| Quer HTTPS com domínio | Configure Caddy + Let's Encrypt (doc em aberto) |

---

## Ver também

- [deploy-local.md](deploy-local.md) — rodar tudo local com Docker (SQL Server)
- [primeiros-passos.md](primeiros-passos.md) — rodar sem Docker (LocalDB)
- [banco.md](banco.md) — scripts e schema por provider
- [README.md](README.md) — índice da documentação
