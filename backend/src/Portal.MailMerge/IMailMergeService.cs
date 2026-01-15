namespace Portal.MailMerge;

/// <summary>
/// Interface for performing mail merge operations with Word documents.
/// </summary>
public interface IMailMergeService
{
    /// <summary>
    /// Performs a mail merge operation using a dictionary of field values.
    /// </summary>
    /// <param name="templatePath">Path to the Word document template.</param>
    /// <param name="mergeData">Dictionary of field names and their replacement values.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>A MergeResult containing the merged document or error information.</returns>
    MergeResult Merge(string templatePath, IDictionary<string, string> mergeData, MergeFieldOptions? options = null);

    /// <summary>
    /// Performs a mail merge operation using a dictionary of field values from a stream.
    /// </summary>
    /// <param name="templateStream">Stream containing the Word document template.</param>
    /// <param name="mergeData">Dictionary of field names and their replacement values.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>A MergeResult containing the merged document or error information.</returns>
    MergeResult Merge(Stream templateStream, IDictionary<string, string> mergeData, MergeFieldOptions? options = null);

    /// <summary>
    /// Performs a mail merge operation using a byte array template.
    /// </summary>
    /// <param name="templateBytes">Byte array containing the Word document template.</param>
    /// <param name="mergeData">Dictionary of field names and their replacement values.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>A MergeResult containing the merged document or error information.</returns>
    MergeResult Merge(byte[] templateBytes, IDictionary<string, string> mergeData, MergeFieldOptions? options = null);

    /// <summary>
    /// Performs a mail merge operation using an object's properties as merge data.
    /// </summary>
    /// <typeparam name="T">The type of the data object.</typeparam>
    /// <param name="templatePath">Path to the Word document template.</param>
    /// <param name="data">Object whose properties will be used as merge field values.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>A MergeResult containing the merged document or error information.</returns>
    MergeResult Merge<T>(string templatePath, T data, MergeFieldOptions? options = null) where T : class;

    /// <summary>
    /// Performs a mail merge operation using an object's properties as merge data from a stream.
    /// </summary>
    /// <typeparam name="T">The type of the data object.</typeparam>
    /// <param name="templateStream">Stream containing the Word document template.</param>
    /// <param name="data">Object whose properties will be used as merge field values.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>A MergeResult containing the merged document or error information.</returns>
    MergeResult Merge<T>(Stream templateStream, T data, MergeFieldOptions? options = null) where T : class;

    /// <summary>
    /// Performs a mail merge operation using an object's properties from a byte array template.
    /// </summary>
    /// <typeparam name="T">The type of the data object.</typeparam>
    /// <param name="templateBytes">Byte array containing the Word document template.</param>
    /// <param name="data">Object whose properties will be used as merge field values.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>A MergeResult containing the merged document or error information.</returns>
    MergeResult Merge<T>(byte[] templateBytes, T data, MergeFieldOptions? options = null) where T : class;

    /// <summary>
    /// Extracts all merge field names from a Word document template.
    /// </summary>
    /// <param name="templatePath">Path to the Word document template.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>List of field names found in the template.</returns>
    IReadOnlyList<string> GetMergeFields(string templatePath, MergeFieldOptions? options = null);

    /// <summary>
    /// Extracts all merge field names from a Word document template stream.
    /// </summary>
    /// <param name="templateStream">Stream containing the Word document template.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>List of field names found in the template.</returns>
    IReadOnlyList<string> GetMergeFields(Stream templateStream, MergeFieldOptions? options = null);

    /// <summary>
    /// Extracts all merge field names from a Word document template byte array.
    /// </summary>
    /// <param name="templateBytes">Byte array containing the Word document template.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>List of field names found in the template.</returns>
    IReadOnlyList<string> GetMergeFields(byte[] templateBytes, MergeFieldOptions? options = null);

    /// <summary>
    /// Performs mail merge and saves the result directly to a file.
    /// </summary>
    /// <param name="templatePath">Path to the Word document template.</param>
    /// <param name="outputPath">Path where the merged document will be saved.</param>
    /// <param name="mergeData">Dictionary of field names and their replacement values.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>A MergeResult containing operation status (Document property will be null).</returns>
    MergeResult MergeToFile(string templatePath, string outputPath, IDictionary<string, string> mergeData, MergeFieldOptions? options = null);

    /// <summary>
    /// Performs mail merge using an object and saves the result directly to a file.
    /// </summary>
    /// <typeparam name="T">The type of the data object.</typeparam>
    /// <param name="templatePath">Path to the Word document template.</param>
    /// <param name="outputPath">Path where the merged document will be saved.</param>
    /// <param name="data">Object whose properties will be used as merge field values.</param>
    /// <param name="options">Optional merge field configuration options.</param>
    /// <returns>A MergeResult containing operation status (Document property will be null).</returns>
    MergeResult MergeToFile<T>(string templatePath, string outputPath, T data, MergeFieldOptions? options = null) where T : class;
}
