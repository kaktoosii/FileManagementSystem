using Base.DomainClasses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels;
public class MessageFilterDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? SearchText { get; set; }
    public MessageFilterDto()
    {
        this.PageNumber = 1;
        this.PageSize = 10;
    }
    public MessageFilterDto(int pageNumber, int pageSize,string? searchText)
    {
        this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
        this.PageSize = pageSize;
        this.SearchText = searchText;
    }
}