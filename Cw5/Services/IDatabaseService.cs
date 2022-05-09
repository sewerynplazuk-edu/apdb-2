using System;
using System.Threading.Tasks;

namespace Cw5
{
	public interface IDatabaseService
	{
		Task<bool> DoesProductExist(int idProduct);
		Task<int> GetOrderId(int idProduct, int amount, DateTime createdAt);
		Task<bool> HasOrderBeenProcessed(int idOrder);
		Task<int> RegisterProduct(ProductMetadata productMetadata, int idOrder);
		Task<int> RegisterProductViaProcedure(ProductMetadata productMetadata);
	}
}

