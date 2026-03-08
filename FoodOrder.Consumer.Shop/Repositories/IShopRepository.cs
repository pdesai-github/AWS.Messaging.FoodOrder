using FoodOrder.Shared;

namespace FoodOrder.Consumer.Shop.Repositories
{
    public interface IShopRepository
    {
        Task<List<FoodOrderMessage>> ReceiveMessages(string shopId);
        Task DeleteMessage(string shopId, string receiptHandle);
    }
}
