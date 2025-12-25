using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ViewModels;
using ViewModels.Dto;

namespace Services.Contracts;

public interface ISettingService
{
    Task<int> AddSettingAsync(SettingViewModel settingViewModel);
    Task UpdateSettingAsync(SettingViewModel settingViewModel);
    Task<SettingViewModel> GetSettingAsync();
}
