using LibroConsoleAPI.Business;
using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using LibroConsoleAPI.DataAccess;
using LibroConsoleAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using NuGet.Frameworks;
using System;
using System.ComponentModel.DataAnnotations;

namespace LibroConsoleAPI.IntegrationTests.NUnit
{
    public  class IntegrationTests
    {
        private TestLibroDbContext dbContext;
        private BookManager bookManager;

        [SetUp]
        public void SetUp()
        {
            string dbName = $"TestDb_{Guid.NewGuid()}";
            this.dbContext = new TestLibroDbContext(dbName);
            this.bookManager = new BookManager(new BookRepository(this.dbContext));
        }

        [TearDown]
        public void TearDown()
        {
            this.dbContext.Dispose();
        }

        [Test]
        public async Task AddBookAsync_ShouldAddBook()
        {
            // Arrange
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);
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

            // Act
            await bookManager.AddAsync(newBook);

            // Assert
            var bookInDb = await dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == newBook.ISBN);
            Assert.That(bookInDb, Is.Not.Null);
            Assert.That(bookInDb.Title, Is.EqualTo("Test Book"));
            Assert.That(bookInDb.Author, Is.EqualTo("John Doe"));
        }

        [Test]
        public async Task AddBookAsync_TryToAddBookWithInvalidCredentials_ShouldThrowException()
        {
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);
            Book invalidBook = new Book
            {
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await bookManager.AddAsync(invalidBook));
            Assert.That(ex.Message, Is.EqualTo("Book is invalid."));
        }

        [Test]
        public async Task DeleteBookAsync_WithValidISBN_ShouldRemoveBookFromDb()
        {
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);
            var bookToDelete = dbContext.Books.First();

            await bookManager.DeleteAsync(bookToDelete.ISBN);

            var booksInDb = dbContext.Books.ToList();
            Assert.That(booksInDb.Count(), Is.EqualTo(9));
            Assert.That(booksInDb, Does.Not.Contain(bookToDelete));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task DeleteBookAsync_TryToDeleteWithNullOrWhiteSpaceISBN_ShouldThrowException(string isbn)
        {
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await bookManager.DeleteAsync(isbn));
            Assert.That(ex.Message, Is.EqualTo("ISBN cannot be empty."));
        }

        [Test]
        public async Task GetAllAsync_WhenBooksExist_ShouldReturnAllBooks()
        {
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);

            var books = await bookManager.GetAllAsync();
            Assert.That(books.Count(), Is.EqualTo(10));
        }


        [Test]
        public async Task GetAllAsync_WhenNoBooksExist_ShouldThrowKeyNotFoundException()
        {
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await bookManager.GetAllAsync());
            Assert.That(ex.Message, Is.EqualTo("No books found."));
        }


        [Test]
        public async Task SearchByTitleAsync_WithValidTitleFragment_ShouldReturnMatchingBooks()
        {
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);

            var expectedBook = dbContext.Books.FirstOrDefault(b => b.ISBN == "9780307743394");

            var act = await bookManager.SearchByTitleAsync("Pride");
            var actualBook = act.First();
            Assert.That(actualBook.Title, Is.EqualTo(expectedBook.Title));
            Assert.That(actualBook.Author, Is.EqualTo(expectedBook.Author));
            Assert.That(actualBook.ISBN, Is.EqualTo(expectedBook.ISBN));
        }


        [Test]
        public async Task SearchByTitleAsync_WithInvalidTitleFragment_ShouldThrowKeyNotFoundException()
        {
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await bookManager.SearchByTitleAsync("Non-Existent fragment"));
            Assert.That(ex.Message, Is.EqualTo("No books found with the given title fragment."));
        }


        [Test]
        public async Task GetSpecificAsync_WithValidIsbn_ShouldReturnBook()
        {
            string validIsbn = "9780062315007";
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);
            var expectedBook = dbContext.Books.FirstOrDefault(b => b.ISBN == validIsbn);

            var actualBook = await bookManager.GetSpecificAsync(validIsbn);
            Assert.That(actualBook.Title, Is.EqualTo(expectedBook.Title));
            Assert.That(actualBook.Author, Is.EqualTo(expectedBook.Author));
            Assert.That(actualBook.Pages, Is.EqualTo(expectedBook.Pages));
        }


        [Test]
        public async Task GetSpecificAsync_WithInvalidIsbn_ShouldThrowKeyNotFoundException()
        {
            string invalidIsbn = "000000000";
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await bookManager.GetSpecificAsync(invalidIsbn));
            Assert.That(ex.Message, Is.EqualTo($"No book found with ISBN: {invalidIsbn}"));
        }


        [Test]
        public async Task UpdateAsync_WithValidBook_ShouldUpdateBook()
        {
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);
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

            await bookManager.AddAsync(newBook);
            var booksInDb = dbContext.Books.ToList();
            Assert.That(booksInDb.Count(), Is.EqualTo(11));

            string updatedTitle = newBook.Title + " UPDATED";
            newBook.Title = updatedTitle;
            await bookManager.UpdateAsync(newBook);

            var updatedBookInDb = dbContext.Books.FirstOrDefault(x => x.Title == updatedTitle);
            Assert.That(updatedBookInDb, Is.Not.Null);
        }


        [Test]
        public async Task UpdateAsync_WithInvalidBook_ShouldThrowValidationException()
        {
            DatabaseSeeder.SeedDatabaseAsync(dbContext, bookManager);
            var invalidBook = new Book
            {

                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await bookManager.UpdateAsync(invalidBook));
            Assert.That(ex.Message, Is.EqualTo("Book is invalid."));
        }

    }
}
