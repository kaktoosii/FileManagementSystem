
using Base.Common.Features.Identity;
using Base.Common.Helpers;
using Base.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using ViewModels;

namespace Base.WebApp.Controllers;

[Route("api/v1/[controller]")]
public class LocationController : Controller
{
    private readonly ILocationService _locationService;
    public LocationController(ILocationService locationService)
    {
        this._locationService = locationService ?? throw new AppException(nameof(locationService));
    }
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(List<UnitedDto>), StatusCodes.Status200OK)]
    public async Task<JsonResult> Uniteds()
    {
        var unitedList = await _locationService.FindUnitedALLAsync();
        return Json(unitedList);
    }
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(List<CityDto>), StatusCodes.Status200OK)]
    public async Task<JsonResult> FindCityByUnitedId(int unitedId)
    {
        var cityList = await _locationService.FindCityByUnitedIDAsync(unitedId);
        return Json(cityList);
    }

    [HttpGet("[action]")]
    [ProducesResponseType(typeof(UnitedDto), StatusCodes.Status200OK)]
    public async Task<JsonResult> FindUnitedById(int id)
    {
        var united = await _locationService.FindUnitedByIDAsync(id);
        return Json(united);
    }

 
}