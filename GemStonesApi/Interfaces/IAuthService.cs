using GemStonesApi.ViewModels;

namespace GemStonesApi.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseVM> RegisterAsync(RegisterVM viewModel);
        Task<AuthResponseVM> LoginAsync(LoginVM viewModel);
    }
}