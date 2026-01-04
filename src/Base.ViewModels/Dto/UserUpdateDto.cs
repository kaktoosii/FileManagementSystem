using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels;
public class UserUpdateDto
{
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? MobileNumber { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImage { get; set; }

}