using System.Reflection;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Portal.MailMerge;

/// <summary>
/// Service for performing mail merge operations with Word documents.
/// </summary>
public class MailMergeService : IMailMergeService
{
    /// <inheritdoc />
    public MergeResult Merge(string templatePath, IDictionary<string, string> mergeData, MergeFieldOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(templatePath))
            return MergeResult.Failed("Template path cannot be null or empty.");

        if (!File.Exists(templatePath))
            return MergeResult.Failed($"Template file not found: {templatePath}");

        try
        {
            var templateBytes = File.ReadAllBytes(templatePath);
            return Merge(templateBytes, mergeData, options);
        }
        catch (Exception ex)
        {
            return MergeResult.Failed($"Error reading template file: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public MergeResult Merge(Stream templateStream, IDictionary<string, string> mergeData, MergeFieldOptions? options = null)
    {
        if (templateStream == null)
            return MergeResult.Failed("Template stream cannot be null.");

        try
        {
            using var memoryStream = new MemoryStream();
            templateStream.CopyTo(memoryStream);
            return Merge(memoryStream.ToArray(), mergeData, options);
        }
        catch (Exception ex)
        {
            return MergeResult.Failed($"Error reading template stream: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public MergeResult Merge(byte[] templateBytes, IDictionary<string, string> mergeData, MergeFieldOptions? options = null)
    {
        if (templateBytes == null || templateBytes.Length == 0)
            return MergeResult.Failed("Template bytes cannot be null or empty.");

        if (mergeData == null)
            return MergeResult.Failed("Merge data cannot be null.");

        options ??= MergeFieldOptions.Default;

        try
        {
            using var memoryStream = new MemoryStream();
            memoryStream.Write(templateBytes, 0, templateBytes.Length);
            memoryStream.Position = 0;

            using var document = WordprocessingDocument.Open(memoryStream, true);

            if (document.MainDocumentPart?.Document?.Body == null)
                return MergeResult.Failed("Invalid Word document: missing main document body.");

            var fieldsFound = new List<string>();
            var fieldsReplaced = new List<string>();
            var unmatchedFields = new List<string>();

            // Process main document body
            ProcessDocumentPart(document.MainDocumentPart, mergeData, options, fieldsFound, fieldsReplaced, unmatchedFields);

            // Process headers
            foreach (var headerPart in document.MainDocumentPart.HeaderParts)
            {
                ProcessHeaderFooter(headerPart.Header, mergeData, options, fieldsFound, fieldsReplaced, unmatchedFields);
            }

            // Process footers
            foreach (var footerPart in document.MainDocumentPart.FooterParts)
            {
                ProcessHeaderFooter(footerPart.Footer, mergeData, options, fieldsFound, fieldsReplaced, unmatchedFields);
            }

            document.MainDocumentPart.Document.Save();

            return MergeResult.Successful(
                memoryStream.ToArray(),
                fieldsFound.Distinct().ToList(),
                fieldsReplaced.Distinct().ToList(),
                unmatchedFields.Distinct().ToList());
        }
        catch (Exception ex)
        {
            return MergeResult.Failed($"Error performing mail merge: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public MergeResult Merge<T>(string templatePath, T data, MergeFieldOptions? options = null) where T : class
    {
        var mergeData = ObjectToDictionary(data);
        return Merge(templatePath, mergeData, options);
    }

    /// <inheritdoc />
    public MergeResult Merge<T>(Stream templateStream, T data, MergeFieldOptions? options = null) where T : class
    {
        var mergeData = ObjectToDictionary(data);
        return Merge(templateStream, mergeData, options);
    }

    /// <inheritdoc />
    public MergeResult Merge<T>(byte[] templateBytes, T data, MergeFieldOptions? options = null) where T : class
    {
        var mergeData = ObjectToDictionary(data);
        return Merge(templateBytes, mergeData, options);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetMergeFields(string templatePath, MergeFieldOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(templatePath) || !File.Exists(templatePath))
            return Array.Empty<string>();

        var templateBytes = File.ReadAllBytes(templatePath);
        return GetMergeFields(templateBytes, options);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetMergeFields(Stream templateStream, MergeFieldOptions? options = null)
    {
        if (templateStream == null)
            return Array.Empty<string>();

        using var memoryStream = new MemoryStream();
        templateStream.CopyTo(memoryStream);
        return GetMergeFields(memoryStream.ToArray(), options);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetMergeFields(byte[] templateBytes, MergeFieldOptions? options = null)
    {
        if (templateBytes == null || templateBytes.Length == 0)
            return Array.Empty<string>();

        options ??= MergeFieldOptions.Default;
        var fields = new HashSet<string>();

        try
        {
            using var memoryStream = new MemoryStream(templateBytes);
            using var document = WordprocessingDocument.Open(memoryStream, false);

            if (document.MainDocumentPart?.Document?.Body == null)
                return Array.Empty<string>();

            var pattern = BuildFieldPattern(options);

            // Extract from main body
            ExtractFieldsFromElement(document.MainDocumentPart.Document.Body, pattern, options, fields);

            // Extract from headers
            foreach (var headerPart in document.MainDocumentPart.HeaderParts)
            {
                ExtractFieldsFromElement(headerPart.Header, pattern, options, fields);
            }

            // Extract from footers
            foreach (var footerPart in document.MainDocumentPart.FooterParts)
            {
                ExtractFieldsFromElement(footerPart.Footer, pattern, options, fields);
            }

            return fields.ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    /// <inheritdoc />
    public MergeResult MergeToFile(string templatePath, string outputPath, IDictionary<string, string> mergeData, MergeFieldOptions? options = null)
    {
        var result = Merge(templatePath, mergeData, options);

        if (!result.Success || result.Document == null)
            return result;

        try
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(outputPath, result.Document);

            return new MergeResult
            {
                Success = true,
                FieldsFound = result.FieldsFound,
                FieldsReplaced = result.FieldsReplaced,
                UnmatchedFields = result.UnmatchedFields
            };
        }
        catch (Exception ex)
        {
            return MergeResult.Failed($"Error saving merged document: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public MergeResult MergeToFile<T>(string templatePath, string outputPath, T data, MergeFieldOptions? options = null) where T : class
    {
        var mergeData = ObjectToDictionary(data);
        return MergeToFile(templatePath, outputPath, mergeData, options);
    }

    private void ProcessDocumentPart(MainDocumentPart mainPart, IDictionary<string, string> mergeData,
        MergeFieldOptions options, List<string> fieldsFound, List<string> fieldsReplaced, List<string> unmatchedFields)
    {
        var body = mainPart.Document.Body;
        if (body == null) return;

        ProcessElement(body, mergeData, options, fieldsFound, fieldsReplaced, unmatchedFields);
    }

    private void ProcessHeaderFooter(OpenXmlCompositeElement? element, IDictionary<string, string> mergeData,
        MergeFieldOptions options, List<string> fieldsFound, List<string> fieldsReplaced, List<string> unmatchedFields)
    {
        if (element == null) return;
        ProcessElement(element, mergeData, options, fieldsFound, fieldsReplaced, unmatchedFields);
    }

    private void ProcessElement(OpenXmlCompositeElement element, IDictionary<string, string> mergeData,
        MergeFieldOptions options, List<string> fieldsFound, List<string> fieldsReplaced, List<string> unmatchedFields)
    {
        // Process all paragraphs
        foreach (var paragraph in element.Descendants<Paragraph>())
        {
            ProcessParagraph(paragraph, mergeData, options, fieldsFound, fieldsReplaced, unmatchedFields);
        }
    }

    private void ProcessParagraph(Paragraph paragraph, IDictionary<string, string> mergeData,
        MergeFieldOptions options, List<string> fieldsFound, List<string> fieldsReplaced, List<string> unmatchedFields)
    {
        // Get all text content from the paragraph, accounting for split runs
        var runs = paragraph.Descendants<Run>().ToList();
        if (runs.Count == 0) return;

        // Collect all text content to find merge fields
        var fullText = string.Join("", runs.SelectMany(r => r.Descendants<Text>()).Select(t => t.Text));
        var pattern = BuildFieldPattern(options);
        var matches = Regex.Matches(fullText, pattern);

        if (matches.Count == 0) return;

        // For each match, find the field name and replace it
        foreach (Match match in matches)
        {
            var fieldName = match.Groups[1].Value.Trim();
            fieldsFound.Add(fieldName);

            var lookupKey = options.CaseInsensitive
                ? mergeData.Keys.FirstOrDefault(k => k.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                : mergeData.Keys.FirstOrDefault(k => k == fieldName);

            string? replacementValue = null;
            if (lookupKey != null)
            {
                replacementValue = mergeData[lookupKey];
                fieldsReplaced.Add(fieldName);
            }
            else
            {
                unmatchedFields.Add(fieldName);
                if (options.RemoveUnmatchedFields)
                {
                    replacementValue = options.UnmatchedFieldReplacement;
                }
            }

            if (replacementValue != null)
            {
                ReplaceFieldInParagraph(paragraph, match.Value, replacementValue);
            }
        }
    }

    private void ReplaceFieldInParagraph(Paragraph paragraph, string fieldPlaceholder, string replacement)
    {
        // Rebuild paragraph text and replace the field
        var runs = paragraph.Descendants<Run>().ToList();
        if (runs.Count == 0) return;

        // Get all text elements
        var textElements = runs.SelectMany(r => r.Descendants<Text>()).ToList();
        if (textElements.Count == 0) return;

        // Build a map of character positions to text elements
        var fullText = new System.Text.StringBuilder();
        var charMap = new List<(Text textElement, int startIndex, int endIndex)>();

        foreach (var text in textElements)
        {
            var start = fullText.Length;
            fullText.Append(text.Text ?? "");
            charMap.Add((text, start, fullText.Length - 1));
        }

        var textString = fullText.ToString();
        var fieldIndex = textString.IndexOf(fieldPlaceholder, StringComparison.Ordinal);

        if (fieldIndex < 0) return;

        var fieldEndIndex = fieldIndex + fieldPlaceholder.Length - 1;

        // Find which text elements contain the field
        var affectedElements = charMap
            .Where(m => m.endIndex >= fieldIndex && m.startIndex <= fieldEndIndex)
            .ToList();

        if (affectedElements.Count == 0) return;

        if (affectedElements.Count == 1)
        {
            // Simple case: field is within a single text element
            var (textElement, startIdx, _) = affectedElements[0];
            var localStart = fieldIndex - startIdx;
            textElement.Text = textElement.Text?.Remove(localStart, fieldPlaceholder.Length).Insert(localStart, replacement);
        }
        else
        {
            // Complex case: field spans multiple text elements
            // Put the replacement in the first element and remove from others
            var first = affectedElements.First();
            var localStart = fieldIndex - first.startIndex;
            var remainingInFirst = (first.textElement.Text?.Length ?? 0) - localStart;

            // Update first element
            first.textElement.Text = first.textElement.Text?.Substring(0, localStart) + replacement;

            // Calculate how many characters of the field remain after the first element
            var remainingFieldChars = fieldPlaceholder.Length - remainingInFirst;

            // Process middle and last elements
            for (int i = 1; i < affectedElements.Count; i++)
            {
                var (textElement, startIdx, _) = affectedElements[i];
                var elementLength = textElement.Text?.Length ?? 0;

                if (remainingFieldChars >= elementLength)
                {
                    // This entire element is part of the field
                    textElement.Text = "";
                    remainingFieldChars -= elementLength;
                }
                else
                {
                    // Only part of this element is the field
                    textElement.Text = textElement.Text?.Substring(remainingFieldChars);
                    remainingFieldChars = 0;
                }
            }
        }
    }

    private void ExtractFieldsFromElement(OpenXmlCompositeElement element, string pattern,
        MergeFieldOptions options, HashSet<string> fields)
    {
        foreach (var paragraph in element.Descendants<Paragraph>())
        {
            var runs = paragraph.Descendants<Run>().ToList();
            var fullText = string.Join("", runs.SelectMany(r => r.Descendants<Text>()).Select(t => t.Text));

            var matches = Regex.Matches(fullText, pattern);
            foreach (Match match in matches)
            {
                var fieldName = match.Groups[1].Value.Trim();
                fields.Add(fieldName);
            }
        }
    }

    private string BuildFieldPattern(MergeFieldOptions options)
    {
        var prefix = Regex.Escape(options.FieldPrefix);
        var suffix = Regex.Escape(options.FieldSuffix);
        return $@"{prefix}\s*(.+?)\s*{suffix}";
    }

    private IDictionary<string, string> ObjectToDictionary<T>(T obj) where T : class
    {
        var dictionary = new Dictionary<string, string>();

        if (obj == null) return dictionary;

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead) continue;

            var value = property.GetValue(obj);
            var stringValue = value?.ToString() ?? string.Empty;
            dictionary[property.Name] = stringValue;
        }

        return dictionary;
    }
}
