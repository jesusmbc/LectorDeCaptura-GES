namespace LineaDeCaptura.GES.Api.Contracts.Ges;

public sealed class GesPaymentContext
{
    public string CaptureLine { get; set; } = string.Empty;
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
}

public sealed class GesDebtInquiryResponse
{
    public string MessageType { get; set; } = string.Empty;
    public string TransactionGuid { get; set; } = string.Empty;
    public string GesTraceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = string.Empty;
    public string GesProcessedAt { get; set; } = string.Empty;
    public GesPaymentContext PaymentContext { get; set; } = new();
}
