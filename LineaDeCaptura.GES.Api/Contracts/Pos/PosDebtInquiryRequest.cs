using System.ComponentModel.DataAnnotations;

namespace LineaDeCaptura.GES.Api.Contracts.Pos;

public sealed class PosDebtInquiryRequest
{
    [Required]
    public string CaptureLine { get; set; } = string.Empty;

    [Required]
    public string StoreId { get; set; } = string.Empty;

    [Required]
    public string CashRegister { get; set; } = string.Empty;

    [Required]
    public string CashierId { get; set; } = string.Empty;
}
