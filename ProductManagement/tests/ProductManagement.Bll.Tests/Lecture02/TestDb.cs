using Microsoft.EntityFrameworkCore;
using ProductManagement.Dal;

namespace ProductManagement.Bll.Tests.Lecture02;

internal static class TestDb
{
    /// <summary>A fresh in-memory database with the seed data. Every test gets its own, so tests cannot affect each other.</summary>
    public static ProductManagementDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ProductManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ProductManagementDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}
