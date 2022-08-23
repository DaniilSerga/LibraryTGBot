using Telegram.Bot;
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
        private readonly CommandService commandService = new();

        readonly ReplyKeyboardMarkup startupReplyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] {"Хочу книгу!", "Поиск"}
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
            #region Message validation region
            if (update.CallbackQuery is not null)
            {
                if (update.CallbackQuery.Data.StartsWith("bookAlike"))
                {
                    string query = update.CallbackQuery.Data;

                    foreach (var book in await commandService.GetBooksByGenreAsync(query[(query.LastIndexOf(':') + 1)..]))
                    {
                        await SendBookAsync(book);
                    }

                    return;
                }

                if (update.CallbackQuery.Data == "menu")
                {
                    await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id,
                            text: "Воспользуйтесь меню, чтобы я прислал вам книгу:",
                            replyMarkup: startupReplyKeyboardMarkup);

                    return;
                }
            }

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
            #endregion

            // Вывод введённого жанра
            if (update.Message.ReplyToMessage is not null && update.Message.ReplyToMessage.Text.Contains("Выберите жанр:"))
            {

                foreach (var book in await commandService.GetBooksByGenreAsync(update.Message.Text))
                {
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
            }

            string userMessage = update.Message.Text;

            switch (userMessage)
            {
                case "/start":
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            text: "Здравствуйте, я - бот, который поможет вам определиться с тем, какую книгу почитать.\n" +
                            "Я был бы очень рад, если бы вы поделились мной со своими знакомыми :)",
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithSwitchInlineQuery("Поделитесь нашим ботом")));
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                            text: "Воспользуйтесь меню, чтобы я прислал вам книгу:",
                            replyMarkup: startupReplyKeyboardMarkup);
                    break;

                case "Хочу книгу!":
                    await SendBookAsync(await commandService.GetRandomBookAsync());
                    break;

                case "Поиск":
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Пожалуйста, выберите _условие_ поиска:",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: searchReplyKeyBoardMarkup);
                    break;

                // Выборка по жанру
                case "Жанр":
                    var genres = await commandService.GetGenresAsync();

                    StringBuilder gnrs = new();

                    for (int i = 0; i < genres.Count; i++)
                    {
                        gnrs.AppendLine($"`{genres[i].Name}`");
                    }

                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: $"Список доступных жанров: {gnrs}\n(Нажмите на жанр, чтобы скопировать его)",
                        parseMode: ParseMode.Markdown);

                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Выберите жанр:",
                        replyMarkup: new ForceReplyMarkup { Selective = true });

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

        private async Task SendBookAsync(Book book)
        {
            if (book is null)
            {
                throw new ArgumentNullException(nameof(book), "Nothing to send.");
            }

            long chatId = update.Message is null ? update.CallbackQuery.Message.Chat.Id : update.Message.Chat.Id;

            await botClient.SendPhotoAsync(chatId: chatId,
                            photo: book.PictureLink,
                            caption: $"*{book.Title}*\n\n*АВТОР:* _{book.Author.Name}_\n*ЖАНР:* _{book.Genre.Name}_\n\n*ОПИСАНИЕ*\n{book.Description}...",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                new[] { InlineKeyboardButton.WithUrl("Прочитать онлайн", book.Link) },
                                new[] { InlineKeyboardButton.WithCallbackData("Меню", "menu"), InlineKeyboardButton.WithCallbackData("Похожая", $"bookAlike:{book.Genre.Name}") },
                            }));
        }
    }
}
