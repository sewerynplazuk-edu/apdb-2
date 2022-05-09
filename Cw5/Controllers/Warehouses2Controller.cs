using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Cw5
{
    [Route("[controller]")]
    public class Warehouses2Controller : Controller
    {
        private readonly IDatabaseService _databaseService;
        public Warehouses2Controller(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet]
        public IActionResult GetWarehouses()
        {
            return Ok(new ProductMetadata
            {
                IdProduct = 1,
                IdWarehouse = 1,
                Amount = 200,
                CreatedAt = DateTime.Now
            });
        }

        [HttpPost]
        public async Task<IActionResult> RegisterProduct([FromBody] ProductMetadata productMetadata)
        {
            var primaryKey = await _databaseService.RegisterProductViaProcedure(productMetadata);
            if (primaryKey == -1)
            {
                return Problem("Failed to register product");
            }
            return Ok(primaryKey);
        }
    }
}

