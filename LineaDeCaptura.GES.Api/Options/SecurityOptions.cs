namespace LineaDeCaptura.GES.Api.Options;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";
    public string ApiKeyHeaderName { get; set; } = "apikey";
}
