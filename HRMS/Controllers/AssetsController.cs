using HRMS.Auth;
using HRMS.Constants;
using HRMS.Dtos.Assets;
using HRMS.Dtos.Common;
using HRMS.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers;

[ApiController]
[Route("api/v1/assets")]
[Authorize]
[Produces("application/json")]
public class AssetsController(IAssetService service, ICurrentUserService currentUser) : ControllerBase
{
    // === CATEGORIES ===
    [HttpGet("categories")]
    [HasPermission(Permissions.AssetsRead)]
    public async Task<IActionResult> GetCategories()
    {
        var result = await service.GetCategoriesAsync();
        return Ok(result);
    }

    [HttpPost("categories")]
    [HasPermission(Permissions.AssetsWrite)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateAssetCategoryRequest request)
    {
        var result = await service.CreateCategoryAsync(request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // === ASSETS INVENTORY ===
    [HttpGet]
    [HasPermission(Permissions.AssetsRead)]
    public async Task<IActionResult> GetAssets([FromQuery] PagedQuery query, [FromQuery] int? categoryId, [FromQuery] string? status)
    {
        var result = await service.GetAssetsAsync(query, categoryId, status);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.AssetsRead)]
    public async Task<IActionResult> GetAsset(int id)
    {
        var result = await service.GetAssetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.AssetsWrite)]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetRequest request)
    {
        var result = await service.CreateAssetAsync(request, currentUser.UserId);
        return result.Success ? CreatedAtAction(nameof(GetAsset), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.AssetsWrite)]
    public async Task<IActionResult> UpdateAsset(int id, [FromBody] UpdateAssetRequest request)
    {
        var result = await service.UpdateAssetAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.AssetsWrite)]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var result = await service.DeleteAssetAsync(id, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // === ASSIGNMENTS ===
    [HttpGet("assignments")]
    [HasPermission(Permissions.AssetsRead)]
    public async Task<IActionResult> GetAssignments([FromQuery] int? assetId, [FromQuery] int? employeeId)
    {
        var result = await service.GetAssignmentsAsync(assetId, employeeId);
        return Ok(result);
    }

    [HttpPost("assignments")]
    [HasPermission(Permissions.AssetsWrite)]
    public async Task<IActionResult> AssignAsset([FromBody] AssignAssetRequest request)
    {
        var result = await service.AssignAssetAsync(request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("assignments/{id:int}/return")]
    [HasPermission(Permissions.AssetsWrite)]
    public async Task<IActionResult> ReturnAsset(int id, [FromBody] ReturnAssetRequest request)
    {
        var result = await service.ReturnAssetAsync(id, request, currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
