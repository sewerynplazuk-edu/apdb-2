using System;
using Cw4.Models;
namespace Cw4.Services
{
	public interface IDatabaseService
	{
		Task<bool> DoesProductExist(int idProduct);
		Task<Order?> GetOrder(int idProduct, int amount, DateTime createdAt);
		Task<bool?> HasOrderBeenProcessed(int idOrder);
		Task<int> RegisterProduct(SomeKindOfProduct productDTO, int idOrder);
		Task<int> RunRegisterProductProcedure(SomeKindOfProduct productDTO);
	}
}

