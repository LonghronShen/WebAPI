using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Data.User;
using WebApi.ViewModel;

namespace WebApi.Controllers
{

    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController
        : BaseController
    {

        /// <summary>
        /// 提供在持久化存储中管理角色的API
        /// </summary>
        private RoleManager<IdentityRole> RoleManager { get; }

        public AccountController(APIDbContext dbContext, RoleManager<IdentityRole> RoleManager, UserManager<ApplicationUser> UserManager, SignInManager<ApplicationUser> signInManager)
            : base(dbContext, signInManager, UserManager)
        {
            this.RoleManager = RoleManager;
        }

        // GET: api/Account
        /// <summary>
        /// 获取登陆用户的信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var id = await GetCurentUserId();
            var user = _dbContext.Users.Where(x => x.Id == id).FirstOrDefault();
            if (user == null)
            {
                return NotFound(new { error = string.Format("用户没用登陆") });
            }
            else
            {
                return this.Json(new UserViewModel
                {
                    UserName = user.UserName,
                    UserPwd = user.PasswordHash
                });
            }
        }

        [HttpPost("LogOut")]
        public IActionResult LogOut()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                this.SignInManager.SignOutAsync().Wait();
            }
            return Ok();
        }

    }

}