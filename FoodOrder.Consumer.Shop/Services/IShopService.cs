using FoodOrder.Shared;

namespace FoodOrder.Consumer.Shop.Services
{
    public interface IShopService
    {
        Task<List<FoodOrderMessage>> ReceiveMessages(string shopId);
        Task DeleteMessage(string shopId, string receiptHandle);
    }
}
