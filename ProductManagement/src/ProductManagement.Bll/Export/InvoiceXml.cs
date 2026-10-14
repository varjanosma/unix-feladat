using System.Xml.Serialization;

namespace ProductManagement.Bll.Export;

// XmlSerializer needs public classes with a parameterless constructor and settable properties,
// so these are plain classes, not records. The shape is what the invoicing system expects.

[XmlRoot("Invoice")]
public sealed class InvoiceXml
{
    [XmlAttribute("number")]
    public string Number { get; set; } = string.Empty;

    [XmlAttribute("currency")]
    public string Currency { get; set; } = "EUR";

    [XmlElement("IssueDate", DataType = "date")]
    public DateTime IssueDate { get; set; }

    public InvoiceCustomerXml Customer { get; set; } = new();

    [XmlArray("Lines")]
    [XmlArrayItem("Line")]
    public List<InvoiceLineXml> Lines { get; set; } = new();

    public decimal Total { get; set; }
}

public sealed class InvoiceCustomerXml
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

public sealed class InvoiceLineXml
{
    [XmlAttribute("sku")]
    public string Sku { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}
