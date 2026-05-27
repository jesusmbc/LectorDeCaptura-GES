using System.Net.Http.Json;
using System.Text.Json;
using LineaDeCaptura.GES.Api.Contracts.Ges;
using LineaDeCaptura.GES.Api.Options;
using Microsoft.Extensions.Options;

namespace LineaDeCaptura.GES.Api.Services;

public interface IGesApiClient
{
    Task<GesDebtInquiryResponse> DebtInquiryAsync(GesDebtInquiryRequest request, CancellationToken cancellationToken);
    Task<GesPaymentApplyResponse> PaymentApplyAsync(GesPaymentApplyRequest request, CancellationToken cancellationToken);
}

public sealed class GesApiClient : IGesApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly GesApiOptions _options;

    public GesApiClient(HttpClient httpClient, IOptions<GesApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public Task<GesDebtInquiryResponse> DebtInquiryAsync(GesDebtInquiryRequest request, CancellationToken cancellationToken)
        => PostAsync<GesDebtInquiryRequest, GesDebtInquiryResponse>(_options.DebtInquiryPath, request, cancellationToken);

    public Task<GesPaymentApplyResponse> PaymentApplyAsync(GesPaymentApplyRequest request, CancellationToken cancellationToken)
        => PostAsync<GesPaymentApplyRequest, GesPaymentApplyResponse>(_options.PaymentApplyPath, request, cancellationToken);

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        message.Headers.Add("apikey", _options.ApiKey);
        message.Headers.UserAgent.ParseAdd("Requestly/1.0");

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"GES API request failed. StatusCode={(int)response.StatusCode}, Body={content}");
        }

        var parsed = JsonSerializer.Deserialize<TResponse>(content, JsonOptions);
        if (parsed == null)
        {
            throw new InvalidOperationException("GES API returned empty or invalid JSON response.");
        }

        return parsed;
    }
}
