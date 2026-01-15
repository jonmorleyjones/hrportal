namespace ESignedPdf.Models;

/// <summary>
/// Represents content to be added to a PDF document.
/// </summary>
public abstract class PdfContent
{
    /// <summary>
    /// The page number to add this content to (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;
}

/// <summary>
/// Represents text content to add to a PDF.
/// </summary>
public class TextContent : PdfContent
{
    /// <summary>
    /// The text to add.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// X coordinate from left edge in points.
    /// </summary>
    public float X { get; set; } = 72; // 1 inch

    /// <summary>
    /// Y coordinate from bottom edge in points.
    /// </summary>
    public float Y { get; set; } = 720; // Near top of letter-size page

    /// <summary>
    /// Font size in points.
    /// </summary>
    public float FontSize { get; set; } = 12;

    /// <summary>
    /// Whether to use bold font.
    /// </summary>
    public bool Bold { get; set; } = false;

    /// <summary>
    /// Whether to use italic font.
    /// </summary>
    public bool Italic { get; set; } = false;

    /// <summary>
    /// Text color as RGB values (0-255).
    /// </summary>
    public (byte R, byte G, byte B) Color { get; set; } = (0, 0, 0);
}

/// <summary>
/// Represents paragraph content with automatic text wrapping.
/// </summary>
public class ParagraphContent : PdfContent
{
    /// <summary>
    /// The text content of the paragraph.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// X coordinate from left edge in points.
    /// </summary>
    public float X { get; set; } = 72;

    /// <summary>
    /// Y coordinate from bottom edge in points.
    /// </summary>
    public float Y { get; set; } = 720;

    /// <summary>
    /// Maximum width for text wrapping in points.
    /// </summary>
    public float MaxWidth { get; set; } = 468; // 6.5 inches

    /// <summary>
    /// Font size in points.
    /// </summary>
    public float FontSize { get; set; } = 12;

    /// <summary>
    /// Line height multiplier.
    /// </summary>
    public float LineHeight { get; set; } = 1.5f;

    /// <summary>
    /// Text alignment.
    /// </summary>
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;
}

/// <summary>
/// Text alignment options.
/// </summary>
public enum TextAlignment
{
    Left,
    Center,
    Right,
    Justify
}

/// <summary>
/// Represents an image to add to a PDF.
/// </summary>
public class ImageContent : PdfContent
{
    /// <summary>
    /// Path to the image file.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Image data as byte array.
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// X coordinate from left edge in points.
    /// </summary>
    public float X { get; set; } = 72;

    /// <summary>
    /// Y coordinate from bottom edge in points.
    /// </summary>
    public float Y { get; set; } = 500;

    /// <summary>
    /// Width of the image in points. If 0, uses original width.
    /// </summary>
    public float Width { get; set; } = 0;

    /// <summary>
    /// Height of the image in points. If 0, maintains aspect ratio based on width.
    /// </summary>
    public float Height { get; set; } = 0;
}

/// <summary>
/// Represents a table to add to a PDF.
/// </summary>
public class TableContent : PdfContent
{
    /// <summary>
    /// Table data as rows of cells.
    /// </summary>
    public required List<List<string>> Rows { get; set; }

    /// <summary>
    /// Whether the first row is a header row.
    /// </summary>
    public bool HasHeader { get; set; } = true;

    /// <summary>
    /// X coordinate from left edge in points.
    /// </summary>
    public float X { get; set; } = 72;

    /// <summary>
    /// Y coordinate from bottom edge in points.
    /// </summary>
    public float Y { get; set; } = 700;

    /// <summary>
    /// Total width of the table in points.
    /// </summary>
    public float Width { get; set; } = 468;

    /// <summary>
    /// Column widths as proportions (should sum to 1.0).
    /// If null, columns are equally sized.
    /// </summary>
    public float[]? ColumnWidths { get; set; }

    /// <summary>
    /// Font size in points.
    /// </summary>
    public float FontSize { get; set; } = 10;
}

/// <summary>
/// Page size presets.
/// </summary>
public static class PageSize
{
    public static readonly (float Width, float Height) Letter = (612, 792);
    public static readonly (float Width, float Height) A4 = (595.28f, 841.89f);
    public static readonly (float Width, float Height) Legal = (612, 1008);
    public static readonly (float Width, float Height) A3 = (841.89f, 1190.55f);
}
