using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> itemsRepository;

        public ItemsController(IRepository<InventoryItem> itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var items = (await itemsRepository.GetAllAsync(item => item.UserId == userId))
                        .Select(item => item.AsDto());

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grandItemsDto)
        {
            var invetoryItem = await itemsRepository.GetAsync(item => item.UserId == grandItemsDto.UserId
                    && item.CatalogItemId == grandItemsDto.CatalogItemId);

            if (invetoryItem is null)
            {
                invetoryItem = new InventoryItem
                {
                    CatalogItemId = grandItemsDto.CatalogItemId,
                    UserId = grandItemsDto.UserId,
                    Quantity = grandItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await itemsRepository.CreateAsync(invetoryItem);
            }

            else
            {
                invetoryItem.Quantity += grandItemsDto.Quantity;
                await itemsRepository.UpdateAsync(invetoryItem);
            }

            return Ok();
        }

    }
}