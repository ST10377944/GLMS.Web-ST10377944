using GLMS.Web.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace GLMS.Tests;

public class FileValidationServiceTests
{
    private readonly FileValidationService _service = new();

    [Fact]
    public void ValidatePdfFile_ValidPdf_ReturnsValid()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("contract.pdf");
        mockFile.Setup(f => f.Length).Returns(1000);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        var result = _service.ValidatePdfFile(mockFile.Object);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidatePdfFile_ExeFile_ReturnsError()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("virus.exe");
        mockFile.Setup(f => f.Length).Returns(1000);
        mockFile.Setup(f => f.ContentType).Returns("application/x-msdownload");

        var result = _service.ValidatePdfFile(mockFile.Object);

        Assert.False(result.IsValid);
        Assert.Contains("Only .pdf", result.ErrorMessage);
    }

    [Fact]
    public void ValidatePdfFile_NullFile_ReturnsError()
    {
        var result = _service.ValidatePdfFile(null!);
        Assert.False(result.IsValid);
        Assert.Equal("No file uploaded.", result.ErrorMessage);
    }

    [Fact]
    public void ValidatePdfFile_FileTooLarge_ReturnsError()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("large.pdf");
        mockFile.Setup(f => f.Length).Returns(11 * 1024 * 1024);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        var result = _service.ValidatePdfFile(mockFile.Object);

        Assert.False(result.IsValid);
        Assert.Contains("exceeds", result.ErrorMessage);
    }
}