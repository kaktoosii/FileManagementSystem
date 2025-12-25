using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Base.ViewModels;
using Base.ViewModels.ViewModel;
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

public class MessageService : IMessageService
{
    private readonly DbSet<Message> _messages;
    private readonly DbSet<MessageSeen> _messageSeens;
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _contextAccessor;

    public MessageService(IUnitOfWork uow,IHttpContextAccessor contextAccessor)
    {
        _uow = uow;
        _uow.CheckArgumentIsNull(nameof(_uow));

        _messages = _uow.Set<Message>();
        _messageSeens = _uow.Set<MessageSeen>();
        _contextAccessor = contextAccessor;
    }

    public async Task<int> AddNewMessageAsync(MessageDto messageDto)
    {
        if (messageDto == null)
        {
            throw new AppException("لطفاً اطلاعات پیام را وارد کنید.");
        }
        var userId = _contextAccessor.HttpContext.User.Identity.GetUserId();
        var message = new Message(
            messageDto.Subject,
            messageDto.Description,
            userId,
            messageDto.PictureId
        );

        await _messages.AddAsync(message);
        await _uow.SaveChangesAsync();
        return message.Id;
    }

    public async Task EditMessageAsync(int id, MessageViewModel messageDto)
    {
        if (messageDto == null)
            throw new AppException("لطفاً اطلاعات پیام را وارد کنید.");

        var message = await _messages.FindAsync(id);
        if (message == null)
            throw new AppException("پیام مورد نظر یافت نشد.");

        message.Subject = messageDto.Subject;
        message.Description = messageDto.Description;
        message.PictureId = messageDto.PictureId;

        _messages.Update(message);
        await _uow.SaveChangesAsync();
    }

    public async Task<PagedResponse<List<MessageViewModel>>> GetMessages(MessageFilterDto filter)
    {
        if (filter == null)
            throw new AppException(nameof(filter));
        MessageFilterDto validFilter = new(pageNumber: filter.PageNumber, filter.PageSize, filter.SearchText);
        var list = _messages
            .Include(x=>x.MessageSeens)
            .OrderByDescending(x=>x.CreateDate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(validFilter.SearchText))
        {
            list = list.Where(x => x.Subject.Contains(validFilter.SearchText));
        }
        var totalRecords = await list.Select(x => x.Id).CountAsync();
        var pagedData = await list
            .Where(x => !x.IsDeleted)
            .Select(x => new MessageViewModel
            {
                Id = x.Id,
                Subject = x.Subject,
                Description = x.Description,
                PictureId = x.PictureId,
                SenderUserId = x.SenderUserId,
                UserName = x.User.DisplayName??x.User.Username,
                CreateDate = x.CreateDate.ToPersianDateTextify(true)
            })
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToListAsync();
        return new PagedResponse<List<MessageViewModel>>(pagedData, validFilter.PageNumber, validFilter.PageSize, totalRecords);
    }
    public async Task<PagedResponse<List<MessageViewModel>>> GetUserMessages(MessageFilterDto filter)
    {
        if (filter == null)
            throw new AppException(nameof(filter));
        MessageFilterDto validFilter = new(pageNumber: filter.PageNumber, filter.PageSize, filter.SearchText);
        var list = _messages
            .Include(x => x.MessageSeens)
            .AsQueryable();
        var userId = _contextAccessor.HttpContext.User.Identity.GetUserId();
        if (!string.IsNullOrWhiteSpace(validFilter.SearchText))
        {
            list = list.Where(x => x.Subject.Contains(validFilter.SearchText));
        }
        var totalRecords = await list.Select(x => x.Id).CountAsync();
        var pagedData = await list
            .Where(x => !x.IsDeleted)
            .Select(x => new MessageViewModel
            {
                Id = x.Id,
                Subject = x.Subject,
                Description = x.Description,
                PictureId = x.PictureId,
                SenderUserId = x.SenderUserId,
                CreateDate = x.CreateDate.ToPersianDateTextify(true),
                IsSeen = x.MessageSeens.Any(s=>s.UserId == userId && s.MessageId == x.Id)
            })
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToListAsync();
        return new PagedResponse<List<MessageViewModel>>(pagedData, validFilter.PageNumber, validFilter.PageSize, totalRecords);
    }
    public async Task<MessageViewModel> GetMessage(int id)
    {
        var message = await _messages
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new MessageViewModel
            {
                Id = x.Id,
                Subject = x.Subject,
                Description = x.Description,
                PictureId = x.PictureId,
                SenderUserId = x.SenderUserId,
                CreateDate = x.CreateDate.ToPersianDateTextify(true)
            }).FirstOrDefaultAsync();

        if (message == null)
            throw new AppException("پیام یافت نشد.");

        return message;
    }

    public async Task DeleteMessageAsync(int id)
    {
        var message = await _messages.FindAsync(id);
        if (message == null)
            throw new AppException("پیام مورد نظر یافت نشد.");

        message.SoftDelete();
        await _uow.SaveChangesAsync();
    }

    public async Task SeenMessageAsync(int id)
    {
        var userId = _contextAccessor.HttpContext.User.Identity.GetUserId();
        await _messageSeens.AddAsync(new MessageSeen(userId, id));
        await _uow.SaveChangesAsync();

    }
}
