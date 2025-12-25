using DNTPersianUtils.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels.Dto.Support;
public class SupportResponseViewModel
{
    public int Id { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime RespondedAt { get; set; }
    public string? RespondedAtPersian
    {
        get
        {
            return RespondedAt.ToShortPersianDateString();
        }
    }
    public int AdminId { get; set; }
}