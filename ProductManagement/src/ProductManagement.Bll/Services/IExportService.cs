namespace ProductManagement.Bll.Services;

public interface IExportService
{
    /// <summary>The active product catalogue as an indented, camelCase JSON document (UTF-8).</summary>
    Task<byte[]> ExportCatalogJsonAsync(CancellationToken cancellationToken = default);

    /// <summary>The invoice of a purchase as XML (UTF-8). <c>null</c> if there is no such purchase.</summary>
    Task<byte[]?> GetInvoiceXmlAsync(int purchaseId, CancellationToken cancellationToken = default);
}
