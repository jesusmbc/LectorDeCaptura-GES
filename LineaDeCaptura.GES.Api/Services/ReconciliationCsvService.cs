using System.Globalization;
using System.Text;
using LineaDeCaptura.GES.Api.Data;
using LineaDeCaptura.GES.Api.Domain;

namespace LineaDeCaptura.GES.Api.Services;

public interface IReconciliationCsvService
{
    Task<(byte[] Content, string FileName)> GenerateAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken);
}

public sealed class ReconciliationCsvService : IReconciliationCsvService
{
    private readonly IGesRepository _repository;

    public ReconciliationCsvService(IGesRepository repository)
    {
        _repository = repository;
    }

    public async Task<(byte[] Content, string FileName)> GenerateAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
    {
        var rows = await _repository.GetReconciliationRowsAsync(fechaInicio, fechaFin, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("recordType,transactionGuid,gesTraceId,storeId,cashRegister,cashierId,paymentMethod,ticketNumber,ticketTimestamp,captureLineOriginalPos,captureLineGesResponse,amountPos,currencyPos,amountGes,currencyGes,statusFinalInterno,statusCodeFinal,statusMessageFinal,authorization,receiptFolio,inquiryAt,applyAt,createdAt,updatedAt,isIdempotent,errorFlag,errorMessage,amountMatch,captureLineMatch,hasInquiry,hasApply,inquiryWithoutApply,applyWithoutInquiry,finalResult");

        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(',',
                "DETAIL",
                Escape(row.TransactionGuid.ToString()),
                Escape(row.GesTraceId),
                Escape(row.StoreId),
                Escape(row.CashRegister),
                Escape(row.CashierId),
                Escape(row.PaymentMethod),
                Escape(row.TicketNumber),
                Escape(FormatDate(row.TicketTimestamp)),
                Escape(row.CaptureLineOriginalPos),
                Escape(row.CaptureLineGesResponse),
                Escape(FormatDecimal(row.AmountPos)),
                Escape(row.CurrencyPos),
                Escape(FormatDecimal(row.AmountGes)),
                Escape(row.CurrencyGes),
                Escape(row.StatusFinalInterno),
                Escape(row.StatusCodeFinal),
                Escape(row.StatusMessageFinal),
                Escape(row.Authorization),
                Escape(row.ReceiptFolio),
                Escape(FormatDate(row.InquiryAt)),
                Escape(FormatDate(row.ApplyAt)),
                Escape(FormatDate(row.CreatedAt)),
                Escape(FormatDate(row.UpdatedAt)),
                Escape(ToBit(row.IsIdempotent)),
                Escape(ToBit(row.ErrorFlag)),
                Escape(row.ErrorMessage),
                Escape(ToBit(row.AmountMatch)),
                Escape(ToBit(row.CaptureLineMatch)),
                Escape(ToBit(row.HasInquiry)),
                Escape(ToBit(row.HasApply)),
                Escape(ToBit(row.InquiryWithoutApply)),
                Escape(ToBit(row.ApplyWithoutInquiry)),
                Escape(row.FinalResult)));
        }

        var totalOps = rows.Count;
        var totalAmountPos = rows.Sum(x => x.AmountPos ?? 0m);
        var totalAmountGes = rows.Sum(x => x.AmountGes ?? 0m);
        var appliedCount = rows.Count(x => x.FinalResult == "APPLIED");
        var rejectedCount = rows.Count(x => x.FinalResult == "REJECTED");
        var alreadyPaidCount = rows.Count(x => x.FinalResult == "ALREADY_PAID");
        var errorCount = rows.Count(x => x.FinalResult == "ERROR");

        csv.AppendLine(string.Join(',',
            "SUMMARY",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            Escape($"totalOperations={totalOps}"),
            string.Empty,
            string.Empty,
            string.Empty,
            Escape(FormatDecimal(totalAmountPos)),
            string.Empty,
            Escape(FormatDecimal(totalAmountGes)),
            string.Empty,
            Escape($"applied={appliedCount}"),
            Escape($"rejected={rejectedCount}"),
            Escape($"alreadyPaid={alreadyPaidCount}"),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            Escape($"errors={errorCount}"),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty));

        var fileName = $"conciliacion_ges_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.csv";
        return (Encoding.UTF8.GetBytes(csv.ToString()), fileName);
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static string FormatDecimal(decimal? value)
    {
        return value.HasValue ? value.Value.ToString("0.00", CultureInfo.InvariantCulture) : string.Empty;
    }

    private static string FormatDate(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
    }

    private static string ToBit(bool value)
    {
        return value ? "1" : "0";
    }
}
