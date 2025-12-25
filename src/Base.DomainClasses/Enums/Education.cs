using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.DomainClasses.Enums;
public enum Education
{
    None,
    [Display(Name = "دکتری Phd")]
    Phd = 1,
    [Display(Name = "کارشناسی ارشد")]
    Master = 2,
    [Display(Name = "کارشناسی")]
    Bachelor = 3,
    [Display(Name = "کاردانی")]
    Associate = 4,
    [Display(Name = "دیپلم")]
    Diploma = 6,
    [Display(Name = "زیردیپلم")]
    UnderDiploma = 5
}
