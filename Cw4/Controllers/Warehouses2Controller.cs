using System;
using Microsoft.AspNetCore.Mvc;
using Cw4.Services;
using Cw4.Models;

namespace Cw4
{
    [Route("[controller]")]
    public class Warehouses2Controller : Controller
    {
        private readonly IDatabaseService _databaseService;
        public Warehouses2Controller(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterProduct([FromBody] SomeKindOfProduct productDTO)
        {
            var productId = await _databaseService.RunRegisterProductProcedure(productDTO);
            if (productId == -1)
            {
                return BadRequest("Failed to register product");
            }
            return Ok(productId);
        }
    }
}


