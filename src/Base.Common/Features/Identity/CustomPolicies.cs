namespace Base.Common.Features.Identity;

public static class CustomPolicies
{
    public const string DynamicClientPermission = $"::{nameof(DynamicClientPermission)}::";

    public const string DynamicServerPermission = $"{nameof(DynamicServerPermission)}";
}