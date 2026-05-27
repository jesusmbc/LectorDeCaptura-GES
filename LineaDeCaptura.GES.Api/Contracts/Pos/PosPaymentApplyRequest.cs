using System.ComponentModel.DataAnnotations;

namespace LineaDeCaptura.GES.Api.Contracts.Pos;

public sealed class PosPaymentApplyRequest
{
    [Required]
    public string TransactionGuid { get; set; } = string.Empty;

    [Required]
    public string GesTraceId { get; set; } = string.Empty;

    [Required]
    public string CaptureLine { get; set; } = string.Empty;

    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    public string StoreId { get; set; } = string.Empty;

    [Required]
    public string CashRegister { get; set; } = string.Empty;

    [Required]
    public string CashierId { get; set; } = string.Empty;

    [Required]
    public string TicketNumber { get; set; } = string.Empty;

    public DateTimeOffset TicketTimestamp { get; set; }
}
