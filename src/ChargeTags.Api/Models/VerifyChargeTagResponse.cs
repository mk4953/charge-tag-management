namespace ChargeTags.Api.Models;

public class VerifyChargeTagResponse
{
    public string TagId { get; init; } = string.Empty;

    public bool Exists { get; init; }

    public bool IsBlocked { get; init; }

    // True when the tag exists and is not blocked
    public bool CanCharge { get; init; }
}
