namespace Base.Common.Features.Identity;

public class DynamicClaimsDto
{
    public int UserId { set; get; }
    public string ClaimType { set; get; } = default!;
    public IReadOnlyList<string> InputClaimValues { set; get; }
}