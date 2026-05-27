# Scripts SQL - Proyecto LineaDeCaptura GES

## Objetivo
Este paquete crea los objetos SQL del proyecto **sin crear base de datos nueva**, usando prefijo obligatorio `GES_` y sintaxis compatible con SQL Server 2008 a 2022.

## Orden de ejecucion
1. `scripts/001_GES_CoreTables.sql`
2. `scripts/002_GES_Indexes.sql`
3. `scripts/003_GES_Constraints.sql`
4. `scripts/004_GES_StoredProcedures.sql`
5. `scripts/005_GES_SeedApiKey.sql`

## Objetos creados

### 1) Tabla `dbo.GES_Transactions`
Registro maestro por `transactionGuid` para estado actual de cada operacion.

Campos:
- `GES_TransactionId` (BIGINT IDENTITY, PK): Identificador interno.
- `TransactionGuid` (UNIQUEIDENTIFIER, UNIQUE): Correlacion tecnica de la transaccion.
- `CaptureLine` (VARCHAR(50), NOT NULL): Linea de captura original enviada por POS.
- `GesTraceId` (VARCHAR(60), NULL): Correlacion generada por GES.
- `Status` (VARCHAR(30), NOT NULL): Estado actual (`INQUIRED`, `APPLIED`, `REJECTED`, `ALREADY_PAID`, `ERROR`).
- `StatusCode` (VARCHAR(20), NULL): Codigo de negocio devuelto.
- `StatusMessage` (VARCHAR(400), NULL): Mensaje de negocio devuelto.
- `Channel` (VARCHAR(20), NOT NULL): Canal de origen, por ejemplo `POS`.
- `StoreId` (VARCHAR(20), NULL): Sucursal.
- `CashRegister` (VARCHAR(20), NULL): Caja.
- `CashierId` (VARCHAR(20), NULL): Cajero.
- `RequestedAmount` (DECIMAL(18,2), NULL): Importe cobrado/intentado.
- `CurrencyCode` (VARCHAR(10), NULL): Moneda (`MXN`).
- `PaymentMethod` (VARCHAR(20), NULL): Metodo de pago.
- `HouseTicketNumber` (VARCHAR(50), NULL): Folio de ticket Casa Ley.
- `HouseTicketTimestamp` (DATETIME, NULL): Fecha/hora del ticket.
- `AuthorizationCode` (VARCHAR(80), NULL): Autorizacion de GES.
- `OfficialReceiptFolio` (VARCHAR(80), NULL): Folio oficial de recibo GES.
- `LastMessageType` (VARCHAR(40), NULL): Ultimo tipo de mensaje procesado.
- `IsActive` (BIT, default 1): Bandera operativa.
- `CreatedAt` (DATETIME, default GETDATE): Alta.
- `UpdatedAt` (DATETIME, default GETDATE): Ultima actualizacion.

### 2) Tabla `dbo.GES_EventStore`
Bitacora de eventos por fase (entrada/salida), con campos rapidos + JSON completo de respaldo.

Campos:
- `GES_EventId` (BIGINT IDENTITY, PK): Identificador del evento.
- `TransactionGuid` (UNIQUEIDENTIFIER, NOT NULL): Correlacion principal.
- `MessageType` (VARCHAR(40), NOT NULL): Tipo (`DebtInquiryRequest`, `PaymentApplyResponse`, etc.).
- `EventPhase` (VARCHAR(20), NOT NULL): Fase (`INQUIRY`, `APPLY`).
- `Direction` (VARCHAR(10), NOT NULL): `IN` o `OUT`.
- `EventStatus` (VARCHAR(30), NULL): Estado funcional.
- `EventStatusCode` (VARCHAR(20), NULL): Codigo funcional.
- `CaptureLine` (VARCHAR(50), NULL): Linea de captura.
- `GesTraceId` (VARCHAR(60), NULL): Trazabilidad GES.
- `AuthorizationCode` (VARCHAR(80), NULL): Autorizacion.
- `HouseTicketNumber` (VARCHAR(50), NULL): Ticket Casa Ley.
- `PayloadJson` (NVARCHAR(MAX), NOT NULL): JSON crudo completo de request/response.
- `CreatedAt` (DATETIME, default GETDATE): Fecha del evento.

### 3) Tabla `dbo.GES_IdempotencyKeys`
Control de no-duplicidad por `transactionGuid + messageType`.

