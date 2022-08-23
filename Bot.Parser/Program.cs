using HtmlAgilityPack;
using System;
using Bot.Model.DatabaseModels;
using Bot.Model;
using Telegram.Bot.Types;

await Task.Run(GetBooksFromSite);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\nSuccessfully pushed to the database.");
Console.ResetColor();

#region Methods
async Task GetBooksFromSite()
{
    Console.WriteLine("Loading...");

    List<Book> books = new();

    for (int j = 201; j < 300; j++)
    {
        HtmlWeb web = new();
        var htmlDoc = await web.LoadFromWebAsync(@$"https://fb2-epub.ru/page/{j}/");

        try
        {
            var nodes = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__title h2')]");
            var nodes_2 = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__content-description')]");
            var nodes_3 = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__info-wrapper')]");
            var nodes_4 = htmlDoc.DocumentNode.SelectNodes(".//a[contains(@class, 'entry__info-download')]");
            var nodes_5 = htmlDoc.DocumentNode.SelectNodes(".//a[contains(@class, 'entry__content-image')]/img");

            List<string> booksTitles = new();
            List<string> booksDescriptions = new();
            List<string> booksLinks = new();
            List<string> booksPics = new();

            List<Genre> booksGenres = new(); //node_3
            List<Author> booksAuthors = new(); //node

            foreach (HtmlNode node in nodes)
            {
                string text = node.InnerText;
                string bookTitle = text[..text.LastIndexOfAny(new char[] { '.', '?' })].Trim();
                string bookAuthor = text[(text.LastIndexOf('.') + 1)..].Trim();

                booksTitles.Add(bookTitle);
                booksAuthors.Add(new Author { Name = bookAuthor} );
            }
            foreach (HtmlNode node in nodes_2)
            {
                booksDescriptions.Add(node.InnerText.Trim());
            }
            foreach (HtmlNode node in nodes_3)
            {
                booksGenres.Add(new Genre { Name = node.InnerText.Trim() });
            }
            foreach (HtmlNode node in nodes_4)
            {
                booksLinks.Add(node.Attributes["href"].Value);
            }
            foreach (HtmlNode node in nodes_5)
            {
                booksPics.Add("https://fb2-epub.ru" + node.Attributes["src"].Value);
            }

            for (int i = 0; i < booksTitles.Count; i++)
            {
                Book book = new()
                {
                    Title = booksTitles[i],
                    Description = booksDescriptions[i],
                    Link = booksLinks[i],
                    PictureLink = booksPics[i]
                };

                using (ApplicationContext db = new())
                {
                    Genre genre = db.Genres.FirstOrDefault(g => g.Name == booksGenres[i].Name);
                    if (genre is null)
                    {
                        book.Genre = booksGenres[i];
                    }
                    else
                    {
                        book.GenreId = genre.Id;
                    }

                    Author author = db.Authors.FirstOrDefault(a => a.Name == booksAuthors[i].Name);
                    if (author is null)
                    {
                        book.Author = booksAuthors[i];
                    }
                    else
                    {
                        book.AuthorId = author.Id;
                    }
                }

                await PushToDataBase(book);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Book was successfully added to database");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
            continue;
        }
    }

    Print(books);
}

void Print(List<Book> books)
{
    for (int i = 0; i < books.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {books[i].Title} - {books[i].Author}\n{books[i].Link}");
    }
}

async Task PushToDataBase(Book books)
{
    if (books is null)
    {
        throw new ArgumentNullException(nameof(books), "No books were occured.");
    }

    using (ApplicationContext db = new())
    {
        await db.Books.AddAsync(books);
        await db.SaveChangesAsync();
    }
}
#endregion