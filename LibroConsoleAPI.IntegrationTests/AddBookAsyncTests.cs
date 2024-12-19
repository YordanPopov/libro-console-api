using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
        Book newBook;

        public AddBookAsyncTests()
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
        public async Task AddBookAsync_ShouldAddBook()
        {
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

            newBook.Title = title;

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));
            var bookInDb = await _dbContext.Books.AnyAsync();

            Assert.Equal("Book is invalid.", exception.Message);
            Assert.False(bookInDb);
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

            newBook.Author = author;

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));
            var booksInDb = await _dbContext.Books.AnyAsync();

            Assert.Equal("Book is invalid.", exception.Message);
            Assert.False(booksInDb);
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
            newBook.ISBN = isbn;

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));
            var bookInDb = await _dbContext.Books.AnyAsync();

            Assert.Equal("Book is invalid.", exception.Message);
            Assert.False(bookInDb);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(1699)]
        [InlineData(2025)]
        [InlineData(-2021)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public async Task TryToAddBookWith_InvalidYearPublished_ShouldThrowException(int yearPublished)
        {
            newBook.YearPublished = yearPublished;

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));
            var bookInDb = await _dbContext.Books.AnyAsync();

            Assert.Equal("Book is invalid.", exception.Message);
            Assert.False(bookInDb);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("c")]
        public async Task TryToAddBookWith_InvalidGenre_ShouldThrowException(string genre)
        {
            if (genre == "c")
                genre = new string('c', 51);
            
            newBook.Genre = genre;

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));
            var bookInDb = await _dbContext.Books.AnyAsync();

            Assert.Equal("Book is invalid.", exception.Message);
            Assert.False(bookInDb);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        //TODO: int.MaxValue + 1
        public async Task TryToAddBookWith_InvalidPages_ShouldThrowException(int pages)
        {
            newBook.Pages = pages;

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));
            var bookInDb = await _dbContext.Books.AnyAsync();

            Assert.Equal("Book is invalid.", exception.Message);
            Assert.False(bookInDb);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0.0099)]
        [InlineData(-1)]
        [InlineData(double.MinValue)]
        public async Task TryToAddBookWith_InvalidPrice_ShouldThrowException(double price)
        {
            
            newBook.Price = price;

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));
            var bookInDb = await _dbContext.Books.AnyAsync();

            Assert.Equal("Book is invalid.", exception.Message);
            Assert.False(bookInDb);
        }
    }
}
