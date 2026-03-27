# AGENTS.md

## Visão Geral

Este repositório implementa um sistema de magias de D&D 5e com backend .NET,
persistência via EF Core, sincronização em background com Hangfire e frontend
Vue/Vite.

## Tecnologias Principais

| Categoria | Tecnologia | Evidência | Uso |
| --- | --- | --- | --- |
| Runtime | .NET 9 | `ApplicationCore/*.csproj`, `WebAPIHost/*.csproj` | Backend e jobs |
| Persistência | EF Core + SQL Server | `Infrastructure/*.csproj`, `WebAPIHost/Program.cs` | Banco relacional |
| Jobs | Hangfire + Hangfire.SqlServer | `WebAPIHost/Program.cs`, `WorkerServiceHost/Program.cs` | Processamento em background |
| API | ASP.NET Core | `WebAPIHost/Program.cs` | HTTP, CORS e Swagger |
| Frontend | Vue 3 + Vite + TypeScript | `FrontEnd/package.json` | UI |
| Estado/HTTP | Pinia, Vue Router, Axios | `FrontEnd/package.json` | Store, navegação e consumo da API |

## Estrutura do Repositório

- `DnDSpellsSolution.sln`: agrupa os projetos .NET.
- `ApplicationCore/`: entidades, DTOs e contratos; não deve depender de infraestrutura.
- `Infrastructure/`: `ApplicationDbContext`, migrations e repositórios.
- `WebAPIHost/`: API HTTP, Swagger, CORS e dashboard do Hangfire.
- `HangfireJobs/`: jobs de sincronização, incluindo `SpellUpsertJob`.
- `WorkerServiceHost/`: host dedicado ao processamento em background.
- `FrontEnd/`: SPA Vue que consome a API.

## Setup e Ambiente

- `DefaultConnection` existe em `WebAPIHost/appsettings*.json` e `WorkerServiceHost/appsettings*.json`.
- `WebAPIHost` e `WorkerServiceHost` compartilham o mesmo storage do Hangfire.
- Integração externa principal: `https://www.dnd5eapi.co`.
- Se alterar o banco, revise API, jobs e frontend em conjunto.

## Comandos de Desenvolvimento

```bash
dotnet restore DnDSpellsSolution.sln
dotnet build DnDSpellsSolution.sln
dotnet run --project .\WebAPIHost\WebAPIHost.csproj
dotnet run --project .\WorkerServiceHost\WorkerServiceHost.csproj
dotnet ef migrations add <Nome> --project Infrastructure --startup-project WebAPIHost
dotnet ef database update --project Infrastructure --startup-project WebAPIHost

cd .\FrontEnd
npm install
npm run dev
npm run build
```

## Convenções e Limites

- Preserve a fronteira `ApplicationCore -> Infrastructure -> hosts`.
- Toda lógica de acesso a dados deve continuar em `Infrastructure/Repositories/`.
- Mudanças no refresh de dados devem considerar `HangfireJobs` e `WorkerServiceHost`.
- Se alterar DTOs ou entidades, revise controller, repositório, job e frontend juntos.
- CORS, Swagger e Hangfire devem continuar concentrados em `WebAPIHost`.

## Peculiaridades do Projeto

- O repositório mistura dois fluxos de desenvolvimento independentes: solução .NET e app Vite.
- O job de sincronização grava no mesmo banco usado pela API e pelo dashboard do Hangfire.
- O histórico de Git mistura commits diretos e merges de `develop`; não há padrão rígido de mensagem.
