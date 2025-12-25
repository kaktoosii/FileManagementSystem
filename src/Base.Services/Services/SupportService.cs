using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Base.DomainClasses.Enums;
using Base.ViewModels;
using Base.ViewModels.Dto.Support;
using Common.GuardToolkit;
using Common.IdentityToolkit;
using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViewModels.Dto;

namespace Services;

public class SupportService : ISupportService
{
    private readonly DbSet<SupportRequest> _supportRequests;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUnitOfWork _uow;

    public SupportService(IUnitOfWork uow, IHttpContextAccessor contextAccessor)
    {
        _uow = uow;
        _uow.CheckArgumentIsNull(nameof(_uow));

        _supportRequests = _uow.Set<SupportRequest>();
        _contextAccessor = contextAccessor;
    }

    public async Task<int> CreateSupportRequestAsync(SupportRequestDto supportRequestDto)
    {
        if (supportRequestDto == null)
            throw new AppException("اطلاعات الزامی را وارد کنید.");
        var userId = _contextAccessor.HttpContext.User.Identity.GetUserId();
        var supportRequest = new SupportRequest(
            supportRequestDto.Subject,
            supportRequestDto.Message,
            userId
        );

        await _supportRequests.AddAsync(supportRequest);
        await _uow.SaveChangesAsync();
        return supportRequest.Id;
    }

    public async Task RespondToSupportRequestAsync(int requestId, SupportResponseDto responseDto)
    {
        if (responseDto == null)
            throw new AppException("لطفاً پاسخ را وارد کنید.");

        var supportRequest = await _supportRequests.FindAsync(requestId);
        if (supportRequest == null)
            throw new AppException("درخواست پشتیبانی یافت نشد.");
        var userId = _contextAccessor.HttpContext.User.Identity.GetUserId();
        var response = new SupportResponse(
            responseDto.ResponseMessage,
            requestId,
            userId
        );

        _uow.Set<SupportResponse>().Add(response);
        supportRequest.UpdateStatus(RequestStatus.RESOLVED);

        await _uow.SaveChangesAsync();
    }

    public async Task<PagedResponse<List<SupportRequestViewModel>>> GetSupportRequestsAsync(SupportFilterDto filterDto)
    {
        if (filterDto == null)
            throw new AppException(nameof(filterDto));
        SupportFilterDto validFilter = new(pageNumber: filterDto.PageNumber, filterDto.PageSize, filterDto.SearchText);
        var list = _supportRequests.Include(x=>x.Customer).OrderByDescending(x=>x.CreatedAt).AsQueryable();

        if (!string.IsNullOrWhiteSpace(validFilter.SearchText))
        {
            list = list.Where(x => x.Subject.Contains(validFilter.SearchText));
        }
        var totalRecords = await list.Select(x => x.Id).CountAsync();
        var pagedData = await list
            .Select(x => new SupportRequestViewModel
            {
                Id = x.Id,
                CustomerName = x.Customer.DisplayName??x.Customer.Username,
                Subject = x.Subject,
                Message = x.Message,
                Status = x.Status.ToString(),
                CreatedAt = x.CreatedAt,
                CustomerId = x.CustomerId
            })
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToListAsync();

        return new PagedResponse<List<SupportRequestViewModel>>(pagedData, validFilter.PageNumber, validFilter.PageSize, totalRecords);
    }

    public async Task<SupportRequestDetailViewModel> GetSupportRequestByIdAsync(int id)
    {
        var model = await _supportRequests
            .Include(x=>x.Customer)
            .Where(x => x.Id == id)
            .Select(x => new SupportRequestDetailViewModel
            {
                Id = x.Id,
                Subject = x.Subject,
                Message = x.Message,
                Status = x.Status.ToString(),
                CreatedAt = x.CreatedAt,
                CustomerName = x.Customer.DisplayName ?? x.Customer.Username,
                CustomerId = x.CustomerId,
                Response = x.Response != null ? new SupportResponseViewModel
                {
                    Id = x.Response.Id,
                    ResponseMessage = x.Response.ResponseMessage,
                    RespondedAt = x.Response.RespondedAt,
                    AdminId = x.Response.AdminId
                } : null
            })
            .FirstOrDefaultAsync();

        if (model == null)
            throw new AppException("درخواست پشتیبانی یافت نشد.");

        return model;
    }

    public async Task<List<SupportRequestDetailViewModel>> GetUserSupportRequestsAsync()
    {
        var userId = _contextAccessor.HttpContext.User.Identity.GetUserId();
        var model = await _supportRequests
           .Where(x => x.CustomerId == userId)
           .Select(x => new SupportRequestDetailViewModel
           {
               Id = x.Id,
               Subject = x.Subject,
               Message = x.Message,
               Status = x.Status.ToString(),
               CreatedAt = x.CreatedAt,
               CustomerId = x.CustomerId,
               Response = x.Response != null ? new SupportResponseViewModel
               {
                   Id = x.Response.Id,
                   ResponseMessage = x.Response.ResponseMessage,
                   RespondedAt = x.Response.RespondedAt,
                   AdminId = x.Response.AdminId
               } : null
           })
           .OrderByDescending(x=>x.CreatedAt)
           .ToListAsync();

        if (model == null)
            throw new AppException("درخواست پشتیبانی یافت نشد.");

        return model;
    }

}
