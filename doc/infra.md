# Infraestrutura â€” Portal Financeiro

> ProduÃ§Ã£o atual: **Neon** (PostgreSQL) + **Render** (API .NET) + **Vercel** (Angular).
> Publicado em `https://portal-financeiro-alpha.vercel.app/`. Veja os 3 ambientes abaixo.

## Ambientes publicados â€” Vercel + Render + Neon

Stack real em produÃ§Ã£o: **Neon** (banco) + **Render** (backend) + **Vercel** (frontend).

### Ambientes

| Ambiente | Front (Vercel) | Back (Render) | Banco (Neon) | Branch git | Uso |
|----------|----------------|---------------|--------------|------------|-----|
| **alpha** | portal-financeiro-alpha.vercel.app | portal-financeiro-p57h.onrender.com | **banco prod** | `main` | pessoal (thayllon e alana) |
| **bravo** | portal-financeiro-bravo.vercel.app | portal-financeiro-bravo.onrender.com | **banco separado** (isolado) | `main` | portfÃ³lio (joÃ£o e maria) |
| **charlie** | portal-financeiro-charlie.vercel.app | portal-financeiro-e0xw.onrender.com | **banco prod** | `develop` | dev |

> **Banco:** alpha e charlie usam o **banco prod**; **bravo usa um projeto Neon separado**
> (isolado, mais seguro para o portfÃ³lio). Frontend: **cada ambiente tem o prÃ³prio build**
> (a URL do backend Ã© embutida por arquivo de ambiente):
> - alpha â†’ `environment.prod.ts` (`npm run build`)
> - bravo â†’ `environment.bravo.ts` (`npm run build:bravo`)
> - charlie â†’ `environment.charlie.ts` (`npm run build:charlie`)

### Custo real (planos free, 2026)

| Plataforma | Plano | Limites | Notas |
|------------|-------|---------|-------|
| **Vercel** | Hobby ($0) | 200 projetos, 25 projetos/repo, 100 deploys/dia | Uso pessoal/nÃ£o-comercial; domÃ­nio `.vercel.app` derivado do nome do projeto (escolhÃ­vel, Ãºnico global) |
| **Render** | Hobby ($0) | atÃ© 25 serviÃ§os; **750h/mÃªs por workspace** | Free dorme apÃ³s 15min de inatividade; horas compartilhadas entre **todos** os serviÃ§os free do workspace |
| **Neon** | Free ($0) | 100 projetos, 10 branches/projeto, 0,5 GB/projeto, 100 CU-h/mÃªs | Escala a zero apÃ³s 5min; storage/compute sÃ£o por projeto |

âš ï¸ **Render: as 750h/mÃªs sÃ£o compartilhadas por workspace** â€” os 3 backends free gastam do
mesmo balde de horas. **NÃ£o** manter keep-alive pingando os 3 serviÃ§os (devora o balde e pode
suspender todos). Uso esparso (demo/portfÃ³lio) fica perto de R$ 0; cold start apÃ³s dormir ~30â€“60s.

### Passo a passo â€” criar cada ambiente

PrÃ©-requisito comum: o repositÃ³rio publicado no GitHub e as contas em **Neon**, **Render** e
**Vercel** jÃ¡ criadas.

#### Ambiente alpha (pessoal â€” thayllon e alana)

1. **Banco (Neon):** usar o **projeto prod jÃ¡ existente** â€” nada a criar.
2. **Backend (Render):**
   - Dashboard â†’ **New â†’ Web Service** (ou **Blueprint** com `render.yaml`) â†’ conectar o repo.
   - Branch: **`main`** Â· Runtime: **Docker** (`Dockerfile`).
   - Env vars: `ConnectionStrings__DefaultConnection` = conn prod, `Auth__Secret` = segredo 1,
     `Cors__AllowedOrigins` = `https://portal-financeiro-alpha.vercel.app`,
     `Database__Provider` = `Postgres`, `ASPNETCORE_ENVIRONMENT` = `Production`.
   - Anotar a URL (ex.: `https://portal-financeiro-p57h.onrender.com`).
