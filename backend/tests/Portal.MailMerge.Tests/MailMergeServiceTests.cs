using FluentAssertions;
using Xunit;

namespace Portal.MailMerge.Tests;

public class MailMergeServiceTests
{
    private readonly MailMergeService _service;

    public MailMergeServiceTests()
    {
        _service = new MailMergeService();
    }

    #region Basic Merge Tests

    [Fact]
    public void Merge_WithSimpleField_ReplacesFieldWithValue()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("Hello {{Name}}!");
        var data = new Dictionary<string, string> { { "Name", "World" } };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        result.Document.Should().NotBeNull();

        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Hello World!");
    }

    [Fact]
    public void Merge_WithMultipleFields_ReplacesAllFields()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument(
            "Dear {{FirstName}} {{LastName}},",
            "Your order #{{OrderNumber}} has been shipped.");
        var data = new Dictionary<string, string>
        {
            { "FirstName", "John" },
            { "LastName", "Doe" },
            { "OrderNumber", "12345" }
        };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Contain("Dear John Doe,");
        text.Should().Contain("Your order #12345 has been shipped.");
    }

    [Fact]
    public void Merge_WithFieldSpacingVariations_HandlesCorrectly()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument(
            "{{ Name }} - {{Name}} - {{  Name  }}");
        var data = new Dictionary<string, string> { { "Name", "Test" } };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Test - Test - Test");
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Merge_WithCaseInsensitiveOption_MatchesRegardlessOfCase()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("{{name}} {{NAME}} {{Name}}");
        var data = new Dictionary<string, string> { { "Name", "Test" } };
        var options = new MergeFieldOptions { CaseInsensitive = true };

        // Act
        var result = _service.Merge(template, data, options);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Test Test Test");
    }

    [Fact]
    public void Merge_WithCaseSensitiveOption_OnlyMatchesExactCase()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("{{name}} {{NAME}} {{Name}}");
        var data = new Dictionary<string, string> { { "Name", "Test" } };
        var options = new MergeFieldOptions { CaseInsensitive = false };

        // Act
        var result = _service.Merge(template, data, options);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Contain("Test");
        result.FieldsReplaced.Should().Contain("Name");
        result.UnmatchedFields.Should().Contain("name");
        result.UnmatchedFields.Should().Contain("NAME");
    }

    #endregion

    #region Unmatched Field Tests

    [Fact]
    public void Merge_WithUnmatchedField_RemovesByDefault()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("Hello {{Name}}! Your code is {{Code}}.");
        var data = new Dictionary<string, string> { { "Name", "World" } };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Hello World! Your code is .");
        result.UnmatchedFields.Should().Contain("Code");
    }

    [Fact]
    public void Merge_WithCustomUnmatchedReplacement_UsesCustomValue()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("Hello {{Name}}! Your code is {{Code}}.");
        var data = new Dictionary<string, string> { { "Name", "World" } };
        var options = new MergeFieldOptions { UnmatchedFieldReplacement = "[MISSING]" };

        // Act
        var result = _service.Merge(template, data, options);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Hello World! Your code is [MISSING].");
    }

    [Fact]
    public void Merge_WithRemoveUnmatchedFalse_KeepsPlaceholder()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("Hello {{Name}}! Your code is {{Code}}.");
        var data = new Dictionary<string, string> { { "Name", "World" } };
        var options = new MergeFieldOptions { RemoveUnmatchedFields = false };

        // Act
        var result = _service.Merge(template, data, options);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Contain("{{Code}}");
    }

    #endregion

    #region Custom Delimiters Tests

    [Fact]
    public void Merge_WithCustomDelimiters_UsesSpecifiedDelimiters()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("Hello [%Name%]!");
        var data = new Dictionary<string, string> { { "Name", "World" } };
        var options = new MergeFieldOptions { FieldPrefix = "[%", FieldSuffix = "%]" };

        // Act
        var result = _service.Merge(template, data, options);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Hello World!");
    }

    [Fact]
    public void Merge_WithMustacheDelimiters_Works()
    {
        // Arrange (default is already Mustache-style)
        var template = TestDocumentHelper.CreateDocument("Hello {{Name}}!");
        var data = new Dictionary<string, string> { { "Name", "World" } };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Hello World!");
    }

    #endregion

    #region Split Runs Tests

    [Fact]
    public void Merge_WithFieldSplitAcrossRuns_HandlesCorrectly()
    {
        // Arrange - simulate Word splitting "{{Name}}" across multiple runs
        var template = TestDocumentHelper.CreateDocumentWithSplitRuns(new[] { "Hello {{", "Name", "}}!" });
        var data = new Dictionary<string, string> { { "Name", "World" } };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Hello World!");
    }

    [Fact]
    public void Merge_WithComplexSplitRuns_HandlesCorrectly()
    {
        // Arrange - more complex split
        var template = TestDocumentHelper.CreateDocumentWithSplitRuns(new[] { "Dear {", "{First", "Name}", "} {{LastName}}" });
        var data = new Dictionary<string, string>
        {
            { "FirstName", "John" },
            { "LastName", "Doe" }
        };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Dear John Doe");
    }

    #endregion

    #region Header/Footer Tests

    [Fact]
    public void Merge_WithHeaderAndFooter_ReplacesFieldsInAllParts()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocumentWithHeaderFooter(
            "Body: {{Name}}",
            "Header: {{Company}}",
            "Footer: Page {{Page}}");
        var data = new Dictionary<string, string>
        {
            { "Name", "John" },
            { "Company", "Acme Inc" },
            { "Page", "1" }
        };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        var (body, header, footer) = TestDocumentHelper.ReadAllDocumentText(result.Document!);
        body.Should().Be("Body: John");
        header.Should().Be("Header: Acme Inc");
        footer.Should().Be("Footer: Page 1");
    }

    #endregion

    #region Object Merge Tests

    [Fact]
    public void Merge_WithObject_UsesPropertyValues()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("Name: {{Name}}, Age: {{Age}}");
        var data = new { Name = "John", Age = 30 };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Name: John, Age: 30");
    }

    [Fact]
    public void Merge_WithTypedObject_UsesPropertyValues()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("Employee: {{FirstName}} {{LastName}}, Department: {{Department}}");
        var employee = new TestEmployee
        {
            FirstName = "Jane",
            LastName = "Smith",
            Department = "Engineering"
        };

        // Act
        var result = _service.Merge(template, employee);

        // Assert
        result.Success.Should().BeTrue();
        var text = TestDocumentHelper.ReadDocumentText(result.Document!);
        text.Should().Be("Employee: Jane Smith, Department: Engineering");
    }

    #endregion

    #region GetMergeFields Tests

    [Fact]
    public void GetMergeFields_ReturnsAllFieldNames()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument(
            "Hello {{Name}}!",
            "Your order {{OrderNumber}} from {{Company}} is ready.");

        // Act
        var fields = _service.GetMergeFields(template);

        // Assert
        fields.Should().HaveCount(3);
        fields.Should().Contain("Name");
        fields.Should().Contain("OrderNumber");
        fields.Should().Contain("Company");
    }

    [Fact]
    public void GetMergeFields_ReturnsDuplicatesOnlyOnce()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument(
            "{{Name}} and {{Name}} and {{Name}}");

        // Act
        var fields = _service.GetMergeFields(template);

        // Assert
        fields.Should().HaveCount(1);
        fields.Should().Contain("Name");
    }

    [Fact]
    public void GetMergeFields_IncludesHeaderAndFooterFields()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocumentWithHeaderFooter(
            "Body: {{BodyField}}",
            "Header: {{HeaderField}}",
            "Footer: {{FooterField}}");

        // Act
        var fields = _service.GetMergeFields(template);

        // Assert
        fields.Should().HaveCount(3);
        fields.Should().Contain("BodyField");
        fields.Should().Contain("HeaderField");
        fields.Should().Contain("FooterField");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Merge_WithNullBytes_ReturnsFailure()
    {
        // Arrange
        byte[]? template = null;
        var data = new Dictionary<string, string>();

        // Act
        var result = _service.Merge(template!, data);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null or empty");
    }

    [Fact]
    public void Merge_WithEmptyBytes_ReturnsFailure()
    {
        // Arrange
        var template = Array.Empty<byte>();
        var data = new Dictionary<string, string>();

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null or empty");
    }

    [Fact]
    public void Merge_WithNullData_ReturnsFailure()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument("Hello");
        IDictionary<string, string>? data = null;

        // Act
        var result = _service.Merge(template, data!);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null");
    }

    [Fact]
    public void Merge_WithInvalidDocumentBytes_ReturnsFailure()
    {
        // Arrange
        var template = new byte[] { 1, 2, 3, 4, 5 };
        var data = new Dictionary<string, string>();

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Result Metadata Tests

    [Fact]
    public void Merge_PopulatesResultMetadataCorrectly()
    {
        // Arrange
        var template = TestDocumentHelper.CreateDocument(
            "{{Found1}} {{Found2}} {{NotFound}}");
        var data = new Dictionary<string, string>
        {
            { "Found1", "Value1" },
            { "Found2", "Value2" }
        };

        // Act
        var result = _service.Merge(template, data);

        // Assert
        result.Success.Should().BeTrue();
        result.FieldsFound.Should().HaveCount(3);
        result.FieldsReplaced.Should().HaveCount(2);
        result.FieldsReplaced.Should().Contain("Found1");
        result.FieldsReplaced.Should().Contain("Found2");
        result.UnmatchedFields.Should().HaveCount(1);
        result.UnmatchedFields.Should().Contain("NotFound");
    }

    #endregion
}

public class TestEmployee
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
