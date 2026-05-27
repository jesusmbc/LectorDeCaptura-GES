# LineaDeCaptura-GES

API .NET 8 para consulta de adeudos y aplicación de pagos en el flujo de Línea de Captura GES para Casa Ley.

## Stack
- .NET 8 (ASP.NET Core Web API)
- SQL Server
- Swagger / OpenAPI

## Estructura
- `LineaDeCaptura.GES.Api/`: API principal
- `scripts/`: scripts SQL de tablas, índices, constraints y procedimientos
- `origenGES/`: artefactos de referencia (Postman y documentación de apoyo)

## Requisitos
- .NET SDK 8.0+
- SQL Server con conectividad desde el host de ejecución

## Configuración local segura
Los archivos reales `appsettings.json` y `appsettings.Development.json` están excluidos de Git para evitar fuga de credenciales.

1. Copiar desde plantilla:
   - `LineaDeCaptura.GES.Api/appsettings.Template.json` -> `LineaDeCaptura.GES.Api/appsettings.json`
   - `LineaDeCaptura.GES.Api/appsettings.Development.Template.json` -> `LineaDeCaptura.GES.Api/appsettings.Development.json`
2. Configurar credenciales reales:
   - `ConnectionStrings:DefaultConnection`
   - `GesApi:ApiKey`

## Ejecución local
```bash
dotnet restore
dotnet build
dotnet run --project LineaDeCaptura.GES.Api
```

Swagger disponible en la URL que indique el arranque de ASP.NET Core.

## Base de datos
En `scripts/` se incluye la secuencia base:
1. `001_GES_CoreTables.sql`
2. `002_GES_Indexes.sql`
3. `003_GES_Constraints.sql`
4. `004_GES_StoredProcedures.sql`
5. `005_GES_SeedApiKey.sql`

## Git
Incluye `.gitignore` para:
- artefactos de compilación (`bin/`, `obj/`)
- metadatos de IDE (`.vs/`, `.idea/`, `.vscode/`)
- logs
- archivos de configuración sensible

## Seguridad
- No subir secretos al repositorio.
- Rotar cualquier credencial que haya estado en archivos locales no protegidos.
- Usar variables de entorno o secretos del entorno en despliegues productivos.
