SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    003_GES_Constraints.sql
    Agrega validaciones básicas de datos para evitar basura operativa.
*/

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_GES_Transactions_Status'
      AND parent_object_id = OBJECT_ID('dbo.GES_Transactions')
)
BEGIN
    ALTER TABLE dbo.GES_Transactions
    ADD CONSTRAINT CK_GES_Transactions_Status
    CHECK (Status IN ('INQUIRED', 'APPLIED', 'REJECTED', 'ALREADY_PAID', 'ERROR'))
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_GES_OperationLogs_Severity'
      AND parent_object_id = OBJECT_ID('dbo.GES_OperationLogs')
)
BEGIN
    ALTER TABLE dbo.GES_OperationLogs
    ADD CONSTRAINT CK_GES_OperationLogs_Severity
    CHECK (Severity IN ('INFO', 'WARN', 'ERROR'))
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_GES_EventStore_Direction'
      AND parent_object_id = OBJECT_ID('dbo.GES_EventStore')
)
BEGIN
    ALTER TABLE dbo.GES_EventStore
    ADD CONSTRAINT CK_GES_EventStore_Direction
    CHECK (Direction IN ('IN', 'OUT'))
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_GES_ApiKeys_Validity'
      AND parent_object_id = OBJECT_ID('dbo.GES_ApiKeys')
)
BEGIN
    ALTER TABLE dbo.GES_ApiKeys
    ADD CONSTRAINT CK_GES_ApiKeys_Validity
    CHECK (ValidTo IS NULL OR ValidTo >= ValidFrom)
END
GO
