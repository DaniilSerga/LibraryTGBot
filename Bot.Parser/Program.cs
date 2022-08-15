using HtmlAgilityPack;
using System;
using Bot.Model;
// var nodes = htmlDoc.DocumentNode.SelectNodes(".//a[@class='art__name__href']");
// List<string> booksTitles = new List<string>();
// foreach (HtmlNode item in nodes_2) - booksAuthors.Add(item.InnerText);

HtmlWeb web = new();
var htmlDoc = await web.LoadFromWebAsync(@"https://fb2-epub.ru/");

var nodes = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__title h2')]");
var nodes_2 = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__content-description')]");
var nodes_3 = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__info-wrapper')]");

// FIX BUG with href
var nodes_4 = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'entry__info-download')]");

List<string> booksTitles = new();
List<string> booksAuthors = new();
List<string> booksDescriptions = new();
List<string> booksGenres = new();
List<string> booksLinks = new();

foreach (HtmlNode node in nodes)
{
    string text = node.InnerText;
    string bookTitle = text[..text.LastIndexOf('.')].Trim();
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
    booksLinks.Add(node.InnerText);
}

for (int i = 0; i < booksTitles.Count; i++)
{
    Console.WriteLine($"{booksGenres[i]}. {booksTitles[i]} - {booksAuthors[i]}");
    Console.WriteLine(booksDescriptions[i]);
    Console.WriteLine("Ссылка: " + booksLinks[i]);
}

