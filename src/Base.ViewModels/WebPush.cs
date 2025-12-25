using System.ComponentModel.DataAnnotations;

namespace ViewModels;

public class WebPush
{
    [Display(Name = " عنوان  ")]
    [Required(ErrorMessage = "وارد کردن {0}  الزامیست")]
    public string? title { set; get; }
    [Display(Name = " شرح ")]
    [Required(ErrorMessage = "وارد کردن {0}  الزامیست")]
    public string? message { set; get; }
    public string? icon { set; get; }
    public dynamic? Data { get; set; }
}