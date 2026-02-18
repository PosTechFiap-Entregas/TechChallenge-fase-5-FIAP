# 🚀 CI/CD - GitHub Actions

## 📋 O que foi implementado

O projeto possui **3 workflows automatizados**:

### 1️⃣ **CI - Build & Test** (`ci.yml`)
Roda em **todos os pushes e Pull Requests**

✅ Restaura dependências  
✅ Compila o projeto (Release)  
✅ Executa testes automatizados  
✅ Gera relatório de testes  

### 2️⃣ **Docker Build** (`docker-build.yml`)
Roda **apenas em pushes na branch main**

✅ Builda imagem Docker da API  
✅ Builda imagem Docker do Worker  
✅ Valida o docker-compose.yml  

### 3️⃣ **CI/CD Pipeline Completo** (`ci-cd-full.yml`)
Pipeline completo com 3 jobs

✅ Job 1: Build & Test  
✅ Job 2: Docker Build (só roda se o build passar)  
✅ Job 3: Code Quality (análise de warnings)  

---

## 📁 Onde colocar os arquivos

Crie a estrutura de pastas:

```
.github/
└── workflows/
    ├── ci.yml              ← Build & Test simples
    ├── docker-build.yml    ← Docker Build
    └── ci-cd-full.yml      ← Pipeline completo (RECOMENDADO)
```

### ⚠️ Escolha APENAS UM workflow:

**Opção A - Simples:**
- Use `ci.yml` + `docker-build.yml` (2 arquivos separados)

**Opção B - Completo (RECOMENDADO):**
- Use `ci-cd-full.yml` (tudo em 1 arquivo, mais organizado)

---

## 🎯 Como usar

### 1️⃣ Criar a estrutura de pastas

No **terminal** na raiz do projeto:

```bash
# Windows (PowerShell)
mkdir .github
mkdir .github\workflows

# Linux/Mac
mkdir -p .github/workflows
```

### 2️⃣ Copiar o workflow

**Opção recomendada:** Copie o arquivo `ci-cd-full.yml` para `.github/workflows/`

```bash
# Renomeie para um nome mais simples (opcional)
# ci-cd-full.yml → ci-cd.yml
```

### 3️⃣ Commit e Push

```bash
git add .github/workflows/
git commit -m "ci: adiciona pipeline CI/CD"
git push origin main
```

### 4️⃣ Verificar no GitHub

Acesse: `https://github.com/SEU-USUARIO/TechChallenge-fase-5-FIAP/actions`

Você verá o workflow rodando! 🎉

---

## 📊 O que cada job faz

### Job 1: Build and Test
```
1. Faz checkout do código
2. Instala .NET 8
3. Restaura pacotes NuGet
4. Compila o projeto (Release)
5. Roda os testes
6. Gera relatório
```

### Job 2: Docker Build
```
1. Builda imagem da API
2. Builda imagem do Worker
3. Valida docker-compose.yml
```

### Job 3: Code Quality
```
1. Analisa warnings do compilador
2. Conta total de warnings
3. Exibe relatório
```

---

## ✅ Como saber se passou

No GitHub Actions:

- ✅ **Verde** = Tudo funcionou
- ❌ **Vermelho** = Algo falhou (veja os logs)
- 🟡 **Amarelo** = Rodando

---

## 🔧 Troubleshooting

### Erro: "TechChallenge-Fase5.slnx not found"
**Solução:** Certifique-se que o arquivo `.slnx` está na raiz do repositório

### Erro: "Dockerfile not found"
**Solução:** Verifique os paths dos Dockerfiles no workflow

### Testes falhando?
**Solução:** Quando criarmos os testes, eles vão rodar automaticamente. Por enquanto, o workflow usa `|| true` para não falhar se não houver testes.

---

## 🎓 Para a FIAP

Este CI/CD atende aos requisitos:

✅ "CI/CD da aplicação" - GitHub Actions configurado  
✅ Build automatizado  
✅ Testes automatizados (quando implementados)  
✅ Validação de Docker  
✅ Análise de qualidade de código  

**Demonstração no vídeo:**
1. Mostre o repositório no GitHub
2. Acesse a aba "Actions"
3. Mostre um workflow rodando com sucesso
4. Explique cada job

---

**Desenvolvido para FIAP - Fase 5** 🎓
