namespace ChargeTags.Api.Models;

public class ChargeTagResponse
{
    public string TagId { get; init; } = string.Empty;

    public bool IsBlocked { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? BlockedAt { get; init; }
}
