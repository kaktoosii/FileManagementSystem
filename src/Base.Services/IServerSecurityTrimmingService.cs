namespace Services.Contracts;

public interface IServerSecurityTrimmingService
{
    Task<bool> CanCurrentUserAccessToActionAsync(
        string area, string controller, string action, string httpMethod);
}