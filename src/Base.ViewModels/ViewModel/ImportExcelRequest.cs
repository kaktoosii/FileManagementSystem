using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels.ViewModel;
public class ImportExcelRequest
{
    public IFormFile? ExcelFile { get; set; }
    public string? MappingsJson { get; set; }
}