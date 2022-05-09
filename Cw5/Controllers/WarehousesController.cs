using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Cw5
{
	[Route("[controller]")]
	[ApiController]
	public class WarehousesController : ControllerBase
	{
        private readonly IDatabaseService _databaseService;
        public WarehousesController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterProduct(ProductMetadata productMetadata)
        {
            if (!await _databaseService.DoesProductExist(productMetadata.IdProduct))
            {
                return NotFound("Product does not exist");
            }
            var idOrder = await _databaseService.GetOrderId(productMetadata.IdProduct, productMetadata.Amount, DateTime.Now);
            if (idOrder == -1)
            {
                return NotFound("Order does not exist");
            }
            if (await _databaseService.HasOrderBeenProcessed(idOrder))
            {
                return UnprocessableEntity("Order has been already processed");
            }
            var primaryKey = await _databaseService.RegisterProduct(productMetadata, idOrder);
            if (primaryKey == -1)
            {
                return Problem("Failed to register product");
            }
            return Ok(primaryKey);
        }
    }
}

