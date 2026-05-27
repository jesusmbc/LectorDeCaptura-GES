namespace LineaDeCaptura.GES.Api.Contracts.Ges;

public sealed class GesDebtInquiryRequest
{
    public string MessageType { get; set; } = "DebtInquiryRequest";
    public string TransactionGuid { get; set; } = string.Empty;
    public string RequestTimestamp { get; set; } = string.Empty;
    public string Channel { get; set; } = "POS";
    public GesCommerce Commerce { get; set; } = new();
    public string CaptureLine { get; set; } = string.Empty;
}
