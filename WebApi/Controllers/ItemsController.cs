using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Common;
using WebApi.Common.Interfaces;
using WebApi.Data;
using WebApi.Data.Items;
using WebApi.Data.User;
using WebApi.ViewModel;

namespace WebApi.Controllers
{

    [Authorize]
    [Produces("application/json")]
    [Route("api/Items")]
    public class ItemsController
        : Controller
    {

        private readonly APIDbContext _dbContext;
        private readonly IDTOMapper _dtoMapper;

        protected UserManager<ApplicationUser> UserManager { get; }

        protected string CurrentUserId
        {
            get
            {
                return this.UserManager.GetUserId(this.User);
            }
        }

        public ItemsController(APIDbContext DbContext, IDTOMapper mapper, UserManager<ApplicationUser> userManager)
        {
            this._dbContext = DbContext;
            this._dtoMapper = mapper;
            this.UserManager = userManager;
        }

        // GET: api/Items
        [HttpGet]
        public async Task<List<ItemsViewModel>> ListItems()
        {
            var list = await this._dbContext.Items.AsNoTracking().ToListAsync();
            var result = this._dtoMapper.AutoMapper<Item, ItemsViewModel>(list);
            return result;
        }

        // GET: api/Items/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<ItemsViewModel> GetItem(int id)
        {
            var item = await this._dbContext.Items.AsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();
            var itemViewModel = this._dtoMapper.AutoMapper<Item, ItemsViewModel>(item);
            return itemViewModel;
        }

        // POST: api/Items
        [HttpPost]
        [EnableCors("any")]
        public async Task<IActionResult> AddItem([FromBody] Item entity)
        {
            try
            {
                entity.Id = 0;
                entity.LastModifiedDate = DateTime.UtcNow;
                entity.CreatedDate = DateTime.UtcNow;
                entity.Flags = 1;
                entity.ViewCount = 1;
                entity.UserId = this.CurrentUserId;
                this._dbContext.Items.Add(entity);
                if (await this._dbContext.SaveChangesAsync() > 0)
                {
                    return this.Accepted(entity.Id);
                }
                throw new Exception("Failed to add a new item.");
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, ex.Message);
            }
        }

        // PUT: api/Items/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(int id, [FromBody] Item entity)
        {
            try
            {
                var dbEntity = await this._dbContext.Items.FindAsync(entity.Id);
                if (dbEntity == null)
                {
                    return this.NotFound($"Couldn't find the matching entity with id = {entity.Id}.");
                }
                dbEntity.LastModifiedDate = DateTime.UtcNow;
                dbEntity.Flags = entity.Flags;
                dbEntity.ViewCount = 1;
                if (await this._dbContext.SaveChangesAsync() > 0)
                {
                    return this.Accepted(entity.Id);
                }
                throw new Exception("Failed to add a new item.");
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, ex.Message);
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                var dbEntity = await this._dbContext.Items.FindAsync(id);
                if (dbEntity == null)
                {
                    return this.NotFound($"Couldn't find the matching entity with id = {id}.");
                }
                this._dbContext.Items.Remove(dbEntity);
                if (await this._dbContext.SaveChangesAsync() > 0)
                {
                    return this.Ok(id);
                }
                throw new Exception("Failed to add a new item.");
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, ex.Message);
            }
        }

    }

}