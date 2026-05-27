using LineaDeCaptura.GES.Api.Contracts.Pos;
using LineaDeCaptura.GES.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LineaDeCaptura.GES.Api.Controllers;

[ApiController]
[Route("api/pos")]
public sealed class PosController : ControllerBase
{
    private readonly IPosService _service;

    public PosController(IPosService service)
    {
        _service = service;
    }

    [HttpPost("debt-inquiry")]
    [ProducesResponseType(typeof(PosDebtInquiryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DebtInquiry([FromBody] PosDebtInquiryRequest request, CancellationToken cancellationToken)
    {
        var response = await _service.DebtInquiryAsync(
            request,
            Request.Path,
            Request.Method,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("payment-apply")]
    [ProducesResponseType(typeof(PosPaymentApplyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PaymentApply([FromBody] PosPaymentApplyRequest request, CancellationToken cancellationToken)
    {
        var response = await _service.PaymentApplyAsync(
            request,
            Request.Path,
            Request.Method,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            cancellationToken);

        return Ok(response);
    }
}
