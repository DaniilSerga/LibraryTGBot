using HtmlAgilityPack;
using System;
using Bot.Model.DatabaseModels;
using Bot.Model;

List<Book> books = await Task.Run(GetBooksFromSite);

await PushToDataBase(books);

#region Methods
async Task<List<Book>> GetBooksFromSite()
{
    Console.WriteLine("Loading...");

    List<Book> books = new();

    // TODO There's a bug which is about books named in english and somehow about books which contain '...' in their title
    // and I don't have enough time to fix this, so I'm going to
    // leave this the way it is and parse only a hundred books in my database.
    // Also there's a problem with reading all the pages, so I put everything in a loop.
    // But anyway there're a hundred books......
    for (int j = 32; j <= 100; j++)
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
            List<string> booksAuthors = new();
            List<string> booksDescriptions = new();
            List<string> booksGenres = new();
            List<string> booksLinks = new();
            List<string> booksPics = new();

            foreach (HtmlNode node in nodes)
            {
                // Contains author and a title of a book
                string text = node.InnerText;
                string bookTitle = text[..text.LastIndexOfAny(new char[] { '.', '?' })].Trim();
                string bookAuthor = text[(text.LastIndexOf('.') + 1)..].Trim();

                booksTitles.Add(bookTitle);
                booksAuthors.Add(bookAuthor);
            }
            foreach (HtmlNode node in nodes_2)
            {
                booksDescriptions.Add(node.InnerText.Trim());
            }
            foreach (HtmlNode node in nodes_3)
            {
                booksGenres.Add(node.InnerText.Trim());
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
                books.Add(new Book
                {
                    Title = booksTitles[i],
                    Author = booksAuthors[i],
                    Description = booksDescriptions[i],
                    Genre = booksGenres[i],
                    Link = booksLinks[i],
                    PictureLink = booksPics[i]
                });
            }
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

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\nSuccessfully parsed.\n\nStarting the proccess of pushing data to the database...");
    Console.ResetColor();
    
    return books;
}

void Print(List<Book> books)
{
    for (int i = 0; i < books.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {books[i].Title} - {books[i].Author}\n{books[i].Link}");
    }
}

async Task PushToDataBase(List<Book> books)
{
    if (books is null)
    {
        throw new ArgumentNullException(nameof(books), "No books were occured.");
    }

    using (ApplicationContext db = new())
    {
        await db.Books.AddRangeAsync(books);
        await db.SaveChangesAsync();
    }

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\nSuccessfully pushed to the database.");
    Console.ResetColor();
}
#endregion