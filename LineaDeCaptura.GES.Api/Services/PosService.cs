using System.Text.Json;
using LineaDeCaptura.GES.Api.Contracts.Ges;
using LineaDeCaptura.GES.Api.Contracts.Pos;
using LineaDeCaptura.GES.Api.Data;
using LineaDeCaptura.GES.Api.Domain;

namespace LineaDeCaptura.GES.Api.Services;

public interface IPosService
{
    Task<PosDebtInquiryResponse> DebtInquiryAsync(PosDebtInquiryRequest request, string path, string method, string remoteIp, CancellationToken cancellationToken);
    Task<PosPaymentApplyResponse> PaymentApplyAsync(PosPaymentApplyRequest request, string path, string method, string remoteIp, CancellationToken cancellationToken);
}

public sealed class PosService : IPosService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IGesApiClient _gesApiClient;
    private readonly IGesRepository _repository;

    public PosService(IGesApiClient gesApiClient, IGesRepository repository)
    {
        _gesApiClient = gesApiClient;
        _repository = repository;
    }

    public async Task<PosDebtInquiryResponse> DebtInquiryAsync(PosDebtInquiryRequest request, string path, string method, string remoteIp, CancellationToken cancellationToken)
    {
        var transactionGuid = Guid.NewGuid();
        var transactionGuidText = transactionGuid.ToString();

        await _repository.InsertEventAsync(
            transactionGuid,
            "PosDebtInquiryRequest",
            "POS_INQUIRY",
            "IN",
            string.Empty,
            string.Empty,
            request.CaptureLine,
            string.Empty,
            string.Empty,
            string.Empty,
            JsonSerializer.Serialize(request, JsonOptions),
            cancellationToken);

        var gesRequest = new GesDebtInquiryRequest
        {
            TransactionGuid = transactionGuidText,
            RequestTimestamp = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            Channel = "POS",
            CaptureLine = request.CaptureLine,
            Commerce = new GesCommerce
            {
                StoreId = request.StoreId,
                CashRegister = request.CashRegister,
                CashierId = request.CashierId
            }
        };

        await _repository.InsertTransactionAsync(new TransactionRecord
        {
            TransactionGuid = transactionGuid,
            CaptureLine = request.CaptureLine,
            Status = "INQUIRED",
            StoreId = request.StoreId,
            CashRegister = request.CashRegister,
            CashierId = request.CashierId
        }, cancellationToken);

        await _repository.InsertEventAsync(
            transactionGuid,
            "DebtInquiryRequest",
            "GES_INQUIRY",
            "IN",
            string.Empty,
            string.Empty,
            request.CaptureLine,
            string.Empty,
            string.Empty,
            string.Empty,
            JsonSerializer.Serialize(gesRequest, JsonOptions),
            cancellationToken);

        var gesResponse = await _gesApiClient.DebtInquiryAsync(gesRequest, cancellationToken);

        await _repository.UpdateTransactionAfterInquiryAsync(
            transactionGuid,
            gesResponse.GesTraceId,
            MapStatus(gesResponse.Status),
            gesResponse.StatusCode,
            gesResponse.StatusMessage,
            gesResponse.PaymentContext.Amount,
            gesResponse.PaymentContext.Currency,
            cancellationToken);

        await _repository.InsertEventAsync(
            transactionGuid,
            "DebtInquiryResponse",
            "GES_INQUIRY",
            "OUT",
            gesResponse.Status,
            gesResponse.StatusCode,
            request.CaptureLine,
            gesResponse.GesTraceId,
            string.Empty,
            string.Empty,
            JsonSerializer.Serialize(gesResponse, JsonOptions),
            cancellationToken);

        await _repository.InsertOperationLogAsync(transactionGuid, gesResponse.GesTraceId, "INFO", "DebtInquiry", "INQUIRY_OK", "Consulta procesada exitosamente", null, path, method, remoteIp, cancellationToken);

        var posResponse = new PosDebtInquiryResponse
        {
            TransactionGuid = gesResponse.TransactionGuid,
            GesTraceId = gesResponse.GesTraceId,
            Status = gesResponse.Status,
            StatusCode = gesResponse.StatusCode,
            StatusMessage = gesResponse.StatusMessage,
            CaptureLine = gesResponse.PaymentContext.CaptureLine,
            Amount = gesResponse.PaymentContext.Amount,
            Currency = gesResponse.PaymentContext.Currency,
            ServiceCode = gesResponse.PaymentContext.ServiceCode,
            ServiceName = gesResponse.PaymentContext.ServiceName,
            DueDate = gesResponse.PaymentContext.DueDate,
            ProcessedAt = gesResponse.GesProcessedAt
        };

        await _repository.InsertEventAsync(
            transactionGuid,
            "PosDebtInquiryResponse",
            "POS_INQUIRY",
            "OUT",
            posResponse.Status,
            posResponse.StatusCode,
            request.CaptureLine,
            posResponse.GesTraceId,
            string.Empty,
            string.Empty,
            JsonSerializer.Serialize(posResponse, JsonOptions),
            cancellationToken);

        return posResponse;
    }

    public async Task<PosPaymentApplyResponse> PaymentApplyAsync(PosPaymentApplyRequest request, string path, string method, string remoteIp, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TransactionGuid, out var transactionGuid))
        {
            throw new InvalidOperationException("transactionGuid is not valid.");
        }

        await _repository.InsertEventAsync(
            transactionGuid,
            "PosPaymentApplyRequest",
            "POS_APPLY",
            "IN",
            string.Empty,
            string.Empty,
            request.CaptureLine,
            request.GesTraceId,
            string.Empty,
            request.TicketNumber,
            JsonSerializer.Serialize(request, JsonOptions),
            cancellationToken);

        var existingResponse = await _repository.TryGetIdempotentResponseAsync(transactionGuid, "PaymentApplyRequest", cancellationToken);
        if (!string.IsNullOrWhiteSpace(existingResponse))
        {
            var cached = JsonSerializer.Deserialize<PosPaymentApplyResponse>(existingResponse, JsonOptions);
            if (cached != null)
            {
                await _repository.InsertEventAsync(
                    transactionGuid,
                    "PosPaymentApplyResponse",
                    "POS_APPLY",
                    "OUT",
                    cached.Status,
                    cached.StatusCode,
                    request.CaptureLine,
                    cached.GesTraceId,
                    cached.Authorization,
                    request.TicketNumber,
                    JsonSerializer.Serialize(cached, JsonOptions),
                    cancellationToken);

                await _repository.InsertOperationLogAsync(transactionGuid, request.GesTraceId, "INFO", "PaymentApply", "IDEMPOTENT_HIT", "Se devolvio respuesta idempotente existente", null, path, method, remoteIp, cancellationToken);
                return cached;
            }
        }

        var transaction = await _repository.GetTransactionByGuidAsync(transactionGuid, cancellationToken);
        if (transaction == null)
        {
            throw new InvalidOperationException("Transaction does not exist. Execute debt inquiry first.");
        }

        if (!string.Equals(transaction.CaptureLine, request.CaptureLine, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Capture line does not match inquiry request.");
        }

        var gesRequest = new GesPaymentApplyRequest
        {
            TransactionGuid = request.TransactionGuid,
            RequestTimestamp = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            GesTraceId = request.GesTraceId,
            CaptureLine = request.CaptureLine,
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentMethod = request.PaymentMethod,
            Commerce = new GesCommerce
            {
                StoreId = request.StoreId,
                CashRegister = request.CashRegister,
                CashierId = request.CashierId
            },
            HouseTicket = new GesHouseTicket
            {
                TicketNumber = request.TicketNumber,
                TicketTimestamp = request.TicketTimestamp.ToString("yyyy-MM-ddTHH:mm:sszzz")
            }
        };

        await _repository.InsertEventAsync(
            transactionGuid,
            "PaymentApplyRequest",
            "GES_APPLY",
            "IN",
            string.Empty,
            string.Empty,
            request.CaptureLine,
            request.GesTraceId,
            string.Empty,
            request.TicketNumber,
            JsonSerializer.Serialize(gesRequest, JsonOptions),
            cancellationToken);

        var gesResponse = await _gesApiClient.PaymentApplyAsync(gesRequest, cancellationToken);

        await _repository.UpdateTransactionAfterPaymentAsync(
            transactionGuid,
            MapStatus(gesResponse.Status),
            gesResponse.StatusCode,
            gesResponse.StatusMessage,
            request.PaymentMethod,
            request.TicketNumber,
            request.TicketTimestamp.UtcDateTime,
            gesResponse.Authorization,
            gesResponse.OfficialReceipt.ReceiptFolio,
            cancellationToken);

        await _repository.InsertEventAsync(
            transactionGuid,
            "PaymentApplyResponse",
            "GES_APPLY",
            "OUT",
            gesResponse.Status,
            gesResponse.StatusCode,
            request.CaptureLine,
            gesResponse.GesTraceId,
            gesResponse.Authorization,
            request.TicketNumber,
            JsonSerializer.Serialize(gesResponse, JsonOptions),
            cancellationToken);

        var posResponse = new PosPaymentApplyResponse
        {
            TransactionGuid = gesResponse.TransactionGuid,
            GesTraceId = gesResponse.GesTraceId,
            Status = gesResponse.Status,
            StatusCode = gesResponse.StatusCode,
            StatusMessage = gesResponse.StatusMessage,
            Authorization = gesResponse.Authorization,
            ReceiptFolio = gesResponse.OfficialReceipt.ReceiptFolio,
            ProcessedAt = gesResponse.GesProcessedAt
        };

        var responseJson = JsonSerializer.Serialize(posResponse, JsonOptions);
        await _repository.SaveIdempotentResponseAsync(transactionGuid, "PaymentApplyRequest", gesResponse.StatusCode, responseJson, true, cancellationToken);

        await _repository.InsertEventAsync(
            transactionGuid,
            "PosPaymentApplyResponse",
            "POS_APPLY",
            "OUT",
            posResponse.Status,
            posResponse.StatusCode,
            request.CaptureLine,
            posResponse.GesTraceId,
            posResponse.Authorization,
            request.TicketNumber,
            responseJson,
            cancellationToken);

        await _repository.InsertOperationLogAsync(transactionGuid, gesResponse.GesTraceId, "INFO", "PaymentApply", "PAYMENT_APPLY_OK", "Afectacion de pago procesada exitosamente", null, path, method, remoteIp, cancellationToken);

        return posResponse;
    }

    private static string MapStatus(string gesStatus)
    {
        return gesStatus switch
        {
            "OK" => "INQUIRED",
            "APPLIED" => "APPLIED",
            "ALREADY_PAID" => "ALREADY_PAID",
            "REJECTED" => "REJECTED",
            _ => "ERROR"
        };
    }
}
