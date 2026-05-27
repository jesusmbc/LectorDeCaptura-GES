namespace LineaDeCaptura.GES.Api.Domain;

public sealed class TransactionRecord
{
    public Guid TransactionGuid { get; set; }
    public string CaptureLine { get; set; } = string.Empty;
    public string GesTraceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public string CashRegister { get; set; } = string.Empty;
    public string CashierId { get; set; } = string.Empty;
    public decimal? RequestedAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}
