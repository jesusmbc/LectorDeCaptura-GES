using LineaDeCaptura.GES.Api.Domain;
using Microsoft.Data.SqlClient;

namespace LineaDeCaptura.GES.Api.Data;

public sealed class GesRepository : IGesRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public GesRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP (1) 1
FROM dbo.GES_ApiKeys
WHERE ApiKeyValue = @ApiKeyValue
  AND IsEnabled = 1
  AND ValidFrom <= GETDATE()
  AND (ValidTo IS NULL OR ValidTo >= GETDATE());";

        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ApiKeyValue", apiKey);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result != null;
    }

    public async Task<TransactionRecord?> GetTransactionByGuidAsync(Guid transactionGuid, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP (1)
    TransactionGuid,
    CaptureLine,
    ISNULL(GesTraceId, '') AS GesTraceId,
    Status,
    ISNULL(StatusCode, '') AS StatusCode,
    ISNULL(StatusMessage, '') AS StatusMessage,
    ISNULL(StoreId, '') AS StoreId,
    ISNULL(CashRegister, '') AS CashRegister,
    ISNULL(CashierId, '') AS CashierId,
    RequestedAmount,
    ISNULL(CurrencyCode, '') AS CurrencyCode
FROM dbo.GES_Transactions
WHERE TransactionGuid = @TransactionGuid;";

        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TransactionGuid", transactionGuid);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new TransactionRecord
        {
            TransactionGuid = reader.GetGuid(0),
            CaptureLine = reader.GetString(1),
            GesTraceId = reader.GetString(2),
            Status = reader.GetString(3),
            StatusCode = reader.GetString(4),
            StatusMessage = reader.GetString(5),
            StoreId = reader.GetString(6),
            CashRegister = reader.GetString(7),
            CashierId = reader.GetString(8),
            RequestedAmount = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
            CurrencyCode = reader.GetString(10)
        };
    }

    public async Task InsertTransactionAsync(TransactionRecord record, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO dbo.GES_Transactions
(
    TransactionGuid, CaptureLine, Status, StatusCode, StatusMessage, Channel,
    StoreId, CashRegister, CashierId, RequestedAmount, CurrencyCode, UpdatedAt
)
VALUES
(
    @TransactionGuid, @CaptureLine, @Status, @StatusCode, @StatusMessage, 'POS',
    @StoreId, @CashRegister, @CashierId, @RequestedAmount, @CurrencyCode, GETDATE()
);";

        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TransactionGuid", record.TransactionGuid);
        cmd.Parameters.AddWithValue("@CaptureLine", record.CaptureLine);
        cmd.Parameters.AddWithValue("@Status", record.Status);
        cmd.Parameters.AddWithValue("@StatusCode", ToDbValue(record.StatusCode));
        cmd.Parameters.AddWithValue("@StatusMessage", ToDbValue(record.StatusMessage));
        cmd.Parameters.AddWithValue("@StoreId", ToDbValue(record.StoreId));
        cmd.Parameters.AddWithValue("@CashRegister", ToDbValue(record.CashRegister));
        cmd.Parameters.AddWithValue("@CashierId", ToDbValue(record.CashierId));
        cmd.Parameters.AddWithValue("@RequestedAmount", record.RequestedAmount.HasValue ? record.RequestedAmount.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@CurrencyCode", ToDbValue(record.CurrencyCode));
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateTransactionAfterInquiryAsync(Guid transactionGuid, string gesTraceId, string status, string statusCode, string statusMessage, decimal amount, string currency, CancellationToken cancellationToken)
    {
        const string sql = @"
UPDATE dbo.GES_Transactions
SET GesTraceId = @GesTraceId,
    Status = @Status,
    StatusCode = @StatusCode,
    StatusMessage = @StatusMessage,
    RequestedAmount = @RequestedAmount,
    CurrencyCode = @CurrencyCode,
    LastMessageType = 'DebtInquiryResponse',
    UpdatedAt = GETDATE()
WHERE TransactionGuid = @TransactionGuid;";

        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TransactionGuid", transactionGuid);
        cmd.Parameters.AddWithValue("@GesTraceId", ToDbValue(gesTraceId));
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@StatusCode", ToDbValue(statusCode));
        cmd.Parameters.AddWithValue("@StatusMessage", ToDbValue(statusMessage));
        cmd.Parameters.AddWithValue("@RequestedAmount", amount);
        cmd.Parameters.AddWithValue("@CurrencyCode", ToDbValue(currency));
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateTransactionAfterPaymentAsync(
        Guid transactionGuid,
        string status,
        string statusCode,
        string statusMessage,
        string paymentMethod,
        string houseTicketNumber,
        DateTime houseTicketTimestamp,
        string authorization,
        string receiptFolio,
        CancellationToken cancellationToken)
    {
        const string sql = @"
UPDATE dbo.GES_Transactions
SET Status = @Status,
    StatusCode = @StatusCode,
    StatusMessage = @StatusMessage,
    PaymentMethod = @PaymentMethod,
    HouseTicketNumber = @HouseTicketNumber,
    HouseTicketTimestamp = @HouseTicketTimestamp,
    AuthorizationCode = @Authorization,
    OfficialReceiptFolio = @OfficialReceiptFolio,
    LastMessageType = 'PaymentApplyResponse',
    UpdatedAt = GETDATE()
WHERE TransactionGuid = @TransactionGuid;";

        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TransactionGuid", transactionGuid);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@StatusCode", ToDbValue(statusCode));
        cmd.Parameters.AddWithValue("@StatusMessage", ToDbValue(statusMessage));
        cmd.Parameters.AddWithValue("@PaymentMethod", ToDbValue(paymentMethod));
        cmd.Parameters.AddWithValue("@HouseTicketNumber", ToDbValue(houseTicketNumber));
        cmd.Parameters.AddWithValue("@HouseTicketTimestamp", houseTicketTimestamp);
        cmd.Parameters.AddWithValue("@Authorization", ToDbValue(authorization));
        cmd.Parameters.AddWithValue("@OfficialReceiptFolio", ToDbValue(receiptFolio));
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertEventAsync(Guid transactionGuid, string messageType, string eventPhase, string direction, string eventStatus, string eventStatusCode, string captureLine, string gesTraceId, string authorizationCode, string houseTicketNumber, string payloadJson, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO dbo.GES_EventStore
(
    TransactionGuid, MessageType, EventPhase, Direction, EventStatus, EventStatusCode,
    CaptureLine, GesTraceId, AuthorizationCode, HouseTicketNumber, PayloadJson
)
VALUES
(
    @TransactionGuid, @MessageType, @EventPhase, @Direction, @EventStatus, @EventStatusCode,
    @CaptureLine, @GesTraceId, @AuthorizationCode, @HouseTicketNumber, @PayloadJson
);";

        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TransactionGuid", transactionGuid);
        cmd.Parameters.AddWithValue("@MessageType", messageType);
        cmd.Parameters.AddWithValue("@EventPhase", eventPhase);
        cmd.Parameters.AddWithValue("@Direction", direction);
        cmd.Parameters.AddWithValue("@EventStatus", ToDbValue(eventStatus));
        cmd.Parameters.AddWithValue("@EventStatusCode", ToDbValue(eventStatusCode));
        cmd.Parameters.AddWithValue("@CaptureLine", ToDbValue(captureLine));
        cmd.Parameters.AddWithValue("@GesTraceId", ToDbValue(gesTraceId));
        cmd.Parameters.AddWithValue("@AuthorizationCode", ToDbValue(authorizationCode));
        cmd.Parameters.AddWithValue("@HouseTicketNumber", ToDbValue(houseTicketNumber));
        cmd.Parameters.AddWithValue("@PayloadJson", payloadJson);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertOperationLogAsync(Guid? transactionGuid, string? gesTraceId, string severity, string source, string eventCode, string message, string? exceptionDetail, string? requestPath, string? httpMethod, string? remoteIp, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO dbo.GES_OperationLogs
(
    TransactionGuid, GesTraceId, Severity, Source, EventCode, LogMessage,
    ExceptionDetail, RequestPath, HttpMethod, RemoteIp
)
VALUES
(
    @TransactionGuid, @GesTraceId, @Severity, @Source, @EventCode, @LogMessage,
    @ExceptionDetail, @RequestPath, @HttpMethod, @RemoteIp
);";

        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TransactionGuid", transactionGuid.HasValue ? transactionGuid.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@GesTraceId", ToDbValue(gesTraceId));
        cmd.Parameters.AddWithValue("@Severity", severity);
        cmd.Parameters.AddWithValue("@Source", source);
        cmd.Parameters.AddWithValue("@EventCode", ToDbValue(eventCode));
        cmd.Parameters.AddWithValue("@LogMessage", message);
        cmd.Parameters.AddWithValue("@ExceptionDetail", ToDbValue(exceptionDetail));
        cmd.Parameters.AddWithValue("@RequestPath", ToDbValue(requestPath));
        cmd.Parameters.AddWithValue("@HttpMethod", ToDbValue(httpMethod));
        cmd.Parameters.AddWithValue("@RemoteIp", ToDbValue(remoteIp));
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<string?> TryGetIdempotentResponseAsync(Guid transactionGuid, string messageType, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT TOP (1) ResponseBodyJson
FROM dbo.GES_IdempotencyKeys
WHERE TransactionGuid = @TransactionGuid
  AND MessageType = @MessageType
  AND IsSuccess = 1;";

        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TransactionGuid", transactionGuid);
        cmd.Parameters.AddWithValue("@MessageType", messageType);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result == null || result == DBNull.Value ? null : Convert.ToString(result);
    }

    public async Task SaveIdempotentResponseAsync(Guid transactionGuid, string messageType, string responseStatusCode, string responseBodyJson, bool isSuccess, CancellationToken cancellationToken)
    {
        const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.GES_IdempotencyKeys WHERE TransactionGuid = @TransactionGuid AND MessageType = @MessageType)
BEGIN
    UPDATE dbo.GES_IdempotencyKeys
    SET ResponseStatusCode = @ResponseStatusCode,
        ResponseBodyJson = @ResponseBodyJson,
        IsSuccess = @IsSuccess
    WHERE TransactionGuid = @TransactionGuid
      AND MessageType = @MessageType;
END
ELSE
BEGIN
    INSERT INTO dbo.GES_IdempotencyKeys
    (
        TransactionGuid, MessageType, ResponseStatusCode, ResponseBodyJson, IsSuccess
    )
    VALUES
    (
        @TransactionGuid, @MessageType, @ResponseStatusCode, @ResponseBodyJson, @IsSuccess
    );
END";

        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@TransactionGuid", transactionGuid);
        cmd.Parameters.AddWithValue("@MessageType", messageType);
        cmd.Parameters.AddWithValue("@ResponseStatusCode", ToDbValue(responseStatusCode));
        cmd.Parameters.AddWithValue("@ResponseBodyJson", ToDbValue(responseBodyJson));
        cmd.Parameters.AddWithValue("@IsSuccess", isSuccess);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReconciliationCsvRow>> GetReconciliationRowsAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
    {
        const string sql = @"
WITH EventAgg AS
(
    SELECT
        es.TransactionGuid,
        MIN(CASE WHEN es.MessageType IN ('DebtInquiryRequest', 'DebtInquiryResponse', 'PosDebtInquiryRequest', 'PosDebtInquiryResponse') THEN es.CreatedAt END) AS InquiryAt,
        MIN(CASE WHEN es.MessageType IN ('PaymentApplyRequest', 'PaymentApplyResponse', 'PosPaymentApplyRequest', 'PosPaymentApplyResponse') THEN es.CreatedAt END) AS ApplyAt,
        MAX(CASE WHEN es.MessageType = 'PosPaymentApplyRequest' THEN JSON_VALUE(es.PayloadJson, '$.captureLine') END) AS CaptureLinePosApply,
        MAX(CASE WHEN es.MessageType = 'DebtInquiryResponse' THEN JSON_VALUE(es.PayloadJson, '$.paymentContext.captureLine') END) AS CaptureLineGesResponse,
        MAX(CASE WHEN es.MessageType = 'PosPaymentApplyRequest' THEN TRY_CONVERT(DECIMAL(18,2), JSON_VALUE(es.PayloadJson, '$.amount')) END) AS AmountPos,
        MAX(CASE WHEN es.MessageType = 'PosPaymentApplyRequest' THEN JSON_VALUE(es.PayloadJson, '$.currency') END) AS CurrencyPos,
        SUM(CASE WHEN es.MessageType = 'PosPaymentApplyResponse' THEN 1 ELSE 0 END) AS PosApplyResponseCount,
        SUM(CASE WHEN es.MessageType IN ('DebtInquiryRequest', 'DebtInquiryResponse', 'PosDebtInquiryRequest', 'PosDebtInquiryResponse') THEN 1 ELSE 0 END) AS InquiryCount,
        SUM(CASE WHEN es.MessageType IN ('PaymentApplyRequest', 'PaymentApplyResponse', 'PosPaymentApplyRequest', 'PosPaymentApplyResponse') THEN 1 ELSE 0 END) AS ApplyCount
    FROM dbo.GES_EventStore es
    GROUP BY es.TransactionGuid
)
SELECT
    t.TransactionGuid,
    ISNULL(t.GesTraceId, '') AS GesTraceId,
    ISNULL(t.StoreId, '') AS StoreId,
    ISNULL(t.CashRegister, '') AS CashRegister,
    ISNULL(t.CashierId, '') AS CashierId,
    ISNULL(t.PaymentMethod, '') AS PaymentMethod,
    ISNULL(t.HouseTicketNumber, '') AS TicketNumber,
    t.HouseTicketTimestamp AS TicketTimestamp,
    ISNULL(NULLIF(ea.CaptureLinePosApply, ''), t.CaptureLine) AS CaptureLineOriginalPos,
    ISNULL(NULLIF(ea.CaptureLineGesResponse, ''), '') AS CaptureLineGesResponse,
    ea.AmountPos,
    ISNULL(ea.CurrencyPos, '') AS CurrencyPos,
    t.RequestedAmount AS AmountGes,
    ISNULL(t.CurrencyCode, '') AS CurrencyGes,
    ISNULL(t.Status, '') AS StatusFinalInterno,
    ISNULL(t.StatusCode, '') AS StatusCodeFinal,
    ISNULL(t.StatusMessage, '') AS StatusMessageFinal,
    ISNULL(t.AuthorizationCode, '') AS [Authorization],
    ISNULL(t.OfficialReceiptFolio, '') AS ReceiptFolio,
    ea.InquiryAt,
    ea.ApplyAt,
    t.CreatedAt,
    t.UpdatedAt,
    CASE WHEN ISNULL(ea.PosApplyResponseCount, 0) > 1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsIdempotent,
    CASE WHEN t.Status = 'ERROR' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS ErrorFlag,
    CASE WHEN t.Status = 'ERROR' THEN ISNULL(t.StatusMessage, 'ERROR') ELSE '' END AS ErrorMessage,
    CASE WHEN ea.AmountPos IS NOT NULL AND t.RequestedAmount IS NOT NULL AND ea.AmountPos = t.RequestedAmount THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS AmountMatch,
    CASE
        WHEN ISNULL(NULLIF(ea.CaptureLinePosApply, ''), t.CaptureLine) <> '' AND ISNULL(NULLIF(ea.CaptureLineGesResponse, ''), t.CaptureLine) <> '' 
             AND ISNULL(NULLIF(ea.CaptureLinePosApply, ''), t.CaptureLine) = ISNULL(NULLIF(ea.CaptureLineGesResponse, ''), t.CaptureLine)
        THEN CAST(1 AS BIT)
        ELSE CAST(0 AS BIT)
    END AS CaptureLineMatch,
    CASE WHEN ISNULL(ea.InquiryCount, 0) > 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS HasInquiry,
    CASE WHEN ISNULL(ea.ApplyCount, 0) > 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS HasApply,
    CASE WHEN ISNULL(ea.InquiryCount, 0) > 0 AND ISNULL(ea.ApplyCount, 0) = 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS InquiryWithoutApply,
    CASE WHEN ISNULL(ea.ApplyCount, 0) > 0 AND ISNULL(ea.InquiryCount, 0) = 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS ApplyWithoutInquiry,
    CASE
        WHEN t.Status = 'APPLIED' THEN 'APPLIED'
        WHEN t.Status = 'REJECTED' THEN 'REJECTED'
        WHEN t.Status = 'ALREADY_PAID' THEN 'ALREADY_PAID'
        ELSE 'ERROR'
    END AS FinalResult
FROM dbo.GES_Transactions t
LEFT JOIN EventAgg ea ON ea.TransactionGuid = t.TransactionGuid
WHERE t.CreatedAt >= @FechaInicio
  AND t.CreatedAt < DATEADD(DAY, 1, @FechaFin)
  AND t.Status = 'APPLIED'
  AND ISNULL(t.AuthorizationCode, '') <> ''
  ORDER BY t.CreatedAt ASC;";

        var rows = new List<ReconciliationCsvRow>();
        using var connection = _connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ReconciliationCsvRow
            {
                TransactionGuid = reader.GetGuid(0),
                GesTraceId = reader.GetString(1),
                StoreId = reader.GetString(2),
                CashRegister = reader.GetString(3),
                CashierId = reader.GetString(4),
                PaymentMethod = reader.GetString(5),
                TicketNumber = reader.GetString(6),
                TicketTimestamp = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                CaptureLineOriginalPos = reader.GetString(8),
                CaptureLineGesResponse = reader.GetString(9),
                AmountPos = reader.IsDBNull(10) ? null : reader.GetDecimal(10),
                CurrencyPos = reader.GetString(11),
                AmountGes = reader.IsDBNull(12) ? null : reader.GetDecimal(12),
                CurrencyGes = reader.GetString(13),
                StatusFinalInterno = reader.GetString(14),
                StatusCodeFinal = reader.GetString(15),
                StatusMessageFinal = reader.GetString(16),
                Authorization = reader.GetString(17),
                ReceiptFolio = reader.GetString(18),
                InquiryAt = reader.IsDBNull(19) ? null : reader.GetDateTime(19),
                ApplyAt = reader.IsDBNull(20) ? null : reader.GetDateTime(20),
                CreatedAt = reader.GetDateTime(21),
                UpdatedAt = reader.GetDateTime(22),
                IsIdempotent = reader.GetBoolean(23),
                ErrorFlag = reader.GetBoolean(24),
                ErrorMessage = reader.GetString(25),
                AmountMatch = reader.GetBoolean(26),
                CaptureLineMatch = reader.GetBoolean(27),
                HasInquiry = reader.GetBoolean(28),
                HasApply = reader.GetBoolean(29),
                InquiryWithoutApply = reader.GetBoolean(30),
                ApplyWithoutInquiry = reader.GetBoolean(31),
                FinalResult = reader.GetString(32)
            });
        }

        return rows;
    }

    private static object ToDbValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
    }
}