3. **Frontend (Vercel):**
   - **Add New â†’ Project** â†’ importar o repo â†’ nome **`portal-financeiro-alpha`**.
   - Root Directory: **`src/PortalFinanceiro.Web`** Â· Production Branch: **`main`**.
   - Build Command: **`npm run build`** (usa `environment.prod.ts`).
   - Conferir se `environment.prod.ts` aponta para a URL do backend alpha.
4. Testar login e o fluxo.

#### Ambiente bravo (portfÃ³lio)

> Banco **separado** (isolado) â€” criar um projeto Neon prÃ³prio.

1. **Banco (Neon):** criar um **novo projeto Neon** (limite free: 100) â†’ anotar a connection
   string. Aplicar schema nele: `001_CriarTabelas` e `100_DDL_AtualizarEstrutura` (usuÃ¡rios reais
   criados via app).
2. **Backend (Render):** novo Web Service â†’ branch **`main`** â†’ Docker.
   - Env vars: `ConnectionStrings__DefaultConnection` = conn do banco bravo,
     `Auth__Secret` = segredo 2, `Cors__AllowedOrigins` = `https://portal-financeiro-bravo.vercel.app`.
3. **Frontend (Vercel):** novo projeto **`portal-financeiro-bravo`** â†’ Root Directory
   `src/PortalFinanceiro.Web` â†’ Production Branch `main` â†’ Build Command
   **`npm run build:bravo`** (usa `environment.bravo.ts` â€” ajuste o `apiUrl` para o backend bravo).
4. Testar com as credenciais do portfÃ³lio (maria/joÃ£o).

#### Ambiente charlie (dev â€” espelha develop)

1. **Banco (Neon):** usar o **mesmo banco prod** que o alpha (ou criar um **branch** Neon
   p/ nÃ£o poluir os dados pessoais).
2. **Backend (Render):** novo Web Service â†’ branch **`develop`** â†’ Docker.
   - `ConnectionStrings__DefaultConnection` = conn prod; `Auth__Secret` = segredo 3;
     `Cors__AllowedOrigins` = `https://portal-financeiro-charlie.vercel.app`.
3. **Frontend (Vercel):** novo projeto **`portal-financeiro-charlie`** â†’ Root Directory
   `src/PortalFinanceiro.Web` â†’ Production Branch **`develop`** â†’ Build Command
   **`npm run build:charlie`** (usa `environment.charlie.ts` â€” ajuste o `apiUrl` para o backend charlie).

### VariÃ¡veis por ambiente (Render)

| Chave | alpha | bravo | charlie |
|-------|-------|-------|---------|
| `Database__Provider` | `Postgres` | `Postgres` | `Postgres` |
| `ConnectionStrings__DefaultConnection` | conn prod | **conn bravo (separada)** | conn prod |
| `Auth__Secret` | segredo 1 | segredo 2 | segredo 3 |
| `Cors__AllowedOrigins` | `https://portal-financeiro-alpha.vercel.app` | `https://portal-financeiro-bravo.vercel.app` | `https://portal-financeiro-charlie.vercel.app` |

> Frontend â€” `apiUrl` compilado no build (um arquivo por ambiente):
> - alpha â†’ `src/environments/environment.prod.ts`
> - bravo â†’ `src/environments/environment.bravo.ts`
> - charlie â†’ `src/environments/environment.charlie.ts`
> Atualize o arquivo do ambiente com a URL do backend antes de publicar cada projeto.

### ConfiguraÃ§Ã£o Vercel (por projeto â€” resumo final)

| Projeto | Root Directory | Build Command | Output Directory | Production Branch |
|---------|----------------|---------------|------------------|-------------------|
| **portal-financeiro-alpha** | `src/PortalFinanceiro.Web` | `npm run build` | `dist/portal-financeiro/browser` | `main` |
| **portal-financeiro-bravo** | `src/PortalFinanceiro.Web` | `npm run build:bravo` | `dist/portal-financeiro/browser` | `main` |
| **portal-financeiro-charlie** | `src/PortalFinanceiro.Web` | `npm run build:charlie` | `dist/portal-financeiro/browser` | `develop` |

> O `vercel.json` da **raiz** foi removido â€” a config vÃ¡lida Ã© a de `src/PortalFinanceiro.Web/vercel.json`
> (output `dist/portal-financeiro/browser` + rewrite de SPA). Manter o Root Directory = `src/PortalFinanceiro.Web`
> em todos os projetos; usar Root = repo raiz quebra o output/rewrite.

### AtualizaÃ§Ã£o (deploy por branch)

- Push em `main` â†’ alpha e bravo (front + back) fazem redeploy automÃ¡tico.
- Push em `develop` â†’ charlie faz redeploy automÃ¡tico.
- **SÃ³ alteraÃ§Ã£o nas branches REMOTAS `main`/`develop` dispara deploy** (push direto ou merge
  de PR) â€” mudanÃ§a em branch local nÃ£o dispara nada; commit local sÃ³ tem efeito apÃ³s o push
  para o GitHub.
- MigraÃ§Ã£o/seed em banco existente Ã© **manual** (psql / SQL editor do Neon).

### SeguranÃ§a

- Segredos (connection string, JWT) sÃ³ em env vars das plataformas â€” nunca no repo.
- Banco **bravo isolado** garante que o portfÃ³lio nÃ£o vaza dados pessoais (alpha/charlie
  compartilham o banco prod; charlie pode usar um branch Neon se quiser isolar o dev).
- CORS por ambiente restringe qual front pode chamar a API.
- Vercel Hobby Ã© **uso pessoal/nÃ£o-comercial** â€” portfÃ³lio para clientes fica no limite da regra.

---

## Alternativa self-hosted na Oracle Cloud (legado)

> Deploy completo **na Oracle Cloud** (Always Free â€” **R$ 0/mÃªs**) com
> Frontend + Backend + PostgreSQL **no mesmo servidor**, via Docker Compose.

## VisÃ£o geral

Tudo roda em **uma Ãºnica VM** (1 servidor) com 3 containers:

```
                        â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
  usuÃ¡rio  â”€â”€HTTPSâ”€â”€â–¶  â”‚  Oracle Cloud VM (Linux)     â”‚
                        â”‚                             â”‚
                        â”‚  web (Nginx)  portas 80/443 â”‚
                        â”‚   â”œâ”€â”€ /      â†’ Angular (SPA)â”‚
                        â”‚   â””â”€â”€ /api/  â†’ proxy        â”‚
                        â”‚                 â”‚           â”‚
                        â”‚  api (.NET)   porta 8080    â”‚
                        â”‚   â””â”€â”€ conectar             â”‚
                        â”‚                 â–¼           â”‚
                        â”‚  db (PostgreSQL)  porta 5432â”‚
                        â”‚                             â”‚
                        â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

| Container | Imagem | FunÃ§Ã£o |
|-----------|--------|--------|
| `web` | `nginx:alpine` | Serve o build do Angular + proxy `/api` |
| `api` | .NET 11 (Dockerfile) | API ASP.NET Core, JWT, Dapper |
| `db` | `postgres:17-alpine` | Banco de dados |

> A imagem `web` Ã© compilada com `npm run build:docker` (configuraÃ§Ã£o `docker`,
> `apiUrl: '/api'` via `environment.docker.ts`) â€” o front fala com a API da mesma VM.

### Custo

| Item | Custo |
|------|-------|
| VM Oracle Cloud (Always Free) | **R$ 0** |
| PostgreSQL / Docker / Nginx | R$ 0 (open source) |
| DomÃ­nio | R$ 0 (usamos URL/IP da Oracle â€” ver [Acesso](#acesso-sem-domÃ­nio)) |
| **Total** | **R$ 0/mÃªs** |

## PrÃ©-requisitos

1. Conta na **Oracle Cloud Infrastructure** (OCI) â€” o Always Free exige cartÃ£o de crÃ©dito para validaÃ§Ã£o, **nÃ£o cobra nada**
2. **Docker** instalado na sua mÃ¡quina (para o passo de build local, opcional)
3. Testar local primeiro: **[deploy-local.md](deploy-local.md)**

## âœ… Suporte a PostgreSQL (jÃ¡ implementado)

O backend jÃ¡ fala com PostgreSQL: pacote `Npgsql` em `PortalFinanceiro.Infrastructure`,
`PostgresDialect`, `PostgresConnectionFactory` e seleÃ§Ã£o de provider em
`DependencyInjectionConfiguration` via `Database__Provider: "Postgres"` (ver
`docker-compose.yml:23-25`). CORS jÃ¡ aceita as origens configuradas em
`Cors__AllowedOrigins` (+ `*.vercel.app`).

---

## Passo a passo

### 1. Criar a VM na Oracle Cloud

1. Acesse o console da OCI â†’ **Compute â†’ Instances â†’ Create instance**
2. DÃª um nome (ex.: `portal-financeiro`)
3. **Image**: Oracle Linux 8 (ou Ubuntu 22.04+)
4. **Shape** (o importante â€” **Always Free**):
   - OpÃ§Ã£o **ARM (recomendada):** `VM.Standard.A1.Flex` â€” atÃ© **4 OCPUs / 24 GB RAM**
     (escolha 4 OCPUs e 24 GB; cabe tudo com folga)
   - OpÃ§Ã£o x86: `VM.Standard.E2.1.Micro` â€” 1 OCPU / 1 GB (apertado)
5. **Networking**: marque **"Assign a public IPv4 address"**
6. **Add SSH keys**: gere/cole sua chave pÃºblica
7. **Create**

> Se aparecer "Out of capacity", o shape ARM estÃ¡ saturado â€” mude o *Availability Domain*
> ou tente o shape E2.1.Micro (1GB tambÃ©m roda, sÃ³ mais apertado).

### 2. Liberar portas no firewall (Security List)

No console: **Virtual Cloud Networks â†’ sua VCN â†’ Subnet â†’ Security List â†’ Edit/Add rules**:

| Direction | Protocol | Source | Ports | Motivo |
|-----------|----------|--------|-------|--------|
| Ingress | TCP | `0.0.0.0/0` | `22` | SSH |
| Ingress | TCP | `0.0.0.0/0` | `80` | HTTP |
| Ingress | TCP | `0.0.0.0/0` | `443` | HTTPS |

**NÃ£o** abra `5432` (PostgreSQL) nem `8080` (API) â€” ficam sÃ³ internos Ã  rede Docker.

### 3. Conectar por SSH

```bash
ssh -i ~/.ssh/sua_chave.pem opc@<IP_PUBLICO>
```

(Oracle Linux usa usuÃ¡rio `opc`; Ubuntu usa `ubuntu`.)

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
# OpÃ§Ã£o A: clone do GitHub (recomendado)
git clone git@github.com:SEU_USUARIO/portal-financeiro.git
cd portal-financeiro

# OpÃ§Ã£o B: enviar via rsync
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

## Acesso sem domÃ­nio

Sem comprar domÃ­nio, hÃ¡ duas formas:

### OpÃ§Ã£o A â€” IP pÃºblico (mais simples, HTTP)

`http://<IP_PUBLICO>` â€” a Oracle mostra o IP na pÃ¡gina da instÃ¢ncia.

- âœ… GrÃ¡tis e imediato
- âš ï¸ Sem HTTPS (trÃ¡fego sem criptografia)
- âš ï¸ IP pode mudar se a instÃ¢ncia for recriada

### OpÃ§Ã£o B â€” Cloudflare Tunnel (HTTPS grÃ¡tis, sem comprar domÃ­nio)

O Cloudflare Tunnel cria um tÃºnel HTTPS **sem expor IP e sem domÃ­nio prÃ³prio**:

1. Instale o `cloudflared` na VM (ou rode como container `cloudflare/cloudflared`)
2. `cloudflared tunnel --url http://localhost:80` â†’ gera uma URL pÃºblica `https://xxxxx.trycloudflare.com`
3. Use essa URL para acessar o sistema com HTTPS

> Para HTTPS com **domÃ­nio prÃ³prio** (recomendado para produÃ§Ã£o real), o caminho Ã©:
> comprar domÃ­nio (~R$ 40/ano) â†’ apontar registro DNS para o IP â†’ usar **Caddy** ou
> **Nginx + Let's Encrypt** (certificado gratuito). O Nginx do `web` jÃ¡ escuta em 443.

