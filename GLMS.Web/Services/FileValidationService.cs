using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services;

public class FileValidationService : IFileValidationService
{
    private const int MaxFileSizeBytes = 10 * 1024 * 1024;

    public (bool IsValid, string ErrorMessage) ValidatePdfFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "No file uploaded.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
        {
            return (false, $"Only .pdf files are allowed. Uploaded: {extension}");
        }

        if (file.ContentType.ToLowerInvariant() != "application/pdf")
        {
            return (false, $"Invalid file type. Expected PDF. Received: {file.ContentType}");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, $"File size exceeds {MaxFileSizeBytes / 1024 / 1024}MB limit.");
        }

        return (true, string.Empty);
    }
}