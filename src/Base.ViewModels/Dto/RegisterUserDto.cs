using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels;
public class RegisterUserDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImage { get; set; }
    public string? MobileNumber { get; set; }
    public bool IsCheckDistance { get; set; }
    public int Distance { get; set; }
    public string? DeviceId { get; set; }
}