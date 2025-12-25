namespace Base.Common.SiteOptions;

public class SiteSettingsDto
{
    public BearerTokenOptionDto BearerToken { set; get; }

    public IReadOnlyList<UserOptionDto> TestUsers { set; get; }

    public UserOptionDto AdminUser { set; get; }

    public string Secret { set; get; }
    public string SmsToken { set; get; }

}