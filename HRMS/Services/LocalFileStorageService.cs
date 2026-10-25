using HRMS.IServices;

namespace HRMS.Services;

public class LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContext) : IFileStorageService
{
    public async Task<(string fileUrl, string publicId)> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var targetDir = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(targetDir);

        var publicId = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{publicId}{extension}";
        var filePath = Path.Combine(targetDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        var req = httpContext.HttpContext?.Request;
        var host = req != null ? $"{req.Scheme}://{req.Host}" : "http://localhost:5219";
        var fileUrl = $"{host}/uploads/{folder}/{fileName}";

        return (fileUrl, publicId);
    }

    public Task DeleteAsync(string publicId, CancellationToken ct = default)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var uploadsDir = Path.Combine(webRoot, "uploads");

        if (Directory.Exists(uploadsDir))
        {
            var files = Directory.GetFiles(uploadsDir, $"{publicId}.*", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                try { File.Delete(f); } catch { /* Ignore delete locks */ }
            }
        }

        return Task.CompletedTask;
    }
}
