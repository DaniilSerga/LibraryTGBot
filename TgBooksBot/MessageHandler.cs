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
        readonly CommandService commandService = new();

        public async Task ManageMessage(ITelegramBotClient botClient, Update update)
        {
            #region Message validation region
            if (update.CallbackQuery is not null)
            {
                if (update.CallbackQuery.Data == "bookAlike")
                {
                    await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id,
                        text: "book alike was acomplished");
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
                    else
                    {
                        await botClient.SendPhotoAsync(chatId: update.Message.Chat.Id,
                            photo: book.PictureLink,
                            caption: $"*{book.Title}*\n\n*АВТОР:* _{book.Author.Name}_\n*ЖАНР:* _{book.Genre.Name}_\n\n*ОПИСАНИЕ*\n{book.Description}...",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                new[] { InlineKeyboardButton.WithUrl("Прочитать онлайн", book.Link) },
                            }));

                        await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Выберите жанр:",
                        replyMarkup: new ForceReplyMarkup { Selective = true });

                        return;
                    }
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
                    foreach (var book in await commandService.GetRandomBookAsync())
                    {
                        await botClient.SendPhotoAsync(chatId: update.Message.Chat.Id,
                            photo: book.PictureLink,
                            caption: $"*{book.Title}*\n\n*АВТОР:* _{book.Author.Name}_\n*ЖАНР:* _{book.Genre.Name}_\n\n*ОПИСАНИЕ*\n{book.Description}...",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                new[] { InlineKeyboardButton.WithUrl("Прочитать онлайн", book.Link) },
                                new[] {InlineKeyboardButton.WithCallbackData("Найти похожую", "bookAlike")}
                            }));
                    }
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
            new KeyboardButton[] { "Жанр", "Название", "Автор"},
            new KeyboardButton[] { "Главное меню" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };

        //readonly InlineKeyboardMarkup genresInlineKeyboard = GetGenresButtons().Result;
        private async Task<List<List<InlineKeyboardButton>>> GetGenresButtons()
        {
            List<List<InlineKeyboardButton>> inlineButtons = new();

            var genres = await commandService.GetGenresAsync();

            for (int i = 0; i < genres.Count; i++)
            {
                inlineButtons.Add(new List<InlineKeyboardButton>());

                for (int j = 0; j < 3; j++)
                {
                    inlineButtons[i].Add(InlineKeyboardButton.WithCallbackData(genres[i].Name, $"/{genres[i].Name}"));
                }
            }
            //foreach (var genre in await commandService.GetGenresAsync())
            //{
            //    inlineButtons.Add(new List<InlineKeyboardButton>()
            //    {
            //    InlineKeyboardButton.WithCallbackData(genre.Name)
            //    });
            //}

            return inlineButtons;
        }
    }
}
