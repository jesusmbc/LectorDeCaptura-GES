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

    private static object ToDbValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
    }
}
