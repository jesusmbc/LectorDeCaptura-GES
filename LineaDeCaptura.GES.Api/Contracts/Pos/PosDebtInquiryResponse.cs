namespace LineaDeCaptura.GES.Api.Contracts.Pos;

public sealed class PosDebtInquiryResponse
{
    public string TransactionGuid { get; set; } = string.Empty;
    public string GesTraceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = string.Empty;
    public string CaptureLine { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string ProcessedAt { get; set; } = string.Empty;
}
