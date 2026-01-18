using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Portal.MailMerge.Tests;

/// <summary>
/// Helper class for creating test Word documents.
/// </summary>
public static class TestDocumentHelper
{
    /// <summary>
    /// Creates a Word document with the specified content.
    /// </summary>
    public static byte[] CreateDocument(params string[] paragraphs)
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            foreach (var text in paragraphs)
            {
                var paragraph = new Paragraph(
                    new Run(
                        new Text(text)));
                body.Append(paragraph);
            }

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Creates a Word document with a paragraph where text is split across multiple runs.
    /// This simulates what happens when Word applies different formatting.
    /// </summary>
    public static byte[] CreateDocumentWithSplitRuns(string[] textParts)
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            var paragraph = new Paragraph();
            foreach (var part in textParts)
            {
                paragraph.Append(new Run(new Text(part) { Space = SpaceProcessingModeValues.Preserve }));
            }
            body.Append(paragraph);

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Creates a Word document with header and footer.
    /// </summary>
    public static byte[] CreateDocumentWithHeaderFooter(string bodyText, string headerText, string footerText)
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            // Add body content
            body.Append(new Paragraph(new Run(new Text(bodyText))));

            // Add header
            var headerPart = mainPart.AddNewPart<HeaderPart>();
            headerPart.Header = new Header(
                new Paragraph(new Run(new Text(headerText))));
            headerPart.Header.Save();

            // Add footer
            var footerPart = mainPart.AddNewPart<FooterPart>();
            footerPart.Footer = new Footer(
                new Paragraph(new Run(new Text(footerText))));
            footerPart.Footer.Save();

            // Create section properties to link header/footer
            var sectionProps = new SectionProperties();
            sectionProps.Append(new HeaderReference
            {
                Type = HeaderFooterValues.Default,
                Id = mainPart.GetIdOfPart(headerPart)
            });
            sectionProps.Append(new FooterReference
            {
                Type = HeaderFooterValues.Default,
                Id = mainPart.GetIdOfPart(footerPart)
            });
            body.Append(sectionProps);

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Reads the text content from a Word document.
    /// </summary>
    public static string ReadDocumentText(byte[] documentBytes)
    {
        using var stream = new MemoryStream(documentBytes);
        using var document = WordprocessingDocument.Open(stream, false);

        if (document.MainDocumentPart?.Document?.Body == null)
            return string.Empty;

        var texts = document.MainDocumentPart.Document.Body
            .Descendants<Text>()
            .Select(t => t.Text);

        return string.Join("", texts);
    }

    /// <summary>
    /// Reads text from all parts of a document including headers and footers.
    /// </summary>
    public static (string body, string header, string footer) ReadAllDocumentText(byte[] documentBytes)
    {
        using var stream = new MemoryStream(documentBytes);
        using var document = WordprocessingDocument.Open(stream, false);

        var bodyText = string.Empty;
        var headerText = string.Empty;
        var footerText = string.Empty;

        if (document.MainDocumentPart?.Document?.Body != null)
        {
            bodyText = string.Join("", document.MainDocumentPart.Document.Body
                .Descendants<Text>()
                .Select(t => t.Text));
        }

        foreach (var headerPart in document.MainDocumentPart?.HeaderParts ?? Enumerable.Empty<HeaderPart>())
        {
            headerText += string.Join("", headerPart.Header
                .Descendants<Text>()
                .Select(t => t.Text));
        }

        foreach (var footerPart in document.MainDocumentPart?.FooterParts ?? Enumerable.Empty<FooterPart>())
        {
            footerText += string.Join("", footerPart.Footer
                .Descendants<Text>()
                .Select(t => t.Text));
        }

        return (bodyText, headerText, footerText);
    }
}
