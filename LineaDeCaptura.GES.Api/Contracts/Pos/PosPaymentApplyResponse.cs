namespace LineaDeCaptura.GES.Api.Contracts.Pos;

public sealed class PosPaymentApplyResponse
{
    public string TransactionGuid { get; set; } = string.Empty;
    public string GesTraceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = string.Empty;
    public string Authorization { get; set; } = string.Empty;
    public string ReceiptFolio { get; set; } = string.Empty;
    public string ProcessedAt { get; set; } = string.Empty;
}
