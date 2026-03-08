using FoodOrder.Consumer.Shop.Repositories;
using FoodOrder.Shared;

namespace FoodOrder.Consumer.Shop.Services
{
    public class ShopService : IShopService
    {
        private readonly IShopRepository shopRepository;

        public ShopService(IShopRepository shopRepository)
        {
            this.shopRepository = shopRepository;
        }

        public Task DeleteMessage(string shopId, string receiptHandle)
        {
            return shopRepository.DeleteMessage(shopId, receiptHandle);
        }

        public Task<List<FoodOrderMessage>> ReceiveMessages(string shopId)
        {
            return shopRepository.ReceiveMessages(shopId);
        }
    }
}
