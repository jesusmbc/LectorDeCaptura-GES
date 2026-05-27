SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    004_GES_StoredProcedures.sql
    Procedimientos utilitarios para seguridad, logs y mantenimiento.
*/

IF OBJECT_ID('dbo.GES_spValidateApiKey', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GES_spValidateApiKey
GO
CREATE PROCEDURE dbo.GES_spValidateApiKey
    @ApiKeyValue VARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        GES_ApiKeyId,
        ApiKeyValue,
        IsEnabled,
        ValidFrom,
        ValidTo
    FROM dbo.GES_ApiKeys
    WHERE ApiKeyValue = @ApiKeyValue
      AND IsEnabled = 1
      AND ValidFrom <= GETDATE()
      AND (ValidTo IS NULL OR ValidTo >= GETDATE());
END
GO

IF OBJECT_ID('dbo.GES_spInsertOperationLog', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GES_spInsertOperationLog
GO
CREATE PROCEDURE dbo.GES_spInsertOperationLog
    @TransactionGuid UNIQUEIDENTIFIER = NULL,
    @GesTraceId VARCHAR(60) = NULL,
    @Severity VARCHAR(10),
    @Source VARCHAR(100),
    @EventCode VARCHAR(50) = NULL,
    @LogMessage VARCHAR(1000),
    @ExceptionDetail NVARCHAR(MAX) = NULL,
    @RequestPath VARCHAR(200) = NULL,
    @HttpMethod VARCHAR(10) = NULL,
    @RemoteIp VARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.GES_OperationLogs
    (
        TransactionGuid,
        GesTraceId,
        Severity,
        Source,
        EventCode,
        LogMessage,
        ExceptionDetail,
        RequestPath,
        HttpMethod,
        RemoteIp
    )
    VALUES
    (
        @TransactionGuid,
        @GesTraceId,
        @Severity,
        @Source,
        @EventCode,
        @LogMessage,
        @ExceptionDetail,
        @RequestPath,
        @HttpMethod,
        @RemoteIp
    );
END
GO

IF OBJECT_ID('dbo.GES_spPurgeRetention', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GES_spPurgeRetention
GO
CREATE PROCEDURE dbo.GES_spPurgeRetention
    @RetentionMonths INT = 12
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME;
    SET @CutoffDate = DATEADD(MONTH, -@RetentionMonths, GETDATE());

    DELETE FROM dbo.GES_OperationLogs
    WHERE CreatedAt < @CutoffDate;

    DELETE FROM dbo.GES_EventStore
    WHERE CreatedAt < @CutoffDate;

    DELETE FROM dbo.GES_IdempotencyKeys
    WHERE CreatedAt < @CutoffDate;

    DELETE FROM dbo.GES_Transactions
    WHERE CreatedAt < @CutoffDate
      AND Status IN ('APPLIED', 'REJECTED', 'ALREADY_PAID', 'ERROR');
END
GO
