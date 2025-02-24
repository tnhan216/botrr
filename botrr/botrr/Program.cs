using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    private static readonly string Token = "8100640488:AAF4Vc2DbJQ3mEQ81ugKJVbcOQk5GiF7lhk"; // Thay bằng token của bạn
    private static readonly TelegramBotClient Bot = new TelegramBotClient(Token);

    static async Task Main()
    {
        using CancellationTokenSource cts = new();

        // Lắng nghe tin nhắn
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        Bot.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

        var me = await Bot.GetMeAsync();
        Console.WriteLine($"🤖 Bot {me.Username} đã khởi động!");

        Console.ReadLine();
        cts.Cancel();
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message.Text != null)
        {
            Message message = update.Message;
            Console.WriteLine($"📩 Nhận tin nhắn từ {message.Chat.FirstName}: {message.Text}");

            // Xử lý lệnh "/tp BTCUSDT 45000 42000"
            string[] parts = message.Text.Split(' ');
            if (parts.Length == 4 && parts[0] == "/tp")
            {
                string coin = parts[1];
                if (float.TryParse(parts[2], out float entry) && float.TryParse(parts[3], out float sl))
                {
                    float risk = entry - sl;
                    float tp1 = entry + risk * 1;
                    float tp2 = entry + risk * 2;
                    float tp3 = entry + risk * 3;

                    string response = $"📌 *{coin}* TP Levels:\n- TP1: `{tp1}`\n- TP2: `{tp2}`\n- TP3: `{tp3}`";
                    Console.WriteLine($"🔍 Chat ID: {message.Chat.Id}, Thread ID: {message.MessageThreadId}");

                    await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: response,
                            parseMode: ParseMode.Markdown,
                            messageThreadId: message.MessageThreadId, // Thêm dòng này
                            cancellationToken: cancellationToken
                        );
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "⚠ Sai cú pháp! Dùng: `/tp BTCUSDT 45000 42000`");
                }
            }
            else
            {
                ///////
                await botClient.SendTextMessageAsync(message.Chat.Id, "Gõ `/tp coin entry sl` để tính TP.");
            }
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"❌ Lỗi bot: {exception.Message}");
        return Task.CompletedTask;
    }
}
