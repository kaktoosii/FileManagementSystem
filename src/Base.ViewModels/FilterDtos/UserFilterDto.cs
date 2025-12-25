using Base.DomainClasses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.ViewModels;
public class UserFilterDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? FirstName { get;  set; }
    public string? LastName { get;  set; }
    public string? UserName { get;  set; }
    public string? SearchText { get; set; }
    public UserFilterDto()
    {
        this.PageNumber = 1;
        this.PageSize = 10;
    }
    public UserFilterDto(int pageNumber, int pageSize, string? firstName, string? lastName, string? userName, string? searchText)
    {
        this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
        this.PageSize = pageSize;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.UserName = userName;
        this.SearchText = searchText;
    }
}