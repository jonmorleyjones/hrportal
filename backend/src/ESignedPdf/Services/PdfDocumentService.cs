using ESignedPdf.Exceptions;
using ESignedPdf.Interfaces;
using ESignedPdf.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Geom;

namespace ESignedPdf.Services;

/// <summary>
/// Implementation of PDF document creation and manipulation service.
/// </summary>
public class PdfDocumentService : IPdfDocumentService
{
    /// <inheritdoc />
    public byte[] CreateDocument(IEnumerable<PdfContent> contents, (float Width, float Height)? pageSize = null)
    {
        try
        {
            var size = pageSize ?? PageSize.Letter;
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdfDoc = new PdfDocument(writer);

            var iTextPageSize = new iText.Kernel.Geom.PageSize(size.Width, size.Height);
            pdfDoc.SetDefaultPageSize(iTextPageSize);

            using var document = new Document(pdfDoc);

            // Group content by page
            var contentsByPage = contents.GroupBy(c => c.PageNumber).OrderBy(g => g.Key);

            int currentPage = 0;
            foreach (var pageGroup in contentsByPage)
            {
                // Add pages if needed
                while (currentPage < pageGroup.Key)
                {
                    if (currentPage > 0)
                    {
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    }
                    currentPage++;
                }

                foreach (var content in pageGroup)
                {
                    AddContentToDocument(document, pdfDoc, content);
                }
            }

            document.Close();
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            throw new PdfDocumentException("Failed to create PDF document", ex);
        }
    }

    /// <inheritdoc />
    public byte[] CreateSimpleDocument(string text, string? title = null)
    {
        var contents = new List<PdfContent>();

        if (!string.IsNullOrEmpty(title))
        {
            contents.Add(new TextContent
            {
                Text = title,
                X = 72,
                Y = 750,
                FontSize = 18,
                Bold = true
            });
        }

        contents.Add(new ParagraphContent
        {
            Text = text,
            X = 72,
            Y = string.IsNullOrEmpty(title) ? 750 : 720,
            MaxWidth = 468,
            FontSize = 12
        });

        return CreateDocument(contents);
    }

    /// <inheritdoc />
    public byte[] AddPage(byte[] existingPdf, IEnumerable<PdfContent> contents)
    {
        try
        {
            using var inputStream = new MemoryStream(existingPdf);
            using var outputStream = new MemoryStream();
            using var reader = new PdfReader(inputStream);
            using var writer = new PdfWriter(outputStream);
            using var pdfDoc = new PdfDocument(reader, writer);
            using var document = new Document(pdfDoc);

            // Add a new page
            pdfDoc.AddNewPage();
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            foreach (var content in contents)
            {
                AddContentToDocument(document, pdfDoc, content);
            }

            document.Close();
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            throw new PdfDocumentException("Failed to add page to PDF document", ex);
        }
    }

    /// <inheritdoc />
    public byte[] MergeDocuments(IEnumerable<byte[]> documents)
    {
        try
        {
            using var outputStream = new MemoryStream();
            using var writer = new PdfWriter(outputStream);
            using var pdfDoc = new PdfDocument(writer);

            foreach (var docBytes in documents)
            {
                using var inputStream = new MemoryStream(docBytes);
                using var reader = new PdfReader(inputStream);
                using var sourceDoc = new PdfDocument(reader);

                sourceDoc.CopyPagesTo(1, sourceDoc.GetNumberOfPages(), pdfDoc);
            }

            pdfDoc.Close();
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            throw new PdfDocumentException("Failed to merge PDF documents", ex);
        }
    }

    /// <inheritdoc />
    public int GetPageCount(byte[] pdfDocument)
    {
        try
        {
            using var stream = new MemoryStream(pdfDocument);
            using var reader = new PdfReader(stream);
            using var pdfDoc = new PdfDocument(reader);

            return pdfDoc.GetNumberOfPages();
        }
        catch (Exception ex)
        {
            throw new PdfDocumentException("Failed to get page count", ex);
        }
    }

