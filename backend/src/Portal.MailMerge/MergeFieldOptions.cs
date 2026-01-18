namespace Portal.MailMerge;

/// <summary>
/// Configuration options for mail merge operations.
/// </summary>
public class MergeFieldOptions
{
    /// <summary>
    /// The prefix used to identify merge fields. Default is "{{".
    /// </summary>
    public string FieldPrefix { get; set; } = "{{";

    /// <summary>
    /// The suffix used to identify merge fields. Default is "}}".
    /// </summary>
    public string FieldSuffix { get; set; } = "}}";

    /// <summary>
    /// If true, removes merge fields that don't have matching data. Default is true.
    /// </summary>
    public bool RemoveUnmatchedFields { get; set; } = true;

    /// <summary>
    /// If true, field names are case-insensitive. Default is true.
    /// </summary>
    public bool CaseInsensitive { get; set; } = true;

    /// <summary>
    /// The text to use when replacing unmatched fields (only used if RemoveUnmatchedFields is true).
    /// Default is empty string.
    /// </summary>
    public string UnmatchedFieldReplacement { get; set; } = string.Empty;

    /// <summary>
    /// Creates the default merge field options.
    /// </summary>
    public static MergeFieldOptions Default => new();
}
