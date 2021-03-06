﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wodsoft.ComBoost.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
//using Microsoft.EntityFrameworkCore;
//using QueryableExtensions = Wodsoft.ComBoost.Data.Entity.AsyncQueryableExtensions;

namespace DataUnitTest
{
    public class CURDTest
    {
        [Fact]
        public async Task AddAndRemoveTest()
        {
            UnitTestEnvironment env = new UnitTestEnvironment();
            using (var scope = env.GetServiceScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var database = serviceProvider.GetService<IDatabaseContext>();
                var categoryContext = database.GetContext<Category>();
                Assert.Equal(0, await categoryContext.CountAsync(categoryContext.Query()));
                var category = categoryContext.Create();
                category.Name = "Test";
                categoryContext.Add(category);
                Assert.Equal(0, await categoryContext.CountAsync(categoryContext.Query()));
                await database.SaveAsync();
                Assert.Equal(1, await categoryContext.CountAsync(categoryContext.Query()));
                categoryContext.Remove(category);
                Assert.Equal(1, await categoryContext.CountAsync(categoryContext.Query()));
                await database.SaveAsync();
                Assert.Equal(0, await categoryContext.CountAsync(categoryContext.Query()));
            }
        }

        [Fact]
        public async Task ReferenceTest()
        {
            UnitTestEnvironment env = new UnitTestEnvironment();
            using (var scope = env.GetServiceScope())
            {
                var database = scope.ServiceProvider.GetService<IDatabaseContext>();
                var categoryContext = database.GetContext<Category>();
                var category = categoryContext.Create();
                category.Name = "Test";
                categoryContext.Add(category);
                var userContext = database.GetContext<User>();
                var user = userContext.Create();
                user.Username = "TestUser";
                userContext.Add(user);
                await database.SaveAsync();
            }
            using (var scope = env.GetServiceScope())
            {
                var database = scope.ServiceProvider.GetService<IDatabaseContext>();
                var userContext = database.GetContext<User>();
                var user = await userContext.FirstOrDefaultAsync(userContext.Query());
                var categoryContext = database.GetContext<Category>();
                var category = await categoryContext.FirstOrDefaultAsync(categoryContext.Query());
                user.Category = category;
                userContext.Update(user);
                //var refCategory = ((DatabaseContext)database).InnerContext.Entry(user).Reference(t => t.Category);
                await database.SaveAsync();
            }

            using (var scope = env.GetServiceScope())
            {
                var database = scope.ServiceProvider.GetService<IDatabaseContext>();
                var userContext = database.GetContext<User>();
                var user = await userContext.FirstOrDefaultAsync(userContext.Include(userContext.Query(), t => t.Category));
                Assert.NotNull(user.Category);
            }
        }

        [Fact]
        public async Task LazyLoadEntityTest()
        {
            UnitTestEnvironment env = new UnitTestEnvironment();
            using (var scope = env.GetServiceScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var database = serviceProvider.GetService<IDatabaseContext>();
                var categoryContext = database.GetContext<Category>();
                var category = categoryContext.Create();
                category.Name = "Test";
                categoryContext.Add(category);
                var userContext = database.GetContext<User>();
                var user = userContext.Create();
                user.Username = "TestUser";
                user.Category = category;
                userContext.Add(user);
                await database.SaveAsync();

                database = serviceProvider.GetService<IDatabaseContext>();
                userContext = database.GetContext<User>();
                user = await userContext.FirstOrDefaultAsync(userContext.Query());
                Assert.Null(user.Category);
                var dbContext = ((DatabaseContext)database).InnerContext;
                category = await user.LoadAsync(t => t.Category);
                Assert.NotNull(category);
                Assert.Equal("Test", category.Name);
            }
        }

        [Fact]
        public async Task LazyLoadQueryTest()
        {
            UnitTestEnvironment env = new UnitTestEnvironment();
            using (var scope = env.GetServiceScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var database = serviceProvider.GetService<IDatabaseContext>();
                var categoryContext = database.GetContext<Category>();
                var category = categoryContext.Create();
                category.Name = "Test";
                categoryContext.Add(category);
                var userContext = database.GetContext<User>();
                var user = userContext.Create();
                user.Username = "TestUser";
                user.Category = category;
                userContext.Add(user);
                await database.SaveAsync();

                database = serviceProvider.GetService<IDatabaseContext>();
                categoryContext = database.GetContext<Category>();
                category = (await categoryContext.ToArrayAsync(categoryContext.Query()))[0];
                userContext = database.GetContext<User>();
                Assert.Null(category.Users);
                var collection = await category.LoadAsync(t => t.Users);
                Assert.Equal(1, collection.Count);
                var users = await userContext.ToArrayAsync(collection);
                Assert.Equal("TestUser", users[0].Username);
            }
        }

        [Fact]
        public async Task OrderTest()
        {
            UnitTestEnvironment env = new UnitTestEnvironment();
            using (var scope = env.GetServiceScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var database = serviceProvider.GetService<IDatabaseContext>();
                await DataCommon.DataInitAsync(database);
                var userContext = database.GetContext<User>();
                var user = await userContext.FirstOrDefaultAsync(userContext.Order());
                Assert.Equal("TestUser1", user.Username);
            }
        }
    }
}
