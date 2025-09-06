using API.Domain.DTOs;
using API.Domain.Request.AccountRequest;
using API.Request;
using Microsoft.AspNetCore.Http;

namespace API.Domain.Service.IService
{
    public interface IAccountService
    {
        Task<List<AccountWithPasswordDto>> CreateAccountsAsync(List<CreateAccountRequest> request);
        Task<ImportResult> ImportAccountsFromExcelAsync(IFormFile file);

        Task<AccountDto?> UpdateAccountAsync(Guid id, UpdateAccountRequest request);
        Task<List<AccountDto>> GetAllAccountsAsync();
        Task<AccountDto?> GetByIdAsync(Guid id);
        Task<AccountDto?> GetByPhoneNumberAsync(string phoneNumber);
        Task<bool> ToggleActiveStatusAsync(Guid id);
        Task<bool> SetActiveStatusesAsync(List<SetActiveStatusRequest> requests);
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<bool> UpdateProfileAsync(Guid id, UpdateProfileRequest request);
    }
}