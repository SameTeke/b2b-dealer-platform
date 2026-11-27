#:property PublishAot=false

using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var openingQuote = new MaterialQuote(UsdPerGram: 8m, EurPerUsd: 0.90m);
var laterQuote = new MaterialQuote(UsdPerGram: 9m, EurPerUsd: 0.95m);
var order = OrderLine.Capture(openingQuote, gramsPerUnit: 5m, quantity: 3);
var nextOrder = OrderLine.Capture(laterQuote, gramsPerUnit: 5m, quantity: 3);

if (args is ["--check"])
{
    Check(order.TotalEur == 108m, "first order keeps its checkout amount");
    Check(nextOrder.TotalEur == 128.25m, "a new order uses the later quote");
    Check(order.ExportUnitUsd == 40m, "first export keeps its USD unit price");
    Check(nextOrder.ExportUnitUsd == 45m, "new export uses its own USD unit price");
    var fractional = OrderLine.Capture(new(1.005m, 1m), 1m, 3);
    Check(fractional.TotalEur == 3.02m, "round only after multiplying the line");
    Check(Rejects(() => OrderLine.Capture(new(8m, 0m), 5m, 3)), "reject a zero rate");
    Check(Rejects(() => OrderLine.Capture(openingQuote, 5m, 0)), "reject an empty line");
    return 0;
}

if (args.Length > 0)
{
    Console.Error.WriteLine("Run with no arguments or with --check.");
    return 2;
}

Console.WriteLine($"First checkout          EUR {order.TotalEur:F2}");
Console.WriteLine($"After the quote update  EUR {nextOrder.TotalEur:F2} (new order)");
Console.WriteLine($"First order ERP export  USD {order.ExportUnitUsd:F2} per unit");
return 0;

static void Check(bool passed, string description)
{
    if (!passed)
        throw new InvalidOperationException(description);
    Console.WriteLine($"PASS  {description}");
}

static bool Rejects(Action capture)
{
    try { capture(); }
    catch (ArgumentOutOfRangeException) { return true; }
    return false;
}

internal sealed record MaterialQuote(decimal UsdPerGram, decimal EurPerUsd);

internal sealed record OrderLine
{
    public decimal ExportUnitUsd { get; }
    public decimal TotalEur { get; }

    private OrderLine(decimal exportUnitUsd, decimal totalEur)
    {
        ExportUnitUsd = exportUnitUsd;
        TotalEur = totalEur;
    }

    public static OrderLine Capture(MaterialQuote quote, decimal gramsPerUnit, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quote.UsdPerGram);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quote.EurPerUsd);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gramsPerUnit);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        decimal unitUsd = gramsPerUnit * quote.UsdPerGram;
        decimal lineEur = decimal.Round(unitUsd * quantity * quote.EurPerUsd, 2,
            MidpointRounding.AwayFromZero);
        return new OrderLine(unitUsd, lineEur);
    }
}
