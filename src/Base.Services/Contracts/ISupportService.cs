using Base.ViewModels;
using Base.ViewModels.Dto.Support;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViewModels.Dto;

namespace Services.Contracts;

public interface ISupportService
{
    Task<int> CreateSupportRequestAsync(SupportRequestDto supportRequestDto);
    Task RespondToSupportRequestAsync(int requestId, SupportResponseDto responseDto);
    Task<PagedResponse<List<SupportRequestViewModel>>> GetSupportRequestsAsync(SupportFilterDto filterDto);
    Task<SupportRequestDetailViewModel> GetSupportRequestByIdAsync(int id);
    Task<List<SupportRequestDetailViewModel>> GetUserSupportRequestsAsync();
}
