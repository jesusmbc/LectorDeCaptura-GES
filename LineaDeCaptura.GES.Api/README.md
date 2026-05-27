# LineaDeCaptura.GES.Api

API puente POS -> GES en C# con persistencia MSSQL sobre objetos `GES_*`.

## Endpoints POS

### POST `/api/pos/debt-inquiry`
Headers:
- `apikey: <valor>`

Body:
```json
{
  "captureLine": "07001974047064739208",
  "storeId": "LEY-045",
  "cashRegister": "CAJA-07",
  "cashierId": "CJR-1024"
}
```

### POST `/api/pos/payment-apply`
Headers:
- `apikey: <valor>`

Body:
```json
{
  "transactionGuid": "1e9d71a6-f894-4257-8712-2bef846e1c89",
  "gesTraceId": "GES-TRX-20260526-1779819807759",
  "captureLine": "07001974047064739208",
  "amount": 38.0,
  "currency": "MXN",
  "paymentMethod": "CASH",
  "storeId": "LEY-045",
  "cashRegister": "CAJA-07",
  "cashierId": "CJR-1024",
  "ticketNumber": "TCK-045-070-0008891",
  "ticketTimestamp": "2026-05-26T11:24:05-07:00"
}
```

## Reglas operativas

- `transactionGuid` se genera en `debt-inquiry` por esta API.
- POS debe reutilizar ese `transactionGuid` en `payment-apply`.
- POS debe enviar en `payment-apply` la `captureLine` original de consulta.
- Idempotencia de pago por `transactionGuid + messageType`.

## Configuracion

`appsettings.json`:
- `ConnectionStrings:DefaultConnection`
- `Security:ApiKeyHeaderName`
- `GesApi:BaseUrl`
- `GesApi:ApiKey`
- `GesApi:DebtInquiryPath`
- `GesApi:PaymentApplyPath`

## Base de datos

Ejecutar scripts en orden:
1. `/scripts/001_GES_CoreTables.sql`
2. `/scripts/002_GES_Indexes.sql`
3. `/scripts/003_GES_Constraints.sql`
4. `/scripts/004_GES_StoredProcedures.sql`
5. `/scripts/005_GES_SeedApiKey.sql`
