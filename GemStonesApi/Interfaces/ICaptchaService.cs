namespace GemStonesApi.Interfaces
{
    public interface ICaptchaService
    {
        Task<bool> ValidateAsync(string token);
    }
}