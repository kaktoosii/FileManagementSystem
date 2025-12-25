using DNTPersianUtils.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels.Dto.Support;
public class SupportRequestViewModel
{

    public int Id { get; set; }
    public string? CustomerName { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreateDatePersian
    {
        get
        {
            return CreatedAt.ToShortPersianDateString();
        }
    }
    public int CustomerId { get; set; }
  
}
