using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibroConsoleAPI.IntegrationTests.XUnit
{
    public class AddBookAsyncTests : IClassFixture<BookManagerFixture>
    {
        private readonly BookManagerFixture _fixture;
        private readonly IBookManager _bookManager;
        private readonly TestLibroDbContext _dbContext;

        public AddBookAsyncTests()
        {
            _fixture = new BookManagerFixture();
            _bookManager = _fixture.BookManager;
            _dbContext = _fixture.DbContext;
        }

        [Fact]
        public async Task AddBookAsync_ShouldAddBook()
        {
            var newBook = new Book
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            await _bookManager.AddAsync(newBook);

            var bookInDb = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == newBook.ISBN);
            Assert.NotNull(bookInDb);
            Assert.Equal("Test Book", bookInDb.Title);
            Assert.Equal("John Doe", bookInDb.Author);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("c")] // 256 characters
        public async Task TryToAddBookWith_InvalidTitle_ShouldThrowException(string title)
        {
            if (title == "c")
                title = new string('c', 256);

            var newBook = new Book
            {
                Title = title,
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            var act = async () => await _bookManager.AddAsync(newBook);

            var exception = Assert.ThrowsAsync<ValidationException>(act);
            var bookInDb = await _dbContext.Books.FirstOrDefaultAsync(b => b.Title == title);

            Assert.Equal("Book is invalid.", exception.Result.Message);
            Assert.Null(bookInDb);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("c")] // 101 characters
        public async Task TryToAddBookWith_InvalidAuthor_ShouldThrowException(string author)
        {
            if (author == "c")
                author = new string('c', 101);

            var newBook = new Book
            {
                Title = "Test Book Title",
                Author = author,
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            var act = async () => await _bookManager.AddAsync(newBook);

            var exception = Assert.ThrowsAsync<ValidationException>(act);
            var booksInDb = await _dbContext.Books.FirstOrDefaultAsync(b => b.Author == author);

            Assert.Equal("Book is invalid.", exception.Result.Message);
            Assert.Null(booksInDb);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("1")]
        [InlineData("012345678911")]
        [InlineData("01234567898765")]
        [InlineData("abcdefghijklm")]
        [InlineData(" 123456123456")]
        [InlineData("123456123456 ")]
        [InlineData("123456 123456")]
        [InlineData("123456a123456")]
        [InlineData("a123456123456")]
        [InlineData("123456123456a")]
        [InlineData("?????????????")]
        public async Task TryToAddBookWith_InvalidISBN_ShouldThrowException(string isbn)
        {
            var newBook = new Book
            {
                Title = "Test book",
                Author = "John Doe",
                ISBN = isbn,
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            var act = async () => await _bookManager.AddAsync(newBook);

            var exception = Assert.ThrowsAsync<ValidationException>(act);
            var bookInDb = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);

            Assert.Equal("Book is invalid.", exception.Result.Message);
            Assert.Null(bookInDb);
        }
    }
}
