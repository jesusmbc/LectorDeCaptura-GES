namespace LineaDeCaptura.GES.Api.Domain;

public sealed class ReconciliationCsvRow
{
    public Guid TransactionGuid { get; set; }
    public string GesTraceId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public string CashRegister { get; set; } = string.Empty;
    public string CashierId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;
    public DateTime? TicketTimestamp { get; set; }
    public string CaptureLineOriginalPos { get; set; } = string.Empty;
    public string CaptureLineGesResponse { get; set; } = string.Empty;
    public decimal? AmountPos { get; set; }
    public string CurrencyPos { get; set; } = string.Empty;
    public decimal? AmountGes { get; set; }
    public string CurrencyGes { get; set; } = string.Empty;
    public string StatusFinalInterno { get; set; } = string.Empty;
    public string StatusCodeFinal { get; set; } = string.Empty;
    public string StatusMessageFinal { get; set; } = string.Empty;
    public string Authorization { get; set; } = string.Empty;
    public string ReceiptFolio { get; set; } = string.Empty;
    public DateTime? InquiryAt { get; set; }
    public DateTime? ApplyAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsIdempotent { get; set; }
    public bool ErrorFlag { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public bool AmountMatch { get; set; }
    public bool CaptureLineMatch { get; set; }
    public bool HasInquiry { get; set; }
    public bool HasApply { get; set; }
    public bool InquiryWithoutApply { get; set; }
    public bool ApplyWithoutInquiry { get; set; }
    public string FinalResult { get; set; } = string.Empty;
}
