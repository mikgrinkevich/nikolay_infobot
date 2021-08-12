using AngleSharp.Html.Parser;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace nikTelegramWeather_bot
{
    public static class Program
    {
        
        private static TelegramBotClient Bot;

        public static async Task Main()
        {
            var botToken = "1805969287:AAE6nE0eKMf3m7S0lLY95CnfsFkeDSmEEbQ";

            Bot = new TelegramBotClient(botToken);

            var me = await Bot.GetMeAsync();
            Console.Title = me.Username;

            var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.

            Bot.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                               cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();

            
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            var action = (message.Text.Split(' ').First()) switch
            {
                "/start" => SendInlineKeyboard(message),

            };
            var sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

             

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            static async Task<Message> SendInlineKeyboard(Message message)
            {

                
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                


                   InlineKeyboardMarkup inlineKeyboard = new(new[]
                   {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Погода ", "Погода"),
                        InlineKeyboardButton.WithCallbackData("Курс валют", "Курс валют"),
                    },


                    });

                return await Bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Choose",
                                                      replyMarkup: inlineKeyboard);
                
            }

        }

        public class ByrRate
        {
            public static DateTime date { get; set; }
            public decimal byr { get; set; }
        }

        // Process Inline Keyboard callback data
        private static async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            
            var htmlParser = new HtmlParser();
            HttpClient client = new HttpClient();
            
            
            switch (callbackQuery.Data)
            {
                case "Курс валют":
                    HttpResponseMessage Response = await client.GetAsync("https://cdn.jsdelivr.net/gh/fawazahmed0/currency-api@1/latest/currencies/usd/byr.json");
                    string responseBody = await Response.Content.ReadAsStringAsync();
                    ByrRate byrRate = JsonSerializer.Deserialize<ByrRate>(responseBody);
                    await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                                                       $"{byrRate.byr}");
                    break;
                case "Погода":
                    InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Минск", "Минск"),
                        InlineKeyboardButton.WithCallbackData("Могилев", "Могилев"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Гомель", "Гомель"),
                        InlineKeyboardButton.WithCallbackData("Гродно", "Гродно"),
                    },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Брест", "Брест"),
                        InlineKeyboardButton.WithCallbackData("Витебск", "Витебск"),
                    },


                });

                     await Bot.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id,
                                                          text: "Choose",
                                                          replyMarkup: inlineKeyboard);
                    break;
                case "Минск":
                    var htmlMinsk = await client.GetStringAsync("https://www.gismeteo.by/weather-minsk-4248/");
                    var documentMinsk = await htmlParser.ParseDocumentAsync(htmlMinsk);
                    var elementMinsk = documentMinsk.QuerySelector("body > section > div.content_wrap > div > div.main > div > div.__frame_sm > div.forecast_frame.hw_wrap > div.tabs._center > a:nth-child(1) > div > div.tab-content > div.tab-weather > div.js_meas_container.temperature.tab-weather__value > span.unit.unit_temperature_c > span");
                    var weatherMinsk = elementMinsk.TextContent;
                    await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                                                       $"{weatherMinsk}");
                    break;
                case "Могилев":
                    var htmlMogilev = await client.GetStringAsync("https://www.gismeteo.by/weather-mogilev-4251/");
                    var documentMogilev = await htmlParser.ParseDocumentAsync(htmlMogilev);
                    var elementMogilev = documentMogilev.QuerySelector("body > section > div.content_wrap > div > div.main > div > div.__frame_sm > div.forecast_frame.hw_wrap > div.tabs._center > a:nth-child(1) > div > div.tab-content > div.tab-weather > div.js_meas_container.temperature.tab-weather__value > span.unit.unit_temperature_c > span");
                    var weatherMogilev = elementMogilev.TextContent;
                    await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                                                       $"{weatherMogilev}");
                    break;
                case "Гомель":
                    var htmlGomel = await client.GetStringAsync("https://www.gismeteo.by/weather-gomel-4918/");
                    var documentGomel = await htmlParser.ParseDocumentAsync(htmlGomel);
                    var elementGomel = documentGomel.QuerySelector("body > section > div.content_wrap > div > div.main > div > div.__frame_sm > div.forecast_frame.hw_wrap > div.tabs._center > a:nth-child(1) > div > div.tab-content > div.tab-weather > div.js_meas_container.temperature.tab-weather__value > span.unit.unit_temperature_c > span");
                    var weatherGomel = elementGomel.TextContent;
                    await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                                                       $"{weatherGomel}");
                    break;
                case "Гродно":
                    var htmlGrodno = await client.GetStringAsync("https://www.gismeteo.by/weather-grodno-4243/");
                    var documentGrodno = await htmlParser.ParseDocumentAsync(htmlGrodno);
                    var elementGrodno = documentGrodno.QuerySelector("body > section > div.content_wrap > div > div.main > div > div.__frame_sm > div.forecast_frame.hw_wrap > div.tabs._center > a:nth-child(1) > div > div.tab-content > div.tab-weather > div.js_meas_container.temperature.tab-weather__value > span.unit.unit_temperature_c > span");
                    var weatherGrodno = elementGrodno.TextContent;
                    await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                                                       $"{weatherGrodno}");
                    break;
                case "Брест":
                    var htmlBrest = await client.GetStringAsync("https://www.gismeteo.by/weather-brest-4912/");
                    var documentBrest = await htmlParser.ParseDocumentAsync(htmlBrest);
                    var elementBrest = documentBrest.QuerySelector("body > section > div.content_wrap > div > div.main > div > div.__frame_sm > div.forecast_frame.hw_wrap > div.tabs._center > a:nth-child(1) > div > div.tab-content > div.tab-weather > div.js_meas_container.temperature.tab-weather__value > span.unit.unit_temperature_c > span");
                    var weatherBrest = elementBrest.TextContent;
                    await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                                                       $"{weatherBrest}");
                    break;
                case "Витебск":
                    var htmlVitebsk = await client.GetStringAsync("https://www.gismeteo.by/weather-vitebsk-4218/");
                    var documentVitebsk = await htmlParser.ParseDocumentAsync(htmlVitebsk);
                    var elementVitebsk = documentVitebsk.QuerySelector("body > section > div.content_wrap > div > div.main > div > div.__frame_sm > div.forecast_frame.hw_wrap > div.tabs._center > a:nth-child(1) > div > div.tab-content > div.tab-weather > div.js_meas_container.temperature.tab-weather__value > span.unit.unit_temperature_c > span");
                    var weatherVitebsk = elementVitebsk.TextContent;
                    await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                                                       $"{weatherVitebsk}");
                    break;
                default:
                    break;

                    
            }

        }

      

            private static async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };

            await Bot.AnswerInlineQueryAsync(inlineQuery.Id,
                                             results,
                                             isPersonal: true,
                                             cacheTime: 0);
        }

        private static Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
            return Task.CompletedTask;
        }

        

        private static Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
