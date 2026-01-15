namespace ESignedPdf.Models;

/// <summary>
/// Configuration for the visual appearance of a digital signature on a PDF document.
/// </summary>
public class SignatureAppearance
{
    /// <summary>
    /// The page number where the signature should appear (1-based index).
    /// Default is 1 (first page).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// X coordinate (from left) of the signature rectangle in points (1 point = 1/72 inch).
    /// </summary>
    public float X { get; set; } = 50;

    /// <summary>
    /// Y coordinate (from bottom) of the signature rectangle in points.
    /// </summary>
    public float Y { get; set; } = 50;

    /// <summary>
    /// Width of the signature rectangle in points.
    /// </summary>
    public float Width { get; set; } = 200;

    /// <summary>
    /// Height of the signature rectangle in points.
    /// </summary>
    public float Height { get; set; } = 100;

    /// <summary>
    /// Optional path to an image file (PNG, JPEG) to display as the signature graphic.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Optional image data as byte array to display as the signature graphic.
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// Whether to show the signer's name in the signature appearance.
    /// </summary>
    public bool ShowSignerName { get; set; } = true;

    /// <summary>
    /// Whether to show the signature date in the signature appearance.
    /// </summary>
    public bool ShowDate { get; set; } = true;

    /// <summary>
    /// Whether to show the reason for signing in the signature appearance.
    /// </summary>
    public bool ShowReason { get; set; } = true;

    /// <summary>
    /// Whether to show the location in the signature appearance.
    /// </summary>
    public bool ShowLocation { get; set; } = true;

    /// <summary>
    /// Custom text to display in the signature appearance.
    /// If null, default text based on certificate information will be used.
    /// </summary>
    public string? CustomText { get; set; }

    /// <summary>
    /// Font size for the signature text in points.
    /// </summary>
    public float FontSize { get; set; } = 10;

    /// <summary>
    /// Whether to create an invisible signature (no visual appearance).
    /// Useful when the signature should be verifiable but not visible on the document.
    /// </summary>
    public bool Invisible { get; set; } = false;

    /// <summary>
    /// Creates a default visible signature appearance.
    /// </summary>
    public static SignatureAppearance Default => new();

    /// <summary>
    /// Creates an invisible signature (no visual representation).
    /// </summary>
    public static SignatureAppearance InvisibleSignature => new() { Invisible = true };

    /// <summary>
    /// Creates a signature appearance at the bottom-right corner of the specified page.
    /// </summary>
    public static SignatureAppearance BottomRight(int pageNumber = 1) => new()
    {
        PageNumber = pageNumber,
        X = 400,
        Y = 50,
        Width = 150,
        Height = 75
    };

    /// <summary>
    /// Creates a signature appearance at the bottom-left corner of the specified page.
    /// </summary>
    public static SignatureAppearance BottomLeft(int pageNumber = 1) => new()
    {
        PageNumber = pageNumber,
        X = 50,
        Y = 50,
        Width = 150,
        Height = 75
    };
}
