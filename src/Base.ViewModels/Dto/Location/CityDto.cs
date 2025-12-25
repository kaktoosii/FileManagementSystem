using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ViewModels;

public class CityDto
{
    public int Id { get; set; }
    [Display(Name="شهرستان")]
    public string Title { get; set; }
    public int UnitedId { get; set; }
    public CityDto(int id, string name)
    {
        Id = id;
        Title = name;
    }
}