---

## Migrations (banco)

O portal usa **scripts SQL em `scripts/postgres/`** aplicados manualmente (sem DbUp).
Em produção (PostgreSQL), o schema deve ser aplicado uma única vez:

**Opção A — script no compose:** monte `scripts/postgres` como volume no container
`api` e aplique na inicialização.

**Opção B — uma vez manualmente:**

```bash
# Executar do host (na VM) ou via container temporário com os scripts montados
psql "postgres://${POSTGRES_USER}:${POSTGRES_PASSWORD}@localhost:5432/${POSTGRES_DB}" \
  -f scripts/postgres/001_CriarTabelas.sql
# ... aplicar os demais na ordem (003,004,005,006,100,103,104,105,106)
```

> Em desenvolvimento (PostgreSQL local), aplique os scripts na réplica local ou use
> `sincronizar-banco.ps1` — veja [banco.md](banco.md) e [primeiros-passos.md](primeiros-passos.md).

---

## Backup e restauraÃ§Ã£o do banco

### Backup (no cron da VM)

```bash
# Criar script /home/opc/backup.sh
#!/bin/bash
docker compose exec -T db pg_dump -U portal portal_financeiro \
  | gzip > ~/backups/portal-$(date +%Y%m%d-%H%M).sql.gz
find ~/backups -name "*.sql.gz" -mtime +14 -delete   # mantÃ©m 14 dias
```

```bash
chmod +x /home/opc/backup.sh
mkdir -p ~/backups

# Agendar todo dia 02h00
crontab -e
# adicione:
# 0 2 * * * /home/opc/backup.sh
```

### RestauraÃ§Ã£o

```bash
gunzip -c ~/backups/portal-YYYYMMDD-HHMM.sql.gz \
  | docker compose exec -T db psql -U portal portal_financeiro
```

---

## AtualizaÃ§Ã£o do sistema

```bash
cd ~/portal-financeiro
git pull                       # ou rsync do novo cÃ³digo
docker compose up -d --build   # reconstrÃ³i sÃ³ o que mudou
docker compose ps              # verifica saÃºde
```

---

## SeguranÃ§a (repo Ã© pÃºblico)

- `.env` **nunca** Ã© commitado (estÃ¡ no `.gitignore`) â€” sÃ³ o `.env.example` vai ao repositÃ³rio, sem segredos reais
- O `appsettings.json` versionado tem apenas valores de desenvolvimento; em produÃ§Ã£o tudo vem de variÃ¡veis de ambiente (`ConnectionStrings__DefaultConnection`, `Auth__Secret`)
- Portas `5432`/`8080` **nÃ£o** expostas ao mundo (sÃ³ dentro da rede Docker)
- Troque a senha do admin em produÃ§Ã£o e gere um `JWT_SECRET` forte
- Mantenha a VM atualizada: `sudo dnf -y update`

---

## Troubleshooting

| Problema | SoluÃ§Ã£o |
|----------|---------|
| Site nÃ£o abre na porta 80 | Liberou a porta na Security List? ApÃ³s liberar, espere 1â€“2 min |
| `Out of capacity` ao criar VM | Mude o Availability Domain ou use shape `VM.Standard.E2.1.Micro` |
| API dÃ¡ erro de conexÃ£o ao banco | `docker compose logs api`; confira senha no `.env` |
| Docker sem permissÃ£o | `newgrp docker` ou use `sudo docker compose ...` |
| Quer HTTPS com domÃ­nio | Configure Caddy + Let's Encrypt (doc em aberto) |

---

## Ver tambÃ©m

- [deploy-local.md](deploy-local.md) — rodar tudo local com Docker (PostgreSQL)
- [primeiros-passos.md](primeiros-passos.md) — rodar sem Docker (PostgreSQL local)
- [banco.md](banco.md) — scripts e schema do PostgreSQL
- [README.md](README.md) â€” Ã­ndice da documentaÃ§Ã£o
