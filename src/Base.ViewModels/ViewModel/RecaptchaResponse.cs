using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels.ViewModel;
public class RecaptchaResponse
{

        public bool success { get; set; }
        public string? hostname { get; set; }
        public float score { get; set; }
        public string? action { get; set; }
    
}
