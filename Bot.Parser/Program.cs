using HtmlAgilityPack;
using System;
using Bot.Model.DatabaseModels;
using Bot.Model;

await Task.Run(GetBooksFromSite);

async Task GetBooksFromSite()
{
    List<Book> books = new();

    // TODO There's a bug which is about books named in english and somehow about books which contain '...' in their title
    // and I don't have enough time to fix this, so I'm going to
    // leave this the way it is and parse only a hundred books in my database.
    // Also there's a problem with reading all the pages, so I put everything in a loop.
    // But anyway there're a hundred books......
    for (int j = 1; j <= 13; j++)
    {
        if (j == 5 || j == 9 || j == 11)
        {
            continue;
        }

        Console.WriteLine($"Page {j}");
        
        HtmlWeb web = new();
        var htmlDoc = await web.LoadFromWebAsync(@$"https://fb2-epub.ru/page/{j}/");

        var nodes = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__title h2')]");
        var nodes_2 = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__content-description')]");
        var nodes_3 = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__info-wrapper')]");

        // FIX BUG with href
        var nodes_4 = htmlDoc.DocumentNode.SelectNodes(".//a[contains(@class, 'entry__info-download')]");

        List<string> booksTitles = new();
        List<string> booksAuthors = new();
        List<string> booksDescriptions = new();
        List<string> booksGenres = new();
        List<string> booksLinks = new();

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

        for (int i = 0; i < booksTitles.Count; i++)
        {
            Console.WriteLine($"{booksGenres[i]}. {booksTitles[i]} - {booksAuthors[i]}");
            Console.WriteLine(booksDescriptions[i]);
            Console.WriteLine("Ссылка: " + booksLinks[i] + "\n");

            books.Add(new Book
            {
                Title = booksTitles[i],
                Author = booksAuthors[i],
                Description = booksDescriptions[i],
                Genre = booksGenres[i],
                Link = booksLinks[i],
            }) ;
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
