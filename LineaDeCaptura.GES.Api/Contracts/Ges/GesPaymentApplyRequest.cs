namespace LineaDeCaptura.GES.Api.Contracts.Ges;

public sealed class GesHouseTicket
{
    public string TicketNumber { get; set; } = string.Empty;
    public string TicketTimestamp { get; set; } = string.Empty;
}

public sealed class GesPaymentApplyRequest
{
    public string MessageType { get; set; } = "PaymentApplyRequest";
    public string TransactionGuid { get; set; } = string.Empty;
    public string RequestTimestamp { get; set; } = string.Empty;
    public string GesTraceId { get; set; } = string.Empty;
    public GesCommerce Commerce { get; set; } = new();
    public string CaptureLine { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public GesHouseTicket HouseTicket { get; set; } = new();
}
