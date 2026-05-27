SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    002_GES_Indexes.sql
    Crea índices de consulta rápida para conciliación y rastreo.
*/

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GES_Transactions_CaptureLine' AND object_id = OBJECT_ID('dbo.GES_Transactions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GES_Transactions_CaptureLine
    ON dbo.GES_Transactions (CaptureLine)
    INCLUDE (Status, GesTraceId, RequestedAmount, CurrencyCode, UpdatedAt)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GES_Transactions_GesTraceId' AND object_id = OBJECT_ID('dbo.GES_Transactions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GES_Transactions_GesTraceId
    ON dbo.GES_Transactions (GesTraceId)
    INCLUDE (TransactionGuid, Status, UpdatedAt)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GES_Transactions_UpdatedAt' AND object_id = OBJECT_ID('dbo.GES_Transactions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GES_Transactions_UpdatedAt
    ON dbo.GES_Transactions (UpdatedAt)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GES_EventStore_TransactionGuid' AND object_id = OBJECT_ID('dbo.GES_EventStore'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GES_EventStore_TransactionGuid
    ON dbo.GES_EventStore (TransactionGuid, CreatedAt)
    INCLUDE (MessageType, EventPhase, Direction, EventStatus, EventStatusCode, CaptureLine, GesTraceId)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GES_EventStore_CaptureLine' AND object_id = OBJECT_ID('dbo.GES_EventStore'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GES_EventStore_CaptureLine
    ON dbo.GES_EventStore (CaptureLine, CreatedAt)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GES_OperationLogs_TransactionGuid' AND object_id = OBJECT_ID('dbo.GES_OperationLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GES_OperationLogs_TransactionGuid
    ON dbo.GES_OperationLogs (TransactionGuid, CreatedAt)
    INCLUDE (Severity, Source, EventCode)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GES_OperationLogs_CreatedAt' AND object_id = OBJECT_ID('dbo.GES_OperationLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GES_OperationLogs_CreatedAt
    ON dbo.GES_OperationLogs (CreatedAt)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GES_IdempotencyKeys_CreatedAt' AND object_id = OBJECT_ID('dbo.GES_IdempotencyKeys'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GES_IdempotencyKeys_CreatedAt
    ON dbo.GES_IdempotencyKeys (CreatedAt)
END
GO
