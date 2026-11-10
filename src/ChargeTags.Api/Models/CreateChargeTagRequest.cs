namespace ChargeTags.Api.Models;

public class CreateChargeTagRequest
{
    /// <summary>Format: dk-{10 digits}-clever</summary>
    /// <example>dk-0123456789-clever</example>
    public string TagId { get; init; } = string.Empty;
}
