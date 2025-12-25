using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Common.GuardToolkit;
using Common.IdentityToolkit;
using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ViewModels;
using ViewModels.Dto;

namespace Services;

public class SettingService : ISettingService
{
    private readonly IUnitOfWork _uow;
    private readonly DbSet<Setting> _setting;

    public SettingService(
        IUnitOfWork uow)
    {
        _uow = uow;
        _uow.CheckArgumentIsNull(nameof(_uow));

        _setting = _uow.Set<Setting>();
    }



    public async Task<int> AddSettingAsync(SettingViewModel settingViewModel)
    {
        if (settingViewModel == null)
        {
            throw new AppException("مقادیر وارد نشده است.");
        }
        var model = new Setting(settingViewModel.Title,
                                settingViewModel.Meta,
                                settingViewModel.Description,
                                settingViewModel.PushToken,
                                settingViewModel.PushApikey,
                                settingViewModel.Icon,
                                settingViewModel.FcmServerKey,
                                settingViewModel.FcmSenderId,
                                settingViewModel.Footer,
                                settingViewModel.LinkedIn,
                                settingViewModel.Telegram,
                                settingViewModel.Instagram,
                                settingViewModel.CopyRight,
                                settingViewModel.Phone,
                                settingViewModel.AboutUs,
                                settingViewModel.Rules,
                                settingViewModel.Questions);
        await _setting.AddAsync(model);
        await _uow.SaveChangesAsync();
        return model.Id;
    }

    public async Task<SettingViewModel> GetSettingAsync()
    {
        var setting = await _setting.FirstOrDefaultAsync();
        if (setting == null)
        {
            await AddSettingAsync(new SettingViewModel
            {
                Title = "",
                Description = "",
                Meta = "",
                FcmSenderId = "",
                FcmServerKey = "",
                Icon = "",
                PushApikey = "",
                PushToken = ""
            });
        }

        return new SettingViewModel
        {
            Id = setting.Id,
            Title = setting.Title,
            Description = setting.Description,
            Meta = setting.Meta,
            FcmSenderId = setting.FcmSenderId,
            FcmServerKey = setting.FcmServerKey,
            Icon = setting.Icon,
            PushApikey = setting.PushApikey,
            PushToken = setting.PushToken,
            Footer = setting.Footer,
            LinkedIn = setting.LinkedIn,
            Telegram = setting.Telegram,
            Instagram = setting.Instagram,
            Phone = setting.Phone,
            AboutUs = setting.AboutUs,
            Rules = setting.Rules,
            Questions = setting.Questions
        };
    }

    public async Task UpdateSettingAsync(SettingViewModel settingViewModel)
    {
        if (settingViewModel == null)
        {
            throw new AppException("مقادیر وارد نشده است.");
        }
        var setting = await _setting.FirstOrDefaultAsync();
        if (setting == null)
        {
            throw new AppException("یافت نشد.");
        }

        setting.Update(
                settingViewModel.Title,
                settingViewModel.Meta,
                settingViewModel.Description,
                settingViewModel.PushToken,
                settingViewModel.PushApikey,
                settingViewModel.Icon,
                settingViewModel.FcmServerKey,
                settingViewModel.FcmSenderId,
                settingViewModel.Footer,
                settingViewModel.LinkedIn,
                settingViewModel.Telegram,
                settingViewModel.Instagram,
                settingViewModel.CopyRight,
                settingViewModel.Phone,
                settingViewModel.AboutUs,
                settingViewModel.Rules,
                settingViewModel.Questions
                );
        await _uow.SaveChangesAsync();
    }

}
