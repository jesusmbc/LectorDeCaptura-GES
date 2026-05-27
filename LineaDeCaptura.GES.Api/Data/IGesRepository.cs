using LineaDeCaptura.GES.Api.Domain;

namespace LineaDeCaptura.GES.Api.Data;

public interface IGesRepository
{
    Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken);
    Task<TransactionRecord?> GetTransactionByGuidAsync(Guid transactionGuid, CancellationToken cancellationToken);
    Task InsertTransactionAsync(TransactionRecord record, CancellationToken cancellationToken);
    Task UpdateTransactionAfterInquiryAsync(Guid transactionGuid, string gesTraceId, string status, string statusCode, string statusMessage, decimal amount, string currency, CancellationToken cancellationToken);
    Task UpdateTransactionAfterPaymentAsync(
        Guid transactionGuid,
        string status,
        string statusCode,
        string statusMessage,
        string paymentMethod,
        string houseTicketNumber,
        DateTime houseTicketTimestamp,
        string authorization,
        string receiptFolio,
        CancellationToken cancellationToken);
    Task InsertEventAsync(Guid transactionGuid, string messageType, string eventPhase, string direction, string eventStatus, string eventStatusCode, string captureLine, string gesTraceId, string authorizationCode, string houseTicketNumber, string payloadJson, CancellationToken cancellationToken);
    Task InsertOperationLogAsync(Guid? transactionGuid, string? gesTraceId, string severity, string source, string eventCode, string message, string? exceptionDetail, string? requestPath, string? httpMethod, string? remoteIp, CancellationToken cancellationToken);
    Task<string?> TryGetIdempotentResponseAsync(Guid transactionGuid, string messageType, CancellationToken cancellationToken);
    Task SaveIdempotentResponseAsync(Guid transactionGuid, string messageType, string responseStatusCode, string responseBodyJson, bool isSuccess, CancellationToken cancellationToken);
}
