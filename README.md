# B2B dealer purchasing platform

Dealers could buy directly from a catalogue whose prices changed with raw material costs. I worked on the platform that connected their orders to the company's existing software. One engineering question ran through the project: if a price changes after checkout, which amount should the ERP receive?

## From catalogue to ERP

1. **Catalogue and dealer ordering.** A React and TypeScript storefront gives dealers a catalogue and cart. A separate React and TypeScript administration panel handles catalogue publication and lets staff review dealer carts.
2. **Pricing at checkout.** The C#/.NET backend calculates order totals and captures price information on each order line. The dealer's order currency and the unit price needed for ERP export serve different purposes.
3. **ERP export.** The export reads the unit price captured at checkout. A later change to the catalogue price should affect future purchases. Recalculating the first order from today's catalogue would change the commercial record.
4. **Reporting.** The backend groups nominal amounts by currency and rounds each subtotal after summing. USD 120 and EUR 80 appear as two subtotals. Combining them requires an explicit conversion rate and a reporting currency.

## Run the pricing example

[OrderPricing.cs](./OrderPricing.cs) is a small C# program written for this portfolio. It uses illustrative material prices and exchange rates to reproduce the order-snapshot decision. The example runs locally with the .NET 10 SDK.

```sh
dotnet run --file OrderPricing.cs
dotnet run --file OrderPricing.cs -- --check
```

A dealer buys three units of a five-gram item. At USD 8 per gram and EUR 0.90 per dollar, checkout records **EUR 108.00**. Later the inputs change to USD 9 per gram and EUR 0.95 per dollar. The same basket now costs **EUR 128.25**. Exporting the first order still yields its captured **USD 40.00 per unit**.

The checks exercise both orders, the two export values, rounding after multiplication, a zero exchange rate and an empty order line. The demonstration uses one line and one currency pair. The original application also has separate tests for checkout's ERP export snapshot, pricing rules and commercial currencies.

The company application, its catalogue and its integration settings are private. All amounts in the example are synthetic.
