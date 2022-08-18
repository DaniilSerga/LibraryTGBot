using Telegram.Bot;
using Telegram.Bot.Types;
using Bot.BusinessLogic.Services;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Bot.BusinessLogic.Services.Implementations;

namespace TgBooksBot
{
    public class MessageHandler
    {
        CommandService commandService = new();

        public async Task ManageMessage(ITelegramBotClient botClient, Update update)
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
                            caption: $"*{book.Title}*\n\n*АВТОР:* _{book.Author}_\n*ЖАНР:* _{book.Genre}_\n\n*ОПИСАНИЕ*\n\n{book.Description}",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Прочитать онлайн", book.Link)));
                    }
                    break;

                case "Поиск":
                    await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                        text: "Пожалуйста, выберите _условие_ поиска:",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: searchReplyKeyBoardMarkup);
                    break;

                case "Жанр":
                    var books = await commandService.GetBooksByGenreAsync(update.Message.Text);
                    //TODO Implement get books by genre method in switch-case section
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
    }
}
