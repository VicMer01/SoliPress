using DocumentApprovalSystem.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace DocumentApprovalSystem.Services
{
    public class FileValidationService : IFileValidationService
    {
        private readonly FileSettings _settings;
        private readonly ILogger<FileValidationService> _logger;

        // Magic numbers for file signatures
        private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
        {
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".docx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } } // ZIP signature
        };

        public FileValidationService(IOptionsSnapshot<FileSettings> settings, ILogger<FileValidationService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IBrowserFile file)
        {
            try
            {
                // 1. Size Validation
                var maxSizeBytes = _settings.MaxFileSizeInMB * 1024 * 1024;
                if (file.Size > maxSizeBytes)
                {
                    return (false, $"El archivo excede el tamaño máximo permitido de {_settings.MaxFileSizeInMB} MB.");
                }

                // 2. Extension Validation
                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                if (!_settings.AllowedExtensions.Contains(extension))
                {
                    return (false, $"Tipo de archivo no permitido. Permitidos: {string.Join(", ", _settings.AllowedExtensions)}");
                }

                // 3. Magic Number Validation (Content Inspection)
                if (_fileSignatures.ContainsKey(extension))
                {
                    using var stream = file.OpenReadStream(maxSizeBytes); // Use max size limit for safety
                    using var memoryStream = new MemoryStream();
                    
                    // Read only the header bytes needed
                    var headerBytes = new byte[8];
                    await stream.ReadAsync(headerBytes, 0, headerBytes.Length);

                    var signatures = _fileSignatures[extension];
                    bool headerMatch = false;

                    foreach (var signature in signatures)
                    {
                        if (headerBytes.Take(signature.Length).SequenceEqual(signature))
                        {
                            headerMatch = true;
                            break;
                        }
                    }

                    if (!headerMatch)
                    {
                        _logger.LogWarning($"File signature mismatch for {file.Name}. Extension: {extension}");
                        return (false, "El contenido del archivo no coincide con su extensión. El archivo podría estar corrupto o ser inseguro.");
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file");
                return (false, "Ocurrió un error al validar el archivo.");
            }
        }
    }
}