    /// <inheritdoc />
    public string ExtractText(byte[] pdfDocument)
    {
        try
        {
            using var stream = new MemoryStream(pdfDocument);
            using var reader = new PdfReader(stream);
            using var pdfDoc = new PdfDocument(reader);

            var text = new System.Text.StringBuilder();
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var page = pdfDoc.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                text.AppendLine(pageText);
            }

            return text.ToString();
        }
        catch (Exception ex)
        {
            throw new PdfDocumentException("Failed to extract text from PDF", ex);
        }
    }

    private void AddContentToDocument(Document document, PdfDocument pdfDoc, PdfContent content)
    {
        switch (content)
        {
            case TextContent textContent:
                AddTextContent(document, pdfDoc, textContent);
                break;
            case ParagraphContent paragraphContent:
                AddParagraphContent(document, paragraphContent);
                break;
            case ImageContent imageContent:
                AddImageContent(document, pdfDoc, imageContent);
                break;
            case TableContent tableContent:
                AddTableContent(document, tableContent);
                break;
        }
    }

    private void AddTextContent(Document document, PdfDocument pdfDoc, TextContent content)
    {
        var font = GetFont(content.Bold, content.Italic);
        var color = new DeviceRgb(content.Color.R, content.Color.G, content.Color.B);

        var text = new Text(content.Text)
            .SetFont(font)
            .SetFontSize(content.FontSize)
            .SetFontColor(color);

        var paragraph = new Paragraph(text)
            .SetFixedPosition(content.X, content.Y, 500);

        document.Add(paragraph);
    }

    private void AddParagraphContent(Document document, ParagraphContent content)
    {
        var paragraph = new Paragraph(content.Text)
            .SetFontSize(content.FontSize)
            .SetFixedPosition(content.X, content.Y - (content.FontSize * content.LineHeight * 10), content.MaxWidth)
            .SetMultipliedLeading(content.LineHeight);

        paragraph.SetTextAlignment(content.Alignment switch
        {
            Models.TextAlignment.Center => iText.Layout.Properties.TextAlignment.CENTER,
            Models.TextAlignment.Right => iText.Layout.Properties.TextAlignment.RIGHT,
            Models.TextAlignment.Justify => iText.Layout.Properties.TextAlignment.JUSTIFIED,
            _ => iText.Layout.Properties.TextAlignment.LEFT
        });

        document.Add(paragraph);
    }

    private void AddImageContent(Document document, PdfDocument pdfDoc, ImageContent content)
    {
        ImageData? imageData = null;

        if (content.ImageData != null)
        {
            imageData = ImageDataFactory.Create(content.ImageData);
        }
        else if (!string.IsNullOrEmpty(content.ImagePath))
        {
            imageData = ImageDataFactory.Create(content.ImagePath);
        }

        if (imageData == null)
        {
            throw new PdfDocumentException("Image content must have either ImageData or ImagePath set");
        }

        var image = new Image(imageData);

        if (content.Width > 0)
        {
            image.SetWidth(content.Width);
        }
        if (content.Height > 0)
        {
            image.SetHeight(content.Height);
        }

        image.SetFixedPosition(content.X, content.Y);
        document.Add(image);
    }

    private void AddTableContent(Document document, TableContent content)
    {
        int columnCount = content.Rows.FirstOrDefault()?.Count ?? 0;
        if (columnCount == 0) return;

        float[] columnWidths;
        if (content.ColumnWidths != null && content.ColumnWidths.Length == columnCount)
        {
            columnWidths = content.ColumnWidths.Select(w => w * content.Width).ToArray();
        }
        else
        {
            float colWidth = content.Width / columnCount;
            columnWidths = Enumerable.Repeat(colWidth, columnCount).ToArray();
        }

        var table = new Table(UnitValue.CreatePointArray(columnWidths))
            .SetFixedPosition(content.X, content.Y - (content.Rows.Count * content.FontSize * 2), content.Width);

        bool isFirstRow = true;
        foreach (var row in content.Rows)
        {
            foreach (var cellText in row)
            {
                var cell = new Cell().Add(new Paragraph(cellText).SetFontSize(content.FontSize));

                if (isFirstRow && content.HasHeader)
                {
                    cell.SetBold()
                        .SetBackgroundColor(new DeviceRgb(240, 240, 240));
                }

                table.AddCell(cell);
            }
            isFirstRow = false;
        }

        document.Add(table);
    }

    private PdfFont GetFont(bool bold, bool italic)
    {
        string fontName;
        if (bold && italic)
            fontName = StandardFonts.HELVETICA_BOLDOBLIQUE;
        else if (bold)
            fontName = StandardFonts.HELVETICA_BOLD;
        else if (italic)
            fontName = StandardFonts.HELVETICA_OBLIQUE;
        else
            fontName = StandardFonts.HELVETICA;

        return PdfFontFactory.CreateFont(fontName);
    }
}
