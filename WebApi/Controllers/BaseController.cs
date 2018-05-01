using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Data;
using WebApi.Data.User;
using WebApi.ViewModel;

namespace WebApi.Controllers
{

    public class BaseController
        : Controller
    {

        protected readonly APIDbContext _dbContext;

        protected SignInManager<ApplicationUser> SignInManager { get; }

        protected UserManager<ApplicationUser> UserManager { get; }

        protected JsonSerializerSettings DefaultJsonSettings
        {
            get
            {
                return JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                };
            }
        }

        public BaseController(APIDbContext dbContext, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> user)
        {
            this._dbContext = dbContext;
            this.SignInManager = signInManager;
            this.UserManager = user;
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        /// <returns></returns>
        protected async Task<string> GetCurentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                return "";

            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return User.FindFirst(ClaimTypes.NameIdentifier).Value;
            else
            {
                var userInfo = await UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (userInfo == null)
                    return "";
                return userInfo.Id;
            }
        }

    }

}
