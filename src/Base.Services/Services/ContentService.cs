using Base.Common.Helpers;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Base.ViewModels;
using Common.GuardToolkit;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using System.Linq.Dynamic.Core;
using ViewModels.Dto;

namespace Services.Services;
public class ContentService : IContentService
{
    private readonly DbSet<Content> _contents;
    private readonly IUnitOfWork _uow;

    public ContentService(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _contents = _uow.Set<Content>();
    }

    public async Task<int> AddNewContentAsync(ContentDto content)
    {
        if (content == null)
            throw new AppException("محتوا نباید خالی باشد.");


        var newContent = new Content(
            content.Title, content.Body,content.LanguageCode, content.Summary, content.ImageId, content.CategoryId,
            content.AuthorId, content.PublishedDate, content.IsPublished, content.Priority,content.ContentGroupId);

        await _contents.AddAsync(newContent);
        await _uow.SaveChangesAsync();
        return newContent.Id;
    }

    public async Task<List<ContentViewModel>> GetAllContentByCategoryAsync(int categoryId)
    {
        return await _contents
            .Where(c => c.CategoryId == categoryId && c.IsPublished)
            .Select(c => ConvertToViewModel(c))
            .ToListAsync();
    }

    public async Task<PagedResponse<List<ContentViewModel>>> GetAllContentPagedAsync(ContentFilterDto filter)
    {
        if (filter == null)
            throw new AppException("فیلتر نباید خالی باشد.");

        var query = _contents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            query = query.Where(c => c.Title.Contains(filter.SearchText) || c.Body.Contains(filter.SearchText));

        if (filter.CategoryId.HasValue)
            query = query.Where(c => c.CategoryId == filter.CategoryId.Value);

        var totalRecords = await query.CountAsync();
        var pagedData = await query
            .OrderByDescending(c => c.PublishedDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => ConvertToViewModel(c))
            .ToListAsync();

        return new PagedResponse<List<ContentViewModel>>(pagedData, filter.PageNumber, filter.PageSize, totalRecords);
    }

    public async Task DeleteContentAsync(int id)
    {
        var content = await _contents.FindAsync(id) ?? throw new AppException("محتوایی یافت نشد.");
        _contents.Remove(content);
        await _uow.SaveChangesAsync();
    }

    public async Task<ContentViewModel> FindContentByIdAsync(int contentId)
    {
        var content = await _contents.FindAsync(contentId) ?? throw new AppException("محتوایی یافت نشد.");
        return ConvertToViewModel(content);
    }

    public async Task<ContentDto> GetContentByIdAsync(int contentId)
    {
        var content = await _contents.FindAsync(contentId) ?? throw new AppException("محتوایی یافت نشد.");
        return ConvertToDto(content);
    }

    public async Task UpdateContentAsync(ContentDto content)
    {
        if (content == null)
            throw new AppException("محتوا نباید خالی باشد.");

        var existingContent = await _contents
            .FirstOrDefaultAsync(c => c.Id == content.Id) ?? throw new AppException("محتوایی یافت نشد.");

        existingContent.Update(
            content.Title, content.Body,content.LanguageCode, content.Summary, content.ImageId, content.CategoryId,
            content.AuthorId, content.PublishedDate, content.IsPublished, content.Priority);

        _contents.Update(existingContent);
        await _uow.SaveChangesAsync();
    }

    public async Task PublishOrUnpublishContentAsync(int contentId)
    {
        var content = await _contents.FindAsync(contentId) ?? throw new AppException("محتوایی یافت نشد.");
        content.TogglePublishStatus();
        _contents.Update(content);
        await _uow.SaveChangesAsync();
    }

    private static ContentViewModel ConvertToViewModel(Content content)
    {
        return new ContentViewModel
        {
            Id = content.Id,
            Title = content.Title,
            Summary = content.Summary,
            Body = content.Body,
            ImageId = content.ImageId,
            CategoryId = content.CategoryId,
            AuthorId = content.AuthorId,
            PublishedDate = content.PublishedDate,
            IsPublished = content.IsPublished,
            Priority = content.Priority
        };
    }

    private static ContentDto ConvertToDto(Content content)
    {
        return new ContentDto
        {
            Id = content.Id,
            Title = content.Title,
            Summary = content.Summary,
            Body = content.Body,
            ImageId = content.ImageId,
            CategoryId = content.CategoryId,
            AuthorId = content.AuthorId,
            PublishedDate = content.PublishedDate,
            IsPublished = content.IsPublished,
            Priority = content.Priority
        };
    }


}
