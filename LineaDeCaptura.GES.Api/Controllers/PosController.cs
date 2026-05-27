using LineaDeCaptura.GES.Api.Contracts.Pos;
using LineaDeCaptura.GES.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LineaDeCaptura.GES.Api.Controllers;

[ApiController]
[Route("api/pos")]
public sealed class PosController : ControllerBase
{
    private readonly IPosService _service;
    private readonly IReconciliationCsvService _reconciliationCsvService;

    public PosController(IPosService service, IReconciliationCsvService reconciliationCsvService)
    {
        _service = service;
        _reconciliationCsvService = reconciliationCsvService;
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

    [HttpGet("reconciliation-csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReconciliationCsv([FromQuery] ReconciliationCsvQuery query, CancellationToken cancellationToken)
    {
        if (query.FechaInicio == default || query.FechaFin == default)
        {
            return BadRequest("fechaInicio y fechaFin son requeridas en formato yyyy-MM-dd.");
        }

        if (query.FechaInicio.Date > query.FechaFin.Date)
        {
            return BadRequest("fechaInicio no puede ser mayor que fechaFin.");
        }

        var (content, fileName) = await _reconciliationCsvService.GenerateAsync(query.FechaInicio.Date, query.FechaFin.Date, cancellationToken);
        return File(content, "text/csv", fileName);
    }
}
