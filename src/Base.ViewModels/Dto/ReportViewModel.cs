using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.Dto;

public class ReportViewModel
{
    public int Id { get; set; }
    [Display(Name = "کد")]
    public string? Code { get; set; }
    [Display(Name = "عنوان")]
    public string? Title { get; set; }
    public string? ReportJson { get; set; }
}

