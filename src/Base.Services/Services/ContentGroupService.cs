using Base.DataLayer.Context;
using Base.DomainClasses;
using Base.ViewModels;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using ViewModels.Dto;

namespace Services.Services;
public class ContentGroupService : IContentGroupService
{
    private readonly DbSet<ContentGroup> _contentGroups;
    private readonly IUnitOfWork _uow;

    public ContentGroupService(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _contentGroups = _uow.Set<ContentGroup>();
    }

    public async Task<int> AddNewContentGroupAsync(ContentGroupDto groupDto)
    {
        ArgumentNullException.ThrowIfNull(groupDto);

        var group = new ContentGroup(groupDto.GroupName, groupDto.Description);
        await _contentGroups.AddAsync(group);
        await _uow.SaveChangesAsync();
        return group.Id;
    }

    public async Task<List<ContentGroupViewModel>> GetAllContentGroupsAsync()
    {
        return await _contentGroups
            .Select(group => new ContentGroupViewModel
            {
                Id = group.Id,
                GroupName = group.GroupName,
                Description = group.Description
            }).ToListAsync();
    }

    public async Task<ContentGroupViewModel> GetContentGroupByIdAsync(int id)
    {
        var group = await _contentGroups.FindAsync(id);
        if (group == null)
            throw new KeyNotFoundException("Content Group not found.");

        return new ContentGroupViewModel
        {
            Id = group.Id,
            GroupName = group.GroupName,
            Description = group.Description
        };
    }

    public async Task UpdateContentGroupAsync(ContentGroupDto groupDto)
    {
        ArgumentNullException.ThrowIfNull(groupDto);

        var group = await _contentGroups.FindAsync(groupDto.Id);
        if (group == null)
            throw new KeyNotFoundException("Content Group not found.");

        group.Update(groupDto.GroupName, groupDto.Description);
        _contentGroups.Update(group);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteContentGroupAsync(int id)
    {
        var group = await _contentGroups.FindAsync(id);
        if (group == null)
            throw new KeyNotFoundException("Content Group not found.");

        _contentGroups.Remove(group);
        await _uow.SaveChangesAsync();
    }
}
