using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Common.GuardToolkit;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using ViewModels;
using ViewModels.Dto;

namespace Services.Services;

public class LocationService : ILocationService
{
    private readonly DbSet<United> _united;
    private readonly DbSet<City> _city;
    private readonly IUnitOfWork _uow;
    public LocationService(IUnitOfWork uow)
    {
        _uow = uow;
        _uow.CheckArgumentIsNull(nameof(_uow));
        _united = _uow.Set<United>();
        _city = _uow.Set<City>();
    }
    public async Task<int> addCity(CityDto city)
    {
        if (city == null)
        {
            throw new AppException(".اطلاعات الزامی را وارد کنید");
        }
        var model = new City(city.Title);
        await _city.AddAsync(model);
        await _uow.SaveChangesAsync();
        return model.Id;
    }
    public async Task<List<CityDto>> FindCityByUnitedIDAsync(int id)
    {
        var listCity = _city.Include(x=>x.United).Where(x => x.United.Id == id).Select(city => new CityDto(city.Id, city.Title));
        return await listCity.ToListAsync();
    }


    public async Task<List<UnitedDto>> FindUnitedALLAsync()
    {
        return await _united.Select(united => new UnitedDto(united.Id, united.Title)).ToListAsync();
    }

    public async Task<UnitedDto> FindUnitedByIDAsync(int id)
    {
        var item = await _united.FindAsync(id);
        UnitedDto unitedDto = new UnitedDto(item.Id, item.Title);
        return unitedDto;
    }



    public async Task<List<CityDto>> FindCityALLAsync(int id)
    {
        return await _city.Include(x => x.United).Select(x => new CityDto(x.Id,x.Title))
            .Where(x=>x.UnitedId==id)
            .ToListAsync();
    }

    public async Task<CityDto> FindCityAsync(int id)
    {
        return await _city.Select(x => new CityDto(x.Id, x.Title)).FirstOrDefaultAsync(x => x.Id == id);
    }
}
