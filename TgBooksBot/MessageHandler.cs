﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Bot.BusinessLogic.Services;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Bot.BusinessLogic.Services.Implementations;
using System.Collections.Generic;
using Bot.Model.DatabaseModels;
using System.Text;

namespace TgBooksBot
{
    public class MessageHandler
    {
        private readonly ITelegramBotClient botClient;
        private readonly Update update;
        private readonly BooksService bookService = new();
        private readonly GenresService genresService = new();
        private readonly UsersService usersService = new();

        readonly ReplyKeyboardMarkup startupReplyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] { "Хочу книгу!", "Поиск" },
            new KeyboardButton[] { "Корзина" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };

        readonly ReplyKeyboardMarkup searchReplyKeyBoardMarkup = new(new[]
        {
            new KeyboardButton[] { "Жанр" },
            new KeyboardButton[] { "Главное меню" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };

        public MessageHandler(ITelegramBotClient botClient, Update update)
        {
            this.botClient = botClient;
            this.update = update;
        }

        public async Task ManageMessage()
        {
            if (update.CallbackQuery is not null)
            {
                Console.WriteLine("Новый callback query поток запущен!!!!!");
                await Task.Run(() => ManageCallbackData());
            }
            else if (update.Message is not null)
            {
                Console.WriteLine("Новый text message поток запущен!!!!!");
                await Task.Run(() => ManageTextMessage());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("BRUH BRUH BRUH BRUH BRUH BRUH BRUH BRUH BRUH BRUH BRUH BRUH ");
                Console.ResetColor();
            }
        }

        // manages user's callback data
        private async Task ManageCallbackData()
        {
            if (update.CallbackQuery is null || update.CallbackQuery.Data is null || update.CallbackQuery.Message is null)
            {
                throw new ArgumentNullException(nameof(update), "Callback query is null.");
            }

            string queryData = update.CallbackQuery.Data;

            if (queryData.StartsWith("bookAlike"))
            {
                string query = update.CallbackQuery.Data;

                // input string looks like "bookAlike:fantasy"
                var book = await bookService.GetRandomBookByGenreAsync(query[(query.LastIndexOf(':') + 1)..]);

                await SendBookAsync(book);

                return;
            }

            if (queryData == "menu")
            {
                await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id,
                        text: "Воспользуйтесь меню, чтобы я прислал вам книгу:",
                        replyMarkup: startupReplyKeyboardMarkup);

                return;
            }

            // TODO Adding book to a basket
            // input string must look like "addToBasket:bookId/userId"
            if (queryData.StartsWith("addToBasket"))
            {
                int bookId = int.Parse(queryData[(queryData.LastIndexOf(':') + 1) .. queryData.LastIndexOf('/')]);
                long userId = long.Parse(queryData[(queryData.LastIndexOf('/') + 1)..queryData.LastIndexOf('-')]);
                string username = queryData[(queryData.LastIndexOf('-') + 1)..];
                Console.WriteLine($"BookId: {bookId} --- UserId: {userId} --- Username: {username}");

                //await usersService.Create(new UserBook { BookId = bookId, UserId = userId });
            }
        }

        // Manages user's text messages
        private async Task ManageTextMessage()
        {
            if (update.Message is null)
            {
                throw new ArgumentNullException(nameof(update), "Message is null.");
            }

            if (update.Message.Type is not MessageType.Text)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Неизвестная команда, повторите попытку:");
                return;
            }

            if (string.IsNullOrEmpty(update.Message.Text))
            {
                throw new ArgumentNullException(nameof(update), "User message's text is null.");
            }

            // Вывод введённого жанра
            if (update.Message.ReplyToMessage is not null && update.Message.ReplyToMessage.Text.Contains("Выберите жанр:"))
            {
                var book = await bookService.GetRandomBookByGenreAsync(update.Message.Text);

                if (book is null)
                {
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Жанр не найден",
                        replyMarkup: searchReplyKeyBoardMarkup);
                    return;
                }

                await SendBookAsync(book);

                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Выберите жанр:",
                        replyMarkup: new ForceReplyMarkup { Selective = true });

                return;
            }

            string userMessage = update.Message.Text;

            switch (userMessage)
            {
                case "/start":
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            text: "Здравствуйте, я - бот, который поможет вам определиться с тем, какую книгу почитать.\n" +
                            "\nЯ был бы очень рад, если бы вы поделились мной со своими знакомыми :)",
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithSwitchInlineQuery("Поделитесь нашим ботом")));
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            text: "Воспользуйтесь меню, чтобы я прислал вам книгу:",
                            replyMarkup: startupReplyKeyboardMarkup);
                    break;

                case "Хочу книгу!":
                    await SendBookAsync(await bookService.GetRandomBookAsync());
                    break;

                case "Поиск":
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Пожалуйста, выберите _условие_ поиска:",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: searchReplyKeyBoardMarkup);
                    break;

                // Выборка по жанру
                case "Жанр":
                    var genres = await genresService.GetAll();

                    StringBuilder gnrs = new();

                    for (int i = 0; i < genres.Count; i++)
                    {
                        gnrs.AppendLine($"`{genres[i].Name}`");
                    }

                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: $"Список доступных жанров:\n {gnrs}\n(Нажмите на жанр, чтобы скопировать его)",
                        parseMode: ParseMode.Markdown);

                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Выберите жанр:",
                        replyMarkup: new ForceReplyMarkup { Selective = true, InputFieldPlaceholder = "Вставьте сюда выбранный жанр" });

                    break;

                case "Главное меню":
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Воспользуйтесь меню, чтобы я прислал вам книгу:",
                        replyMarkup: startupReplyKeyboardMarkup);
                    break;

                default:
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Неизвестная команда, повторите попытку:");
                    break;
            }
        }

        // Send a book
        private async Task SendBookAsync(Book book)
        {
            if (book is null)
            {
                throw new ArgumentNullException(nameof(book), "Nothing to send.");
            }

            long chatId = update.Message is null ? update.CallbackQuery.Message.Chat.Id : update.Message.Chat.Id;
            string username = update.Message is null ? update.Message.From.Username : update.CallbackQuery.From.Username;

            await botClient.SendPhotoAsync(chatId: chatId,
                            photo: book.PictureLink,
                            caption: $"*{book.Title}*\n\n*АВТОР:* _{book.Author.Name}_\n*ЖАНР:* _{book.Genre.Name}_\n\n*ОПИСАНИЕ*\n{book.Description}...",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                new[] { InlineKeyboardButton.WithUrl("Прочитать онлайн", book.Link) },
                                new[] { InlineKeyboardButton.WithCallbackData("Меню", "menu"), InlineKeyboardButton.WithCallbackData("Похожая", $"bookAlike:{book.Genre.Name}") },
                                new[] { InlineKeyboardButton.WithCallbackData("В архив", $"addToBasket:{book.Id}/{chatId}-{username}") }
                            }));
        }
    }
}
