SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    005_GES_SeedApiKey.sql
    Inserta o actualiza una sola API Key activa.
*/

DECLARE @ApiKeyValue VARCHAR(128);
SET @ApiKeyValue = 'test_key';

IF EXISTS (SELECT 1 FROM dbo.GES_ApiKeys WHERE ApiKeyValue = @ApiKeyValue)
BEGIN
    UPDATE dbo.GES_ApiKeys
    SET IsEnabled = 1,
        ValidTo = NULL
    WHERE ApiKeyValue = @ApiKeyValue;
END
ELSE
BEGIN
    INSERT INTO dbo.GES_ApiKeys
    (
        ApiKeyValue,
        IsEnabled,
        ValidFrom,
        ValidTo
    )
    VALUES
    (
        @ApiKeyValue,
        1,
        GETDATE(),
        NULL
    );
END
GO
