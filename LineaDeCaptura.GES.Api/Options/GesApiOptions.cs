namespace LineaDeCaptura.GES.Api.Options;

public sealed class GesApiOptions
{
    public const string SectionName = "GesApi";
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public string DebtInquiryPath { get; set; } = "/api/CasaLey/DebtInquiryRequest";
    public string PaymentApplyPath { get; set; } = "/api/CasaLey/PaymentApplyRequest";
}
