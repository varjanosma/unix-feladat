using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;
using ProductManagement.Bll.Export;
using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

internal sealed class ExportService : IExportService
{
    // Created once and reused: building the options / the serializer is expensive.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly XmlSerializer InvoiceSerializer = new(typeof(InvoiceXml));

    private readonly IProductSearchService _productSearchService;
    private readonly IPurchaseService _purchaseService;

    public ExportService(IProductSearchService productSearchService, IPurchaseService purchaseService)
    {
        _productSearchService = productSearchService;
        _purchaseService = purchaseService;
    }

    public async Task<byte[]> ExportCatalogJsonAsync(CancellationToken cancellationToken = default)
    {
        var catalog = await _productSearchService.SearchAsync(
            new ProductSearchRequest { PageSize = 100 },
            cancellationToken);

        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, catalog.Items, JsonOptions, cancellationToken);
        return stream.ToArray();
    }

    public async Task<byte[]?> GetInvoiceXmlAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        var purchase = await _purchaseService.GetByIdAsync(purchaseId, cancellationToken);
        if (purchase is null)
        {
            return null;
        }

        var invoice = new InvoiceXml
        {
            Number = $"INV-{purchase.Id:D6}",
            IssueDate = purchase.PurchasedAtUtc.Date,
            Customer = new InvoiceCustomerXml { Name = purchase.UserFullName, Email = purchase.UserEmail },
            Lines = purchase.Items
                .Select(i => new InvoiceLineXml
                {
                    Sku = i.Sku,
                    Description = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal,
                })
                .ToList(),
            Total = purchase.Total,
        };

        // Without this, the output gets xmlns:xsi / xmlns:xsd attributes nobody asked for.
        var noNamespaces = new XmlSerializerNamespaces();
        noNamespaces.Add(string.Empty, string.Empty);

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) }))
        {
            InvoiceSerializer.Serialize(writer, invoice, noNamespaces);
        }

        return stream.ToArray();
    }
}
