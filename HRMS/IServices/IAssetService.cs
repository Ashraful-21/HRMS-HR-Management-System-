using HRMS.Dtos.Assets;
using HRMS.Dtos.Common;

namespace HRMS.IServices;

public interface IAssetService
{
    // Categories
    Task<ApiResponse<List<AssetCategoryDto>>> GetCategoriesAsync();
    Task<ApiResponse<AssetCategoryDto>> CreateCategoryAsync(CreateAssetCategoryRequest request, int? userId);

    // Assets
    Task<ApiResponse<PagedResult<AssetDto>>> GetAssetsAsync(PagedQuery query, int? categoryId = null, string? status = null);
    Task<ApiResponse<AssetDto>> GetAssetByIdAsync(int id);
    Task<ApiResponse<AssetDto>> CreateAssetAsync(CreateAssetRequest request, int? userId);
    Task<ApiResponse<AssetDto>> UpdateAssetAsync(int id, UpdateAssetRequest request, int? userId);
    Task<ApiResponse> DeleteAssetAsync(int id, int? userId);

    // Assignments
    Task<ApiResponse<List<AssetAssignmentDto>>> GetAssignmentsAsync(int? assetId = null, int? employeeId = null);
    Task<ApiResponse<AssetAssignmentDto>> AssignAssetAsync(AssignAssetRequest request, int? userId);
    Task<ApiResponse<AssetAssignmentDto>> ReturnAssetAsync(int assignmentId, ReturnAssetRequest request, int? userId);
}
