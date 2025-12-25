
using Base.Common.Features.Identity;

namespace Services.Contracts;

public interface IApiActionsDiscoveryService
{
    IReadOnlyList<ApiControllerDto> DynamicallySecuredActions { get; }
}