# Infraestrutura — Portal Financeiro

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
