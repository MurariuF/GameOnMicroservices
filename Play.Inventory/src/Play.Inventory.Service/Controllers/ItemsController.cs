using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> inventoryItemsRepository;
        private readonly IRepository<CatalogItem> catalogItemsRepository;

        public ItemsController(IRepository<InventoryItem> itemsRepository, IRepository<CatalogItem> catalogItemsRepository)
        {
            this.inventoryItemsRepository = itemsRepository;
            this.catalogItemsRepository = catalogItemsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var invetoryItemEntities = await inventoryItemsRepository.GetAllAsync(item => item.UserId == userId);
            var itemIds = invetoryItemEntities.Select(item => item.CatalogItemId);
            var catalogItemEntities = await catalogItemsRepository.GetAllAsync(item => itemIds.Contains(item.Id));

            var inventoryItemsDtos = invetoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemsDtos);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grandItemsDto)
        {
            var invetoryItem = await inventoryItemsRepository.GetAsync(item => item.UserId == grandItemsDto.UserId
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

                await inventoryItemsRepository.CreateAsync(invetoryItem);
            }

            else
            {
                invetoryItem.Quantity += grandItemsDto.Quantity;
                await inventoryItemsRepository.UpdateAsync(invetoryItem);
            }

            return Ok();
        }

    }
}