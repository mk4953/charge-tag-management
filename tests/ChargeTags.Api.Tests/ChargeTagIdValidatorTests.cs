using ChargeTags.Api.Validation;

namespace ChargeTags.Api.Tests;

public class ChargeTagIdValidatorTests
{
    [Test]
    public void IsValid_ReturnsTrue_ForCorrectFormat()
    {
        var isValid = ChargeTagIdValidator.IsValid("dk-0123456789-clever");

        Assert.That(isValid, Is.True);
    }

    [TestCase("dk-123-clever")]
    [TestCase("dk-01234567890-clever")]
    [TestCase("dk-abcdefghij-clever")]
    [TestCase("DK-0123456789-CLEVER")]
    [TestCase("se-0123456789-clever")]
    [TestCase("")]
    [TestCase(null)]
    public void IsValid_ReturnsFalse_ForWrongFormat(string? tagId)
    {
        var isValid = ChargeTagIdValidator.IsValid(tagId);

        Assert.That(isValid, Is.False);
    }
}
