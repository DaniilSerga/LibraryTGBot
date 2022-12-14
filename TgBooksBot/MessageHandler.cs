using Bot.BusinessLogic.Services.Implementations;
using Bot.Model.DatabaseModels;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgBooksBot
{
    public class MessageHandler
    {
        private readonly ITelegramBotClient botClient;
        private readonly Update update;
        private readonly BooksService bookService = new();
        private readonly GenresService genresService = new();
        private readonly UsersService usersService = new();
        private readonly UsersBooksService usersBooksService = new();
        private List<UserBook> basketUserBooks = new();

        readonly ReplyKeyboardMarkup startupReplyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] { "Хочу книгу!", "Поиск" },
            new KeyboardButton[] { "Архив" }
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
                await Task.Run(() => ManageCallbackData());
            }
            else if (update.Message is not null)
            {
                await Task.Run(() => ManageTextMessage());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error was occured while managing user's message.");
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

            if (queryData.StartsWith("addToBasket"))
            {
                int bookId = int.Parse(queryData[(queryData.LastIndexOf(':') + 1)..queryData.LastIndexOf('/')]);
                long userId = long.Parse(queryData[(queryData.LastIndexOf('/') + 1)..queryData.LastIndexOf('-')]);
                string username = queryData[(queryData.LastIndexOf('-') + 1)..];

                if (await usersService.UserExists(userId))
                {
                    var user = await usersService.GetByTelegramId(userId);
                    UserBook userBook = new()
                    {
                        BookId = bookId,
                        UserId = user.Id
                    };

                    if (await usersBooksService.UserOwnsBook(userBook))
                    {
                        await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id,
                            text: "Данная книга уже в вашем архиве!");
                        return;
                    }

                    await usersBooksService.Create(userBook);

                    await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id,
                            text: "Книга была добавлена в ваш архив!");
                }
                else
                {
                    await usersBooksService.Create(new UserBook()
                    {
                        BookId = bookId,
                        User = new() { Username = username, UserId = userId }
                    });

                    await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id,
                            text: "Книга была добавлена в ваш архив!");
                }
            }

            if (queryData.StartsWith("basket-delete"))
            {
                int userBookId = int.Parse(queryData[(queryData.LastIndexOf(':') + 1)..]);

                await usersBooksService.Delete(userBookId);

                await Task.Run(() => UpdateBasketBooks(update.CallbackQuery.From.Id));

                if (basketUserBooks.Count == 0)
                {
                    await botClient.DeleteMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);

                    await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id,
                        text: "Ваш архив пуст.\nДобавьте книгу в свой архив, используя кнопку \"В архив\"",
                        replyMarkup: startupReplyKeyboardMarkup);
                }

                var userBook = basketUserBooks[0];

                string text = $"1) {userBook.Book.Author.Name} - {userBook.Book.Title}";

                await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId,
                    text: text,
                    replyMarkup: new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithUrl("Прочитать", userBook.Book.Link), InlineKeyboardButton.WithCallbackData("Описание", $"basket-info:{userBook.BookId}") },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(char.ConvertFromUtf32(0x25C0), $"basket-back:{basketUserBooks.IndexOf(userBook)}"),
                            InlineKeyboardButton.WithCallbackData("Удалить", $"basket-delete:{userBook.Id}"),
                            InlineKeyboardButton.WithCallbackData(char.ConvertFromUtf32(0x25B6), $"basket-next:{basketUserBooks.IndexOf(userBook)}")
                        }
                    }));
            }

            if (queryData.StartsWith("basket-next"))
            {
                await Task.Run(() => UpdateBasketBooks(update.CallbackQuery.From.Id));

                int userBookId = int.Parse(queryData[(queryData.LastIndexOf(':') + 1)..]) + 1;

                if (userBookId == basketUserBooks.Count)
                {
                    userBookId = 0;
                }

                var userBook = basketUserBooks[userBookId];

                string text = $"{userBookId + 1}) {userBook.Book.Author.Name} - {userBook.Book.Title}";

                await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId,
                    text: text,
                    replyMarkup: new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithUrl("Прочитать", userBook.Book.Link), InlineKeyboardButton.WithCallbackData("Описание", $"basket-info:{userBook.BookId}") },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(char.ConvertFromUtf32(0x25C0), $"basket-back:{basketUserBooks.IndexOf(userBook)}"),
                            InlineKeyboardButton.WithCallbackData("Удалить", $"basket-delete:{userBook.Id}"),
                            InlineKeyboardButton.WithCallbackData(char.ConvertFromUtf32(0x25B6), $"basket-next:{basketUserBooks.IndexOf(userBook)}")
                        }
                    }));
            }

            if (queryData.StartsWith("basket-back"))
            {
                await Task.Run(() => UpdateBasketBooks(update.CallbackQuery.From.Id));

                int userBookId = int.Parse(queryData[(queryData.LastIndexOf(':') + 1)..]) - 1;

                if (userBookId < 0)
                {
                    userBookId = basketUserBooks.Count - 1;
                }

                var userBook = basketUserBooks[userBookId];

                string text = $"{userBookId + 1}) {userBook.Book.Author.Name} - {userBook.Book.Title}";

                await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId,
                    text: text,
                    replyMarkup: new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithUrl("Прочитать", userBook.Book.Link), InlineKeyboardButton.WithCallbackData("Описание", $"basket-info:{userBook.BookId}") },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(char.ConvertFromUtf32(0x25C0), $"basket-back:{basketUserBooks.IndexOf(userBook)}"),
                            InlineKeyboardButton.WithCallbackData("Удалить", $"basket-delete:{userBook.Id}"),
                            InlineKeyboardButton.WithCallbackData(char.ConvertFromUtf32(0x25B6), $"basket-next:{basketUserBooks.IndexOf(userBook)}")
                        }
                    }));
            }

            if (queryData.StartsWith("basket-info"))
            {
                int bookId = int.Parse(queryData[(queryData.LastIndexOf(':') + 1)..]);

                await SendBookAsync(await bookService.GetBook(bookId));
            }
        }

        // Manages user's text messages
        private async Task ManageTextMessage()
        {
            #region Checks
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
                var book = await Task.Run(() => bookService.GetRandomBookByGenreAsync(update.Message.Text));

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
                    var book = await Task.Run(() => bookService.GetRandomBookAsync());
                    await SendBookAsync(book);

                    break;

                case "Поиск":
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Пожалуйста, выберите _условие_ поиска:",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: searchReplyKeyBoardMarkup);
                    break;

                // Выборка по жанру
                case "Жанр":
                    var genres = await Task.Run(() => genresService.GetAll());

                    StringBuilder gnrs = new();

                    for (int i = 0; i < genres.Count; i++)
                    {
                        gnrs.AppendLine($"`{genres[i].Name}`");
                    }

                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: $"Список доступных жанров:\n {gnrs}\n(Нажмите на жанр, чтобы скопировать его)",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Меню", "menu")));

                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Выберите жанр:",
                        replyMarkup: new ForceReplyMarkup { Selective = true, InputFieldPlaceholder = "Вставьте сюда выбранный жанр" });

                    break;

                case "Архив":
                    basketUserBooks = await usersBooksService.GetAllUserBooksByTelegramId(update.Message.Chat.Id);

                    await Task.Run(() => SendBasketBookAsync());

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

        // Sends a book
        private async Task SendBookAsync(Book book)
        {
            if (book is null)
            {
                throw new ArgumentNullException(nameof(book), "Nothing to send.");
            }

            long chatId = update.Message is null ? update.CallbackQuery.Message.Chat.Id : update.Message.Chat.Id;
            long userId = update.Message is null ? update.CallbackQuery.From.Id : update.Message.From.Id;
            string username = update.Message is null ? update.CallbackQuery.From.Username : update.Message.From.Username;

            await botClient.SendPhotoAsync(chatId: chatId,
                            photo: book.PictureLink,
                            caption: $"*{book.Title}*\n\n*АВТОР:* _{book.Author.Name}_\n*ЖАНР:* _{book.Genre.Name}_\n\n*ОПИСАНИЕ*\n{book.Description}...",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                new[] { InlineKeyboardButton.WithUrl("Прочитать онлайн", book.Link) },
                                new[] { InlineKeyboardButton.WithCallbackData("Меню", "menu"), InlineKeyboardButton.WithCallbackData("Похожая", $"bookAlike:{book.Genre.Name}") },
                                new[] { InlineKeyboardButton.WithCallbackData("В архив", $"addToBasket:{book.Id}/{userId}-{username}") }
                            }));
        }

        private async Task SendBasketBookAsync()
        {
            if (update.Message is not null)
            {
                await UpdateBasketBooks(update.Message.From.Id);
            }
            else
            {
                await UpdateBasketBooks(update.CallbackQuery.From.Id);
            }

            if (basketUserBooks.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Ваш архив пуст.\nДобавьте книгу в свой архив, используя кнопку \"В архив\"",
                        replyMarkup: startupReplyKeyboardMarkup);

                return;
            }

            UserBook userBook = basketUserBooks[0];

            string text = $"1) {userBook.Book.Author.Name} - {userBook.Book.Title}";

            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                text: text,
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                      new[] 
                      { 
                          InlineKeyboardButton.WithUrl("Прочитать", userBook.Book.Link), 
                          InlineKeyboardButton.WithCallbackData("Описание", $"basket-info:{userBook.BookId}") 
                      },
                      new[]
                      {
                          InlineKeyboardButton.WithCallbackData(char.ConvertFromUtf32(0x25C0), $"basket-back:{0}"),
                          InlineKeyboardButton.WithCallbackData("Удалить", $"basket-delete:{userBook.Id}"),
                          InlineKeyboardButton.WithCallbackData(char.ConvertFromUtf32(0x25B6), $"basket-next:{0}")
                      }
                }));
        }

        private async Task UpdateBasketBooks(long userId)
        {
            basketUserBooks = await usersBooksService.GetAllUserBooksByTelegramId(userId);
        }
    }
}
