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
    public class DeleteBookAsyncTests : IClassFixture<BookManagerFixture>
    {
        private readonly BookManagerFixture _fixture;
        private readonly IBookManager _bookManager;
        private readonly TestLibroDbContext _dbContext;
        Book newBook;

        public DeleteBookAsyncTests()
        {
            _fixture = new BookManagerFixture();
            _bookManager = _fixture.BookManager;
            _dbContext = _fixture.DbContext;
            newBook = new Book
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };
        }

        [Fact]
        public async Task DeleteBookAsync_WithValidISBN_ShouldRemoveBookFromDb()
        {
            await _bookManager.AddAsync(newBook);

            await _bookManager.DeleteAsync(newBook.ISBN);

            var booksInDb = _dbContext.Books.FirstOrDefault();
            Assert.Null(booksInDb);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookManager.GetAllAsync());
            Assert.Equal("No books found.", exception.Message);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("")]
        public async Task TryToDeleteBookWith_NullOrWhiteSpaceISBN_ShouldThrowException(string isbn)
        {
            await _bookManager.AddAsync(newBook);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookManager.DeleteAsync(isbn));
            Assert.Equal("ISBN cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task TryToDeleteBookWith_NonExistentISBN_ShouldThrowException()
        {
            string nonExsitentISBN = "1112223334447";
            await _bookManager.AddAsync(newBook);
            
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _bookManager.DeleteAsync(nonExsitentISBN));

            Assert.Equal("Value cannot be null. (Parameter 'entity')", exception.Message);
        }
    }
}
