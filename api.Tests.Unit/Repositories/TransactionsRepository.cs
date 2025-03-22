using api.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace api.Tests.Unit.Repositories
{
    public class TransactionsRepository : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ITestOutputHelper _output;
        public TransactionsRepository(ITestOutputHelper output)
        {

            Console.WriteLine("constructor");
            _output = output;
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
        }
        [Fact]
        public void Test1()
        {
            Console.WriteLine("testing 1");
        }

        [Fact]
        public void Test2()
        {
            Console.WriteLine("testing 2");
        }

        public void Dispose()
        {
            Console.WriteLine("Hello dispose");
            _context.Dispose();
        }
    }
}
