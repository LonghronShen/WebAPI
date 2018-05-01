using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WebApi.Common;
using WebApi.Data;
using WebApi.Data.User;
using WebApi.ViewModel;

namespace WebApi.Middlewares
{

    //中间件是应用程序管道中的一个组件，用来拦截请求过程进行一些其他处理和响应。
    //每个中间件可以对管道中的请求进行拦截，并可以决定是否将请求转移给下一个中间件。
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class JWTMiddleware
    {

        public const string TokenEndPoint = "/api/connect/token";

        //通过HTTP请求的方法（请求委托）
        private readonly RequestDelegate _next;
        private readonly TokenHelper _tokenHelper;
        private readonly IServiceProvider _serviceProvider;

        public JWTMiddleware(RequestDelegate next, IServiceProvider serviceProvider, TokenHelper tokenHelper)
        {
            this._next = next;
            this._serviceProvider = serviceProvider;
            this._tokenHelper = tokenHelper;
        }

        public Task Invoke(HttpContext httpContext)
        {
            //请求路径是否匹配TokenEndPoint，如果是的话，执行下个请求方法
            if (httpContext.Request.Path.Equals(TokenEndPoint, StringComparison.OrdinalIgnoreCase))
                return _next(httpContext);
            //请求方式是否为post，请求的内容类型是否为form类型（application/x-www-form-urlencoded）
            if (httpContext.Request.Method.Equals("POST", StringComparison.CurrentCultureIgnoreCase) && httpContext.Request.HasFormContentType)
                return Login(httpContext);
            if (!httpContext.Request.Path.Equals(TokenEndPoint, StringComparison.OrdinalIgnoreCase))
            {
                var header = httpContext.Request.Headers["Authorization"];
                return _next(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = 400;
                return httpContext.Response.WriteAsync("Bad Request.");
            }
        }

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task Login(HttpContext httpContext)
        {
            ApplicationUser user = null;
            ResultViewModel resultModel = new ResultViewModel();
            try
            {
                string userName = httpContext.Request.Form["userName"];
                string psw = httpContext.Request.Form["userPwd"];
                using (var scope = _serviceProvider.CreateScope())
                {
                    using (var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>())
                    {
                        user = await userManager.FindByNameAsync(userName);
                        if (user == null && userName.Contains("@"))
                        {
                            user = await userManager.FindByEmailAsync(userName);
                        }
                        //请求中的密码是否和指定用户的密码一致
                        var succeeded = user != null && await userManager.CheckPasswordAsync(user, psw);
                        if (succeeded)
                        {
                            var signInManager = scope.ServiceProvider.GetService<SignInManager<ApplicationUser>>();
                            await signInManager.SignInAsync(user, true);
                            var jwt = this._tokenHelper.CreateJWTToken(user.Id);
                            resultModel.IsSuccess = true;
                            resultModel.Token = JsonConvert.SerializeObject(jwt); ;
                        }
                        else
                        {
                            resultModel.IsSuccess = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultModel.IsSuccess = false;
                resultModel.Message = ex.Message;
            }
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(resultModel));
            return;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class JWTMiddlewareExtensions
    {

        //IApplicationBuilder的扩展方法
        public static IApplicationBuilder UseJWTMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JWTMiddleware>();
        }

    }

}
