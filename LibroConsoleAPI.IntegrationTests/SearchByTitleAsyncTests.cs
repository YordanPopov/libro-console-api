using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibroConsoleAPI.IntegrationTests.XUnit
{
    public class SearchByTitleAsyncTests : IClassFixture<BookManagerFixture>
    {
        private readonly BookManagerFixture _fixture;
        private readonly IBookManager _bookManager;
        private readonly TestLibroDbContext _dbContext;
        Book[] books;

        public SearchByTitleAsyncTests()
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
        public async Task SearchByTitleAsync_WithValidTitleFragment_ShouldReturnMatchingBooks()
        {
            var titleFragment = "Te";

            foreach (var newBook in books)
            {
                await _bookManager.AddAsync(newBook);
            }

            var booksInDb = await _bookManager.SearchByTitleAsync(titleFragment);
            Assert.NotNull(booksInDb);

            foreach (var book in booksInDb)
            {
                Assert.NotEmpty(book.Author);
                Assert.NotEmpty(book.ISBN);
                Assert.NotEmpty(book.Genre);
            }
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SearchByTitleAsync_WithInvalidTitleFragment_ShouldThrowArgumentException(string titleFragment)
        {    
            foreach (var newBook in books)
            {
                await _bookManager.AddAsync(newBook);
            }

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _bookManager.SearchByTitleAsync(titleFragment));

            Assert.Equal("Title fragment cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task SearchByTitleAsync_NoExistentTitleFragment_ShouldThrowKeyNotFoundException()
        {
            string nonExistentTitleFragment = "Non Existent";

            foreach (var newBook in books)
            {
                await _bookManager.AddAsync(newBook);
            }

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _bookManager.SearchByTitleAsync(nonExistentTitleFragment));

            Assert.Equal("No books found with the given title fragment.", exception.Message);
        }
    }
}
