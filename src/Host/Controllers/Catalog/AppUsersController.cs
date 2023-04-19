using Microsoft.AspNetCore.Http.HttpResults;
using RAFFLE.WebApi.Application.Catalog.AppUsers;
using RAFFLE.WebApi.Domain.Catalog;

namespace RAFFLE.WebApi.Host.Controllers.Catalog;

public class AppUsersController : VersionedApiController
{
    [HttpPost("search")]
    [MustHavePermission(RAFFLEAction.Search, RAFFLEResource.AppUsers)]
    [OpenApiOperation("Search application user details using available filters.", "")]
    public Task<PaginationResponse<AppUserDto>> SearchAsync(SearchAppUsersRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpPost]
    [MustHavePermission(RAFFLEAction.Create, RAFFLEResource.AppUsers)]
    [OpenApiOperation("Create required an application user details.", "")]
    public Task<Guid> CreateAsync(CreateAppUserRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpPut("{id}")]
    [MustHavePermission(RAFFLEAction.Update, RAFFLEResource.AppUsers)]
    [OpenApiOperation("Update an application user details. Input: Guid", "")]
    public async Task<ActionResult<Guid>> UpdateAsync(UpdateAppUserRequest request, Guid id)
    {
        return (id != request.Id)
            ? BadRequest()
            : Ok(await Mediator.Send(request));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(RAFFLEAction.Delete, RAFFLEResource.AppUsers)]
    [OpenApiOperation("Delete an application user details. Input: Guid", "")]
    public Task<Guid> DeleteAsync(Guid id)
    {
        return Mediator.Send(new DeleteAppUserRequest(id));
    }

    [HttpGet("application/{applicationUserId}")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.AppUsers)]
    [OpenApiOperation("Get an app user details. Input: string main's application user id", "")]
    public Task<AppUserDto> GetApplicationUserAsync(string applicationUserId)
    {
        return Mediator.Send(new GetAppUserByUserApplicationRequest(applicationUserId));
    }

    [HttpGet("{id}")]
    [MustHavePermission(RAFFLEAction.View, RAFFLEResource.AppUsers)]
    [OpenApiOperation("Get an app user details using appuser id", "")]
    public async Task<ActionResult<AppUserDto>> GetAsync(Guid id)
    {
        GetAppUserRequest? getAppUserRequest = new GetAppUserRequest(id);

        return getAppUserRequest is null ? BadRequest() : Ok(await Mediator.Send(getAppUserRequest));
    }
}