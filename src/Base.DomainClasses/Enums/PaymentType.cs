using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.DomainClasses.Enums;
public enum PaymentType
{
        None,
        Cash = 1,
        CardReader = 2,
        TransferToTheAccount = 3,
}
