using api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace api.Tests.Unit.Validation.Dtos;

public class TrimmedLengthAttributeTests
{
    [Fact]
    public void IsValid_NullValue_ReturnsSuccess()
    {
        var attribute = new TrimmedLengthAttribute(3, 10);

        var result = Validate(attribute, null);

        Assert.Null(result);
    }

    [Fact]
    public void IsValid_StringAtMinimumLength_ReturnsSuccess()
    {
        var attribute = new TrimmedLengthAttribute(3, 10);

        var result = Validate(attribute, "abc");

        Assert.Null(result);
    }

    [Fact]
    public void IsValid_StringAtMaximumLength_ReturnsSuccess()
    {
        var attribute = new TrimmedLengthAttribute(3, 10);

        var result = Validate(attribute, "abcdefghij");

        Assert.Null(result);
    }

    [Fact]
    public void IsValid_StringShorterThanMinimumLength_ReturnsValidationError()
    {
        var attribute = new TrimmedLengthAttribute(3, 10);

        var result = Validate(attribute, "ab");

        Assert.NotNull(result);
        Assert.Equal(
            "Length must be between 3 and 10 characters.",
            result!.ErrorMessage);
    }

    [Fact]
    public void IsValid_StringLongerThanMaximumLength_ReturnsValidationError()
    {
        var attribute = new TrimmedLengthAttribute(3, 10);

        var result = Validate(attribute, "abcdefghijk");

        Assert.NotNull(result);
        Assert.Equal(
            "Length must be between 3 and 10 characters.",
            result!.ErrorMessage);
    }

    [Fact]
    public void IsValid_LeadingAndTrailingWhitespace_IsIgnored()
    {
        var attribute = new TrimmedLengthAttribute(3, 10);

        var result = Validate(attribute, "         abc         ");

        Assert.Null(result);
    }

    [Fact]
    public void IsValid_OnlyWhitespace_ReturnsValidationError()
    {
        var attribute = new TrimmedLengthAttribute(3, 10);

        var result = Validate(attribute, "    ");

        Assert.NotNull(result);
    }

    [Fact]
    public void IsValid_CustomErrorMessage_UsesCustomMessage()
    {
        var attribute = new TrimmedLengthAttribute(3, 10)
        {
            ErrorMessage = "Comment length is invalid."
        };

        var result = Validate(attribute, "ab");

        Assert.NotNull(result);
        Assert.Equal("Comment length is invalid.", result!.ErrorMessage);
    }

    [Fact]
    public void IsValid_NonStringValue_ReturnsSuccess()
    {
        var attribute = new TrimmedLengthAttribute(3, 10);

        var result = Validate(attribute, 123);

        Assert.Null(result);
    }

    private static ValidationResult? Validate(
        TrimmedLengthAttribute attribute,
        object? value,
        string memberName = "Comment")
    {
        var context = new ValidationContext(new object())
        {
            MemberName = memberName
        };

        return attribute.GetValidationResult(value, context);
    }
}