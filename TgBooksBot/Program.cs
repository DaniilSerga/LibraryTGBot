using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
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

    MessageHandler messageHandler = new (botClient, update);

    try
    {
        await Task.Run(() => messageHandler.ManageMessage());
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    // Некоторые действия
    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
}
