using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cw4.Services;
using Cw4.Models;

namespace Cw4
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
        public async Task<IActionResult> RegisterProduct([FromBody] SomeKindOfProduct productDTO)
        {
            if (!await _databaseService.DoesProductExist(productDTO.IdProduct))
            {
                return NotFound("Could not find product for given IdProduct");
            }

            var order = await _databaseService.GetOrder(productDTO.IdProduct, productDTO.Amount, productDTO.CreatedAt);
            if (order == null)
            {
                return NotFound("Could not find an order for given IdProduct, Amount and CreatedAt parameters");
            }

            var shouldProcessOrder = !await _databaseService.HasOrderBeenProcessed(order.IdOrder) ?? false;
            if (!shouldProcessOrder)
            {
                return UnprocessableEntity("Order has been already processed");
            }

            var productId = await _databaseService.RegisterProduct(productDTO, order.IdOrder);
            if (productId == -1)
            {
                return BadRequest("Failed to register product");
            }
            return Ok(productId);
        }
    }
}


