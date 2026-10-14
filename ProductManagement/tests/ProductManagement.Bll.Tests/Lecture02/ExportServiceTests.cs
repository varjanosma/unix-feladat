using System.Text.Json;
using System.Xml.Linq;
using ProductManagement.Bll.Services;
using Xunit;

namespace ProductManagement.Bll.Tests.Lecture02;

public class ExportServiceTests
{
    private static ExportService CreateService(Dal.ProductManagementDbContext db)
        => new(new ProductSearchService(db), new PurchaseService(db));

    [Fact]
    public async Task Catalog_is_camel_case_json_with_active_products_only()
    {
        using var db = TestDb.Create();

        var json = await CreateService(db).ExportCatalogJsonAsync();

        using var document = JsonDocument.Parse(json);
        Assert.Equal(11, document.RootElement.GetArrayLength());
        Assert.True(document.RootElement[0].TryGetProperty("sku", out _));
    }

    [Fact]
    public async Task Invoice_xml_has_the_lines_and_the_total()
    {
        using var db = TestDb.Create();

        var xml = await CreateService(db).GetInvoiceXmlAsync(purchaseId: 1);

        var invoice = XDocument.Load(new MemoryStream(xml!)).Root!;
        Assert.Equal("INV-000001", invoice.Attribute("number")!.Value);
        Assert.Equal(2, invoice.Element("Lines")!.Elements("Line").Count());
        Assert.Equal(448.90m, (decimal)invoice.Element("Total")!);
    }

    [Fact]
    public async Task Invoice_of_an_unknown_purchase_is_null()
    {
        using var db = TestDb.Create();

        Assert.Null(await CreateService(db).GetInvoiceXmlAsync(purchaseId: 999));
    }
}
