using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace Services.Contracts;

public interface ILocationService
{
    Task<int> addCity(CityDto city);
    Task<List<UnitedDto>> FindUnitedALLAsync();
    Task<List<CityDto>> FindCityALLAsync(int id);
    Task<CityDto> FindCityAsync(int id);
    Task<UnitedDto> FindUnitedByIDAsync(int id);
    Task<List<CityDto>> FindCityByUnitedIDAsync(int id);

}
