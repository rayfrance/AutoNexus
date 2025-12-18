# 🏎️ AutoNexus - Gestão Inteligente de Concessionárias

O **AutoNexus** é uma plataforma robusta de gestão de vendas e estoque para concessionárias, integrada com Inteligência Artificial generativa para suporte à decisão estratégica.

## 🚀 Funcionalidades Principais

### 🤖 Consultor Estratégico IA (Gemini 2.0 Flash)
- **Análise em Tempo Real**: O sistema utiliza o modelo `gemini-2.0-flash` para analisar KPIs de faturamento, volume de vendas e giro de estoque.
- **Indicadores de Clima**: Insights visuais padronizados com emojis (✅ Boa Notícia / ⚠️ Atenção) para rápida interpretação do gestor.
- **Contexto de Marca**: Sugestões práticas baseadas nos fabricantes em estoque (BMW, Toyota, Ford, etc.).

### 📊 Dashboard & Relatórios
- **Vendas Recentes**: Listagem dinâmica das últimas transações com integração total ao banco de dados SQL Server.
- **Exportação de Dados**: Funcionalidade de exportação de relatórios de vendas para formato Excel (CSV) com interface clean.
- **Gráficos Dinâmicos**: Visualização de distribuição de estoque por fabricante via Chart.js.

## 🛠️ Stack Tecnológica
- **Backend**: .NET 9.0 (ASP.NET Core Razor Pages)
- **Frontend**: Bootstrap 5.3 + Bootstrap Icons
- **IA**: Google Gemini API (Model 2.0 Flash)
- **ORM**: Entity Framework Core
- **Segurança**: Variáveis de ambiente com `DotNetEnv` e proteção via `.gitignore`.

## ⚙️ Como Executar o Projeto

### 1. Preparação
* Certifique-se de ter o **SDK .NET 9.0** e o **SQL Server** instalados.
* Clone o repositório e execute `dotnet restore` no terminal.

### 2. Configuração de IA (Gemini)
1. Na raiz do projeto `AutoNexus.Web`, crie um arquivo chamado **`.env`**.
2. Adicione sua chave de API: `GEMINI_API_KEY=sua_chave_aqui`.
3. No Visual Studio, defina o arquivo `.env` para **"Copiar se for mais novo"** nas Propriedades.

### 3. Banco de Dados e Execução
```powershell
# Cria as tabelas e aplica o Seed de dados
dotnet ef database update

# Executa a aplicação
dotnet run --project AutoNexus.Web
