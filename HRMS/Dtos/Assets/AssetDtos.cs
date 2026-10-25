namespace HRMS.Dtos.Assets;

public class AssetCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int AssetsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAssetCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class AssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string Condition { get; set; } = "Good"; // New, Good, Fair, Damaged
    public string Status { get; set; } = "Available"; // Available, Assigned, Maintenance, Retired
    public string? Notes { get; set; }
    public int? CurrentAssigneeId { get; set; }
    public string? CurrentAssigneeName { get; set; }
    public DateOnly? AssignedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAssetRequest
{
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? SerialNumber { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string Condition { get; set; } = "Good";
    public string? Notes { get; set; }
}

public class UpdateAssetRequest
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? SerialNumber { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string Condition { get; set; } = "Good";
    public string Status { get; set; } = "Available";
    public string? Notes { get; set; }
}

public class AssetAssignmentDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateOnly AssignedDate { get; set; }
    public DateOnly? ReturnedDate { get; set; }
    public string Condition { get; set; } = "Good";
    public string? ReturnCondition { get; set; }
    public string? Notes { get; set; }
    public bool IsReturned { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssignAssetRequest
{
    public int AssetId { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly AssignedDate { get; set; }
    public string Condition { get; set; } = "Good";
    public string? Notes { get; set; }
}

public class ReturnAssetRequest
{
    public DateOnly ReturnedDate { get; set; }
    public string ReturnCondition { get; set; } = "Good";
    public string? Notes { get; set; }
}