Campos:
- `GES_IdempotencyKeyId` (BIGINT IDENTITY, PK): Identificador interno.
- `TransactionGuid` (UNIQUEIDENTIFIER, NOT NULL): Correlacion.
- `MessageType` (VARCHAR(40), NOT NULL): Tipo de mensaje deduplicado.
- `RequestHash` (VARCHAR(128), NULL): Hash opcional del request.
- `ResponseStatusCode` (VARCHAR(20), NULL): Codigo de la respuesta ya emitida.
- `ResponseBodyJson` (NVARCHAR(MAX), NULL): Respuesta cacheada para reintentos.
- `IsSuccess` (BIT, default 0): Resultado exitoso/fallido.
- `CreatedAt` (DATETIME, default GETDATE): Fecha de creacion.

Constraint clave:
- `UQ_GES_IdempotencyKeys_Key` unico sobre (`TransactionGuid`, `MessageType`).

### 4) Tabla `dbo.GES_OperationLogs`
Log tecnico/operativo para rastreo de fallas y auditoria.

Campos:
- `GES_OperationLogId` (BIGINT IDENTITY, PK): Identificador interno.
- `TransactionGuid` (UNIQUEIDENTIFIER, NULL): Correlacion de negocio.
- `GesTraceId` (VARCHAR(60), NULL): Correlacion GES.
- `Severity` (VARCHAR(10), NOT NULL): `INFO`, `WARN`, `ERROR`.
- `Source` (VARCHAR(100), NOT NULL): Componente que registra.
- `EventCode` (VARCHAR(50), NULL): Codigo tecnico interno.
- `LogMessage` (VARCHAR(1000), NOT NULL): Mensaje principal.
- `ExceptionDetail` (NVARCHAR(MAX), NULL): Stack/message de excepcion.
- `RequestPath` (VARCHAR(200), NULL): Ruta invocada.
- `HttpMethod` (VARCHAR(10), NULL): Metodo HTTP.
- `RemoteIp` (VARCHAR(45), NULL): IP origen.
- `CreatedAt` (DATETIME, default GETDATE): Fecha de log.

### 5) Tabla `dbo.GES_ApiKeys`
Repositorio de API key activa (permite rotacion controlada sin redeploy).

Campos:
- `GES_ApiKeyId` (INT IDENTITY, PK): Identificador interno.
- `ApiKeyValue` (VARCHAR(128), UNIQUE): Valor de la apikey.
- `IsEnabled` (BIT, default 1): Habilitada/deshabilitada.
- `ValidFrom` (DATETIME, default GETDATE): Inicio de vigencia.
- `ValidTo` (DATETIME, NULL): Fin de vigencia (NULL = sin caducidad).
- `CreatedAt` (DATETIME, default GETDATE): Alta.

## Indices creados
- `IX_GES_Transactions_CaptureLine`
- `IX_GES_Transactions_GesTraceId`
- `IX_GES_Transactions_UpdatedAt`
- `IX_GES_EventStore_TransactionGuid`
- `IX_GES_EventStore_CaptureLine`
- `IX_GES_OperationLogs_TransactionGuid`
- `IX_GES_OperationLogs_CreatedAt`
- `IX_GES_IdempotencyKeys_CreatedAt`

## Constraints creados
- `CK_GES_Transactions_Status`
- `CK_GES_OperationLogs_Severity`
- `CK_GES_EventStore_Direction`
- `CK_GES_ApiKeys_Validity`

## Stored Procedures creados

### `dbo.GES_spValidateApiKey`
Valida que la apikey exista, este habilitada y en vigencia.

Parametros:
- `@ApiKeyValue VARCHAR(128)`

### `dbo.GES_spInsertOperationLog`
Inserta log tecnico de forma estandarizada.

Parametros:
- `@TransactionGuid UNIQUEIDENTIFIER = NULL`
- `@GesTraceId VARCHAR(60) = NULL`
- `@Severity VARCHAR(10)`
- `@Source VARCHAR(100)`
- `@EventCode VARCHAR(50) = NULL`
- `@LogMessage VARCHAR(1000)`
- `@ExceptionDetail NVARCHAR(MAX) = NULL`
- `@RequestPath VARCHAR(200) = NULL`
- `@HttpMethod VARCHAR(10) = NULL`
- `@RemoteIp VARCHAR(45) = NULL`

### `dbo.GES_spPurgeRetention`
Purgado por retencion en meses para bitacoras/eventos/idempotencia y transacciones cerradas.

Parametro:
- `@RetentionMonths INT = 12`

## Carga inicial
`005_GES_SeedApiKey.sql` inserta o reactiva la API key actual:
- `0NyZT3bdNqPwRd5ded4BNc3soC6GLz7QnZDepKXiY`

## Recomendacion operativa
Programar `GES_spPurgeRetention @RetentionMonths = 12` en SQL Agent con corrida mensual.
