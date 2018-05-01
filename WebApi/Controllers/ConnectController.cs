using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Common;
using WebApi.Data;
using WebApi.Data.User;
using WebApi.ViewModel;

namespace WebApi.Controllers
{

    [Produces("application/json")]
    [Route("api/Connect")]
    public class ConnectController
        : BaseController
    {

        private readonly TokenHelper _tokenHelper;

        public ConnectController(APIDbContext dbContext, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> user, TokenHelper TokenHelper)
            : base(dbContext, signInManager, user)
        {
            this._tokenHelper = TokenHelper;
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="usrvm"></param>
        /// <returns></returns>
        [HttpPost("token")]
        [EnableCors("any")]
        public async Task<IActionResult> PostToken([FromBody] UserViewModel usrvm)
        {
            if (usrvm == null)
            {
                return new StatusCodeResult(500);
            }
            else
            {
                var resultModel = new ResultViewModel();
                try
                {
                    var user = await this.UserManager.FindByNameAsync(usrvm.UserName) ??
                        await this.UserManager.FindByEmailAsync(usrvm.UserName);
                    var isExist = user != null;
                    if (isExist)
                    {
                        resultModel.IsSuccess = false; resultModel.Message = "已存在此账号";
                    }
                    else
                    {
                        var CreateDate = DateTime.Now;
                        user = new ApplicationUser()
                        {
                            UserName = usrvm.UserName,
                            PasswordHash = usrvm.UserPwd,
                            CreateDate = CreateDate,
                            LastModifiedDate = CreateDate,
                            EmailConfirmed = false,
                            LockoutEnabled = false,
                        };

                        var result = await this.UserManager.CreateAsync(user, usrvm.UserPwd);
                        while (true)
                        {
                            if (result.Succeeded)
                            {
                                result = await this.UserManager.AddToRoleAsync(user, "Registered");
                                if (result.Succeeded)
                                {
                                    var jwt = _tokenHelper.CreateJWTToken(user.Id);
                                    string tokenJson = JsonConvert.SerializeObject(jwt);
                                    resultModel.IsSuccess = true;
                                    resultModel.Token = tokenJson;
                                    break;
                                }
                            }
                            resultModel.IsSuccess = false;
                            resultModel.Message = result.Errors.FirstOrDefault().Description;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    resultModel.IsSuccess = false;
                    resultModel.Message = e.Message;
                }
                return this.Json(resultModel, DefaultJsonSettings);
            }
        }

        [HttpPost("POST")]
        [EnableCors("any")]
        public async Task<IActionResult> Connect(string userName, string userPwd, string grant_type, string client_id, string scope, int count = 1)
        {
            var resultModel = new ResultViewModel { IsSuccess = false, Message = "登陆失败" }; ;
            //var isPersistent = rememnerMe == 1 ? true : false;
            var isPersistent = true;
            try
            {
                var signResult = await this.SignInManager.PasswordSignInAsync(userName, userPwd, isPersistent, count > 3);
                if (signResult.Succeeded)
                {
                    var jwt = this._tokenHelper.CreateJWTToken(await GetCurentUserId());
                    var tokenJson = JsonConvert.SerializeObject(jwt);
                    resultModel = new ResultViewModel { IsSuccess = true, Token = tokenJson };
                }
                if (signResult.IsLockedOut)
                {
                    resultModel = new ResultViewModel { IsSuccess = false, Message = "你登陆已超过3次，该账户已被锁" };
                }
            }
            catch (Exception e)
            {
                resultModel = new ResultViewModel { IsSuccess = false, Message = e.Message };
            }
            return this.Json(resultModel, DefaultJsonSettings);
        }

    }

}