using LibroConsoleAPI.Business;
using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using LibroConsoleAPI.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibroConsoleAPI.IntegrationTests.XUnit
{
    public class GetAllAsyncTests : IClassFixture<BookManagerFixture>
    {
        private readonly BookManagerFixture _fixture;
        private readonly IBookManager _bookManager;
        private readonly TestLibroDbContext _dbContext;

        public GetAllAsyncTests()
        {
            _fixture = new BookManagerFixture();
            _bookManager = _fixture.BookManager;
            _dbContext = _fixture.DbContext;
        }

        [Fact]
        public async Task GetAllAsync_WhenBooksExist_ShouldReturnAllBooks()
        {
            //await DatabaseSeeder.SeedDatabaseAsync(_dbContext, _bookManager);
            var books = new Book[]
            {
                new Book
                {
                    Title = "Test Book I",
                    Author = "John Doe",
                    ISBN = "1234567890123",
                    YearPublished = 2020,
                    Genre = "Fiction",
                    Pages = 200,
                    Price = 19.99
                },
                new Book
                {
                    Title = "Test Book II",
                    Author = "John Wick",
                    ISBN = "1234567890123",
                    YearPublished = 2021,
                    Genre = "Action",
                    Pages = 100,
                    Price = 10.99
                }
            };

            foreach (var newBook in books)
            {
                await _bookManager.AddAsync(newBook);
            }

            var booksInDb = await _bookManager.GetAllAsync();
            Assert.Equal(2, booksInDb.Count());

            foreach (var book in booksInDb)
            {
                Assert.NotNull(book.Title);
                Assert.NotNull(book.Author);
                Assert.NotNull(book.ISBN);
                Assert.NotNull(book.Price);
            }
        }
         

        [Fact]
        public async Task GetAllAsync_WhenNoBooksExist_ShouldThrowKeyNotFoundException()
        {
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookManager.GetAllAsync());

            Assert.Equal("No books found.", exception.Message);
        }
    }
}
