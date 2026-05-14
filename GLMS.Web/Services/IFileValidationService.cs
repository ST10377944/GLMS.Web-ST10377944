using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services;

public interface IFileValidationService
{
    (bool IsValid, string ErrorMessage) ValidatePdfFile(IFormFile file);
}