namespace ChargeTags.Api.Data;

public class ChargeTag
{
    // e.g. "dk-0123456789-clever"
    public string Id { get; set; } = string.Empty;

    public bool IsBlocked { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    // Null until the tag is blocked
    public DateTimeOffset? BlockedAt { get; set; }
}
