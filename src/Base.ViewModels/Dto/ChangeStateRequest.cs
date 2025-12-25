using Base.DomainClasses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels.Dto;
public class ChangeStateRequest
{
    public State NewState { get; set; }
    public string? Description { get; set; }
}
