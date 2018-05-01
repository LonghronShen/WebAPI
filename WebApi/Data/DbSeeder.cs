using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data.User;

namespace WebApi.Data
{

    public class DbSeeder
    {

        private readonly APIDbContext _dbContext;

        public DbSeeder(APIDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task SeedAsync()
        {
            //确定数据库存在，如果存在不做任何操作，如果不存在，则创建，如果存在，要确定与数据库模型兼容
            await _dbContext.Database.EnsureCreatedAsync();
        }

    }

}
