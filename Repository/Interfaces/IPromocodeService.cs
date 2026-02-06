using Repository.Model;

namespace Repository.service
{
    public interface IPromocodeService
    {
        Task<bool> SendPromocodeAsync(t_promocode promocode);
        Task<bool> VerifyPromocodeAsync(string promocode);
        Task<bool> SendPromoAsync(string promo);
        Task<bool> VerifyPromoAsync(string promo);
        
    }
}
