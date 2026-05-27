namespace LineaDeCaptura.GES.Api.Contracts.Ges;

public sealed class GesOfficialReceipt
{
    public string ReceiptFolio { get; set; } = string.Empty;
}

public sealed class GesPaymentApplyResponse
{
    public string MessageType { get; set; } = string.Empty;
    public string TransactionGuid { get; set; } = string.Empty;
    public string GesTraceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = string.Empty;
    public string GesProcessedAt { get; set; } = string.Empty;
    public string Authorization { get; set; } = string.Empty;
    public GesOfficialReceipt OfficialReceipt { get; set; } = new();
}
