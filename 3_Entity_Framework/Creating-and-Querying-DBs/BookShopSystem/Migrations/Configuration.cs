namespace BookShopSystem.Migrations
{
    using Data;
    using Models;
    using System;
    using System.Data.Entity.Migrations;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    public class Configuration : DbMigrationsConfiguration<BookShopContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "BookShopSystem.Data.BookShopContext";
        }

        protected override void Seed(BookShopContext context)
        {
            SeedAuthors(context);
            SeedBooks(context);
            SeedCategories(context);
        }

        private void SeedAuthors(BookShopContext context)
        {
            string[] authors = File.ReadAllLines("../../Import/authors.csv");
            // Foreach all authors and add them to database.
            // Skip first row because it is only describing column names.
            for (int i = 1; i < authors.Length; i++)
            {
                string[] data = authors[i].Split(',');
                string firstName = data[0].Replace("\"", string.Empty);
                string lastName = data[1].Replace("\"", string.Empty);

                Author author = new Author()
                {
                    FirstName = firstName,
                    LastName = lastName
                };

                // When adding we may want to check if author with same first name and last name is existing.
                context.Authors.AddOrUpdate(a => new { a.FirstName, a.LastName }, author);
            }
        }

        private static void SeedBooks(BookShopContext context)
        {
            int authorsCount = context.Authors.Local.Count;

            string[] books = File.ReadAllLines("../../Import/books.csv");

            for (int i = 1; i < books.Length; i++)
            {
                string[] data = books[i]
                    .Split(',')
                    .Select(arg => arg.Replace("\"", string.Empty))
                    .ToArray();

                int authorIndex = i % authorsCount;

                Author author = context.Authors.Local[authorIndex];

                EditionTypeEnum edition = (EditionTypeEnum)int.Parse(data[0]);

                DateTime releaseDate = DateTime.ParseExact(data[1], "d/M/yyyy", CultureInfo.InvariantCulture);

                int copies = int.Parse(data[2]);

                decimal price = decimal.Parse(data[3]);

                AgeRestEnum ageRestriction = (AgeRestEnum)int.Parse(data[4]);

                string title = data[5];

                Book book = new Book
                {
                    Author = author,
                    Author_Id = author.Id,
                    EditionType = edition,
                    ReleaseDate = releaseDate,
                    Copies = copies,
                    Price = price,
                    AgeRestriction = ageRestriction,
                    Title = title
                };

                context.Books.AddOrUpdate(b => new { b.Title, b.Author_Id }, book);
            }
        }

        private static void SeedCategories(BookShopContext context)
        {
            int booksCount = context.Books.Local.Count;

            string[] categories = File.ReadAllLines("../../Import/categories.csv");

            for (int i = 1; i < categories.Length; i++)
            {
                string[] data = categories[i]
                  .Split(',')
                  .Select(arg => arg.Replace("\"", string.Empty))
                  .ToArray();

                string categoryName = data[0];

                Category category = new Category() { Name = categoryName };

                int bookIndex = (i * 30) % booksCount;

                for (int j = 0; j < bookIndex; j++)
                {
                    Book book = context.Books.Local[j];
                    category.Books.Add(book);
                }

                context.Categories.AddOrUpdate(c => c.Name, category);
            }
        }
    }
}
