using api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace api.Tests.Unit.Helpers
{
    public class TestData
    {
        public static IQueryable<Category> GetCategories(string userId, string otherUserId)
        {
            return new List<Category>
        {
            new() { Id = 1, Name = "Active", AppUserId = userId, IsActive = true },
            new() { Id = 2, Name = "Active", AppUserId = userId, IsActive = true },

            new() { Id = 3, Name = "Inactive", AppUserId = userId, IsActive = false },
            new() { Id = 4, Name = "Inactive", AppUserId = userId, IsActive = false },

            new() { Id = 5, Name = "Other user Active", AppUserId = otherUserId, IsActive = true },
            new() { Id = 6, Name = "Other user Active", AppUserId = otherUserId, IsActive = true },

            new() { Id = 7, Name = "Other user Inactive", AppUserId = otherUserId, IsActive = false },
            new() { Id = 8, Name = "Other user Inactive", AppUserId = otherUserId, IsActive = false },
        }.AsQueryable();
        }
    }
}
