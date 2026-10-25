namespace HRMS.IServices;

public interface IFileStorageService
{
    Task<(string fileUrl, string publicId)> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
    Task DeleteAsync(string publicId, CancellationToken ct = default);
}
