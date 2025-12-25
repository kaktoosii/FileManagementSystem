using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.DomainClasses.Enums;
public enum State
{
    None,
    [Display(Name = "در انتظار بررسی")]
    PENDING = 1,
    [Display(Name = "تایید نشده")]
    NOTAPPROVED = 2,
    [Display(Name = "تایید شده")]
    APPROVED = 3

}
