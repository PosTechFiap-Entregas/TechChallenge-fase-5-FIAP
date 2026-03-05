using FiapX.Application.DTOs;
using FiapX.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace FiapX.Application.Tests.Validators;

public class UploadVideoRequestValidatorTests
{
    private readonly UploadVideoRequestValidator _validator;

    public UploadVideoRequestValidatorTests()
    {
        _validator = new UploadVideoRequestValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveErrors()
    {
        var request = new UploadVideoRequest
        {
            FileName = "video.mp4",
            FileSize = 1024 * 1024,
            FileStream = new MemoryStream(),
            UserId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyFileName_ShouldHaveError(string fileName)
    {
        var request = new UploadVideoRequest
        {
            FileName = fileName,
            FileSize = 1024,
            FileStream = new MemoryStream(),
            UserId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("Nome do arquivo é obrigatório.");
    }

    [Fact]
    public void Validate_WithNullFileName_ShouldHaveError()
    {
        var request = new UploadVideoRequest
        {
            FileName = null!,
            FileSize = 1024,
            FileStream = new MemoryStream(),
            UserId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Theory]
    [InlineData("video.txt")]
    [InlineData("video.pdf")]
    [InlineData("video.exe")]
    [InlineData("video.doc")]
    public void Validate_WithInvalidExtension_ShouldHaveError(string fileName)
    {
        var request = new UploadVideoRequest
        {
            FileName = fileName,
            FileSize = 1024,
            FileStream = new MemoryStream(),
            UserId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("Formato de arquivo não suportado. Use: mp4, avi, mov, mkv, wmv, flv, webm, m4v.");
    }

    [Theory]
    [InlineData("video.mp4")]
    [InlineData("video.avi")]
    [InlineData("video.mov")]
    [InlineData("video.mkv")]
    [InlineData("video.wmv")]
    [InlineData("video.MP4")]
    public void Validate_WithValidExtensions_ShouldNotHaveError(string fileName)
    {
        var request = new UploadVideoRequest
        {
            FileName = fileName,
            FileSize = 1024,
            FileStream = new MemoryStream(),
            UserId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithZeroOrNegativeFileSize_ShouldHaveError(long fileSize)
    {
        var request = new UploadVideoRequest
        {
            FileName = "video.mp4",
            FileSize = fileSize,
            FileStream = new MemoryStream(),
            UserId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FileSize)
            .WithErrorMessage("Arquivo não pode estar vazio.");
    }

    [Fact]
    public void Validate_WithFileSizeExceeding2GB_ShouldHaveError()
    {
        var request = new UploadVideoRequest
        {
            FileName = "video.mp4",
            FileSize = 3L * 1024 * 1024 * 1024,
            FileStream = new MemoryStream(),
            UserId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FileSize)
            .WithErrorMessage("Tamanho máximo permitido é 2048MB (2GB).");
    }

    [Fact]
    public void Validate_WithNullFileStream_ShouldHaveError()
    {
        var request = new UploadVideoRequest
        {
            FileName = "video.mp4",
            FileSize = 1024,
            FileStream = null!,
            UserId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FileStream)
            .WithErrorMessage("Stream do arquivo é obrigatório.");
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        var request = new UploadVideoRequest
        {
            FileName = "video.mp4",
            FileSize = 1024,
            FileStream = new MemoryStream(),
            UserId = Guid.Empty
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("ID do usuário é obrigatório.");
    }
}