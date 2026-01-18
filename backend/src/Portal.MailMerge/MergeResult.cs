namespace Portal.MailMerge;

/// <summary>
/// Represents the result of a mail merge operation.
/// </summary>
public class MergeResult
{
    /// <summary>
    /// Indicates whether the merge operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The merged document as a byte array, or null if the operation failed.
    /// </summary>
    public byte[]? Document { get; init; }

    /// <summary>
    /// Error message if the operation failed, otherwise null.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// List of field names that were found in the template.
    /// </summary>
    public IReadOnlyList<string> FieldsFound { get; init; } = Array.Empty<string>();

    /// <summary>
    /// List of field names that were successfully replaced.
    /// </summary>
    public IReadOnlyList<string> FieldsReplaced { get; init; } = Array.Empty<string>();

    /// <summary>
    /// List of field names that were found but had no matching data.
    /// </summary>
    public IReadOnlyList<string> UnmatchedFields { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Creates a successful merge result.
    /// </summary>
    public static MergeResult Successful(
        byte[] document,
        IReadOnlyList<string> fieldsFound,
        IReadOnlyList<string> fieldsReplaced,
        IReadOnlyList<string> unmatchedFields)
    {
        return new MergeResult
        {
            Success = true,
            Document = document,
            FieldsFound = fieldsFound,
            FieldsReplaced = fieldsReplaced,
            UnmatchedFields = unmatchedFields
        };
    }

    /// <summary>
    /// Creates a failed merge result.
    /// </summary>
    public static MergeResult Failed(string errorMessage)
    {
        return new MergeResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
