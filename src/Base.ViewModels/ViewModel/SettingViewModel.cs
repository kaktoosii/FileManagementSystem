using System.ComponentModel.DataAnnotations;

namespace ViewModels;

public class SettingViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Meta { get; set; }
    public string? PushToken { get; set; }
    public string? PushApikey { get; set; }
    public string? Icon { get; set; }
    public string? FcmServerKey { get; set; }
    public string? FcmSenderId { get; set; }
    public string? Telegram { get; set; }
    public string? Instagram { get; set; }
    public string? LinkedIn { get; set; }
    public string? Footer { get; set; }
    public string? CopyRight { get; set; }
    public string? Phone { get; set; }
    public string? AboutUs { get; set; }
    public string? Rules { get; set; }
    public string? Questions { get; set; }
}
