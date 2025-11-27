using Microsoft.AspNetCore.Components.Forms;

namespace DocumentApprovalSystem.Services
{
    public interface IFileValidationService
    {
        Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IBrowserFile file);
    }
}
