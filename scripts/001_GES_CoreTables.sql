SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    001_GES_CoreTables.sql
    Crea tablas base del proyecto GES sin crear base de datos.
*/

IF OBJECT_ID('dbo.GES_Transactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GES_Transactions
    (
        GES_TransactionId BIGINT IDENTITY(1,1) NOT NULL,
        TransactionGuid UNIQUEIDENTIFIER NOT NULL,
        CaptureLine VARCHAR(50) NOT NULL,
        GesTraceId VARCHAR(60) NULL,
        Status VARCHAR(30) NOT NULL,
        StatusCode VARCHAR(20) NULL,
        StatusMessage VARCHAR(400) NULL,
        Channel VARCHAR(20) NOT NULL,
        StoreId VARCHAR(20) NULL,
        CashRegister VARCHAR(20) NULL,
        CashierId VARCHAR(20) NULL,
        RequestedAmount DECIMAL(18,2) NULL,
        CurrencyCode VARCHAR(10) NULL,
        PaymentMethod VARCHAR(20) NULL,
        HouseTicketNumber VARCHAR(50) NULL,
        HouseTicketTimestamp DATETIME NULL,
        AuthorizationCode VARCHAR(80) NULL,
        OfficialReceiptFolio VARCHAR(80) NULL,
        LastMessageType VARCHAR(40) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_GES_Transactions_IsActive DEFAULT (1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GES_Transactions_CreatedAt DEFAULT (GETDATE()),
        UpdatedAt DATETIME NOT NULL CONSTRAINT DF_GES_Transactions_UpdatedAt DEFAULT (GETDATE()),
        CONSTRAINT PK_GES_Transactions PRIMARY KEY CLUSTERED (GES_TransactionId),
        CONSTRAINT UQ_GES_Transactions_TransactionGuid UNIQUE (TransactionGuid)
    )
END
GO

IF OBJECT_ID('dbo.GES_EventStore', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GES_EventStore
    (
        GES_EventId BIGINT IDENTITY(1,1) NOT NULL,
        TransactionGuid UNIQUEIDENTIFIER NOT NULL,
        MessageType VARCHAR(40) NOT NULL,
        EventPhase VARCHAR(20) NOT NULL,
        Direction VARCHAR(10) NOT NULL,
        EventStatus VARCHAR(30) NULL,
        EventStatusCode VARCHAR(20) NULL,
        CaptureLine VARCHAR(50) NULL,
        GesTraceId VARCHAR(60) NULL,
        AuthorizationCode VARCHAR(80) NULL,
        HouseTicketNumber VARCHAR(50) NULL,
        PayloadJson NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GES_EventStore_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT PK_GES_EventStore PRIMARY KEY CLUSTERED (GES_EventId)
    )
END
GO

IF OBJECT_ID('dbo.GES_IdempotencyKeys', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GES_IdempotencyKeys
    (
        GES_IdempotencyKeyId BIGINT IDENTITY(1,1) NOT NULL,
        TransactionGuid UNIQUEIDENTIFIER NOT NULL,
        MessageType VARCHAR(40) NOT NULL,
        RequestHash VARCHAR(128) NULL,
        ResponseStatusCode VARCHAR(20) NULL,
        ResponseBodyJson NVARCHAR(MAX) NULL,
        IsSuccess BIT NOT NULL CONSTRAINT DF_GES_IdempotencyKeys_IsSuccess DEFAULT (0),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GES_IdempotencyKeys_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT PK_GES_IdempotencyKeys PRIMARY KEY CLUSTERED (GES_IdempotencyKeyId),
        CONSTRAINT UQ_GES_IdempotencyKeys_Key UNIQUE (TransactionGuid, MessageType)
    )
END
GO

IF OBJECT_ID('dbo.GES_OperationLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GES_OperationLogs
    (
        GES_OperationLogId BIGINT IDENTITY(1,1) NOT NULL,
        TransactionGuid UNIQUEIDENTIFIER NULL,
        GesTraceId VARCHAR(60) NULL,
        Severity VARCHAR(10) NOT NULL,
        Source VARCHAR(100) NOT NULL,
        EventCode VARCHAR(50) NULL,
        LogMessage VARCHAR(1000) NOT NULL,
        ExceptionDetail NVARCHAR(MAX) NULL,
        RequestPath VARCHAR(200) NULL,
        HttpMethod VARCHAR(10) NULL,
        RemoteIp VARCHAR(45) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GES_OperationLogs_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT PK_GES_OperationLogs PRIMARY KEY CLUSTERED (GES_OperationLogId)
    )
END
GO

IF OBJECT_ID('dbo.GES_ApiKeys', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GES_ApiKeys
    (
        GES_ApiKeyId INT IDENTITY(1,1) NOT NULL,
        ApiKeyValue VARCHAR(128) NOT NULL,
        IsEnabled BIT NOT NULL CONSTRAINT DF_GES_ApiKeys_IsEnabled DEFAULT (1),
        ValidFrom DATETIME NOT NULL CONSTRAINT DF_GES_ApiKeys_ValidFrom DEFAULT (GETDATE()),
        ValidTo DATETIME NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GES_ApiKeys_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT PK_GES_ApiKeys PRIMARY KEY CLUSTERED (GES_ApiKeyId),
        CONSTRAINT UQ_GES_ApiKeys_ApiKeyValue UNIQUE (ApiKeyValue)
    )
END
GO
