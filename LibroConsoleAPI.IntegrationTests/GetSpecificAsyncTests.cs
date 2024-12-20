using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibroConsoleAPI.IntegrationTests.XUnit
{
    public class GetSpecificAsyncTests : IClassFixture<BookManagerFixture>
    {
        private readonly BookManagerFixture _fixture;
        private readonly IBookManager _bookManager;
        private readonly TestLibroDbContext _dbContext;
        Book[] books;

        public GetSpecificAsyncTests()
        {
            _fixture = new BookManagerFixture();
            _bookManager = _fixture.BookManager;
            _dbContext = _fixture.DbContext;
            books = new Book[]
            {
                new Book
                {
                    Title = "Test Book 1",
                    Author = "John Doe",
                    ISBN = "1234567890123",
                    YearPublished = 2021,
                    Genre = "Fiction",
                    Pages = 100,
                    Price = 19.99
                },
                new Book
                {
                    Title = "Test Book 2",
                    Author = "Some Author",
                    ISBN = "1234567890121",
                    YearPublished = 2021,
                    Genre = "Horror",
                    Pages = 110,
                    Price = 11.99
                },
                new Book
                {
                    Title = "Test Book 3",
                    Author = "Some Author 2",
                    ISBN = "1234567890122",
                    YearPublished = 2020,
                    Genre = "Action",
                    Pages = 320,
                    Price = 10.99
                },
                new Book
                {
                    Title = "IT",
                    Author = "Stephen King",
                    ISBN = "1234567890124",
                    YearPublished = 1978,
                    Genre = "Horror",
                    Pages = 483,
                    Price = 21.99
                }
            };
        }

        [Fact]
        public async Task GetSpecificAsync_WithValidIsbn_ShouldReturnBook()
        {
            string validISBN = books[0].ISBN;

            foreach (var newBook in books)
            {
                await _bookManager.AddAsync(newBook);
            }

            var bookInDb = await _bookManager.GetSpecificAsync(validISBN);
            Assert.NotNull(bookInDb);
            Assert.Equal(books[0].Title, bookInDb.Title);
            Assert.Equal(books[0].Author, bookInDb.Author);
            Assert.Equal(books[0].Price, bookInDb.Price);
        }


        [Fact]
        public async Task GetSpecificAsync_WithInvalidIsbn_ShouldThrowKeyNotFoundException()
        {
            string invalidISBN = "0000000000000";
            foreach (var newBook in books)
            {
                await _bookManager.AddAsync(newBook);
            }

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookManager.GetSpecificAsync(invalidISBN));

            Assert.Equal($"No book found with ISBN: {invalidISBN}", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetSpecificAsync_NullOrEmptyIsbn_ShouldThrowArgumentException(string invalidISBN)
        {
            foreach (var newBook in books)
            {
                await _bookManager.AddAsync(newBook);
            }

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookManager.GetSpecificAsync(invalidISBN));

            Assert.Equal("ISBN cannot be empty.", exception.Message);
        }
    }
}
