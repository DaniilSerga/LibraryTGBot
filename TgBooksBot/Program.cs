using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using System.Configuration;
using Microsoft.IdentityModel.Protocols;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using TgBooksBot;

#region Bot settings
ITelegramBotClient bot = new TelegramBotClient("5512051457:AAHr_huh1vactyi9EygMUFtF-BkSq0RkDWg");

Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { }, // receive all update types
};
bot.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken
);
Console.ReadLine();
#endregion

static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

    MessageHandler a = new();
    await a.ManageMessage(botClient, update);
}

static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    // Некоторые действия
    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
}
