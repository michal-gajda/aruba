# Aruba

## Domain 

```powershell
dotnet new classlib --framework net10.0 --output src/Domain --name Aruba.Domain
```

## Application

```powershell
dotnet new classlib --framework net10.0 --output src/Application --name Aruba.Application
dotnet add src/Application reference src/Domain
dotnet add src/Application package MediatR
```

## Infrastructure

```powershell
dotnet new classlib --framework net10.0 --output src/Infrastructure --name Aruba.Infrastructure
dotnet add src/Infrastructure reference src/Application
dotnet add src/Infrastructure package Microsoft.Extensions.Configuration.Binder
```

## WebApi

```powershell
dotnet new webapi --framework net10.0 --no-https --use-controllers --use-program-main --output src/WebApi --name Aruba.WebApi
dotnet add src/WebApi reference src/Infrastructure
dotnet sln add src/WebApi
```

## Functional Tests

```powershell
dotnet new mstest --framework net10.0 --output tests/Application.FunctionalTests --name Aruba.Application.FunctionalTests
dotnet add tests/Application.FunctionalTests reference src/Infrastructure
dotnet add tests/Application.FunctionalTests package Microsoft.Extensions.DependencyInjection
dotnet add tests/Application.FunctionalTests package Microsoft.Extensions.Logging
dotnet add tests/Application.FunctionalTests package Microsoft.Extensions.Logging.Console
dotnet add tests/Application.FunctionalTests package Microsoft.Extensions.Logging.Debug
dotnet add tests/Application.FunctionalTests package Shouldly
```
