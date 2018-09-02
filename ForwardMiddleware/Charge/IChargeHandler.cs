namespace ForwardMiddleware.Charge
{    
    using System.Threading.Tasks;
    
    public interface IChargeHandler
    {
        bool IsNeedToCharged { get; }

        Task DoChargeAsync(ApiModel apiModel,string json);
    }
}
