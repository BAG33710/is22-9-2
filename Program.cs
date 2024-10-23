using System.Linq.Expressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
//Библиотеки 
//Тестовый бот запущен на @sielom_paralegal_bot
class Program //начало программы
{

    private static ITelegramBotClient? _botClient;
    private static ReceiverOptions? _receiverOptions;
    static async Task Main()
    {
        _botClient = new TelegramBotClient("7930979593:AAFfWN4lmMYZFmJoEUqTIv--DRWLZOpaQP4"); /* Если вы хотите потестить бота, то измените ключ. Ключ можно получить в @BotFather(пропишите /newbot, после name бота, а дальше и тег (@test_bot или че то подобное) удачи)*/
        _receiverOptions = new ReceiverOptions();

        using var cts = new CancellationTokenSource();

        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"{me.FirstName} запущен!"); /* бот пишет в консольку имя и то что он запущен*/
        await Task.Delay(-1);
    }

    static async Task<Task> ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken) /*функция с ошибками(API ключа)*/
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
            => $"Telegram API Error:n[{apiRequestException.ErrorCode}]n{apiRequestException.Message}",
            _ => error.ToString()
        };
        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) // ОСНОВНАЯ ЧАСТЬ БОТА
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    {

                        var message = update.Message;
                        var user = message?.From;
                        var chat = message?.Chat;

                        Console.WriteLine($"{user?.FirstName} ({user?.Id}) написал(а) сообщение: {message?.Text}"); /*Это выводит что пользователь вводит*/
                        switch (message?.Type)
                        {
                            case MessageType.Text:
                                {
                                    if (message.Text == "/start") /*О, инлайн кейборд. Эта короче кнопка под сообщениями, дальше будет прописано как применять*/
                                    {
                                        InlineKeyboardMarkup firstmenu = new(new[]
                                        {
                                            new []  {InlineKeyboardButton.WithCallbackData(text: "Войти в бота", callbackData: "auth")},
                                        }); /*Это создание кнопки. И ещё Callback - САМАЯ ВАЖНАЯ ВЕЩЬ В КНОПКАХ*/
                                        await botClient.SendTextMessageAsync(chat.Id, "Необходимо авторизоваться!", replyMarkup: firstmenu);
                                        return;
                                    }

                                    return;
                                }
                            case MessageType.Contact: //Кейс который отвечает за наличие контакта
                                {
                                    var contact = message?.Contact;
                                    try
                                    {
                                        await botClient.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId - 1, cancellationToken: cancellationToken);
                                    }
                                    catch (Exception e)
                                    {
                                        await botClient.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId - 1, cancellationToken: cancellationToken);
                                    }
                                    Console.WriteLine($"Пользователь {user?.FirstName} поделился информацией: Вот его номер:({contact?.PhoneNumber})");
                                    InlineKeyboardMarkup mainmenu = new(new[]
                                    {new []  {InlineKeyboardButton.WithCallbackData(text: "👤Мой профиль", callbackData: "profile")}, /*Тут и так понятно, callbackData: "mainmenu" - вернёт вас в эту менюшку, ОБРАТИТЕ ВНИМАНИЕ НА НАЗВАНИЯ!! */
                                        new []  {InlineKeyboardButton.WithCallbackData(text: "📝Текущая успеваемость", callbackData: "usp")},
                                        new []  {InlineKeyboardButton.WithWebApp("🗓Расписание", new WebAppInfo() {Url = "https://sielom.ru/schedule"})},
                                        new []  {InlineKeyboardButton.WithCallbackData(text: "📖Зачетная книжка", callbackData: "zach")},
                                        new []  {InlineKeyboardButton.WithCallbackData(text: "📬Предложения", callbackData: "predloz")},
                                        new []  {InlineKeyboardButton.WithCallbackData(text: "⚙️Настройки", callbackData: "settings")},
                                        new []  {InlineKeyboardButton.WithCallbackData(text: "💔Выход", callbackData: "exit")}
                                    });
                                    try
                                    {
                                        await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, $"Чем я могу вам помочь?", replyMarkup: mainmenu);
                                    }
                                    catch (Exception e)
                                    {
                                        await botClient.SendTextMessageAsync(chat.Id, $"Чем я могу вам помочь?", replyMarkup: mainmenu);
                                    }
                                }
                                return;
                        }

                    }
                    return;


                case UpdateType.CallbackQuery: /*Самое интересное что тут есть >:)*/
                    {
                        InlineKeyboardMarkup back = new(new[]
                        {
                            new []  {InlineKeyboardButton.WithCallbackData(text: "↩️Вернуться в главное меню", callbackData: "mainmenu")}
                        }); /*Это кнопка которая вернёт тебя назад в обычное меню */
                        
                        var callbackQuery = update.CallbackQuery;
                        var message = callbackQuery?.Message;
                        var user = callbackQuery?.From;
                        var chat = callbackQuery?.Message?.Chat;
                        var contact = message?.Contact;

                        if (message?.Text != null || user != null || chat != null)
                        {
                            Console.WriteLine($"{user?.FirstName} ({user.Id}) нажал на кнопку: {callbackQuery?.Data}"); /*Если пользователь нажал кнопку то будет написанно на что он нажал и id пользователя*/
                        }

                        bool firstMessageReceived = false;

                        if (update.CallbackQuery != null && update.CallbackQuery.Data == "predloz") /*Обязательеные условия, но они работать не будут тк. не успел реализовать функцию отправки предложений(кто починит дам 50 рублей)*/
                        {
                            await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, "Напишите какую функцию вы хотели бы получить в личном кабинете?", replyMarkup: back);
                        }

                        if (update.Message != null && !firstMessageReceived)
                        {
                            firstMessageReceived = true;
                        }
                        if (update.Message != null && update.Message.Text != null && firstMessageReceived)
                        {
                            await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, "Спасибо за ваше сообщение!", replyMarkup: back);
                        }

                        switch (callbackQuery?.Data) //это свитч который выводит сообщения по callback
                        {
                            case "mainmenu":
                                {
                                    InlineKeyboardMarkup mainmenu = new(new[]
                                    {new []  {InlineKeyboardButton.WithCallbackData(text: "👤Мой профиль", callbackData: "profile")}, /*Тут и так понятно, callbackData: "mainmenu" - вернёт вас в эту менюшку, ОБРАТИТЕ ВНИМАНИЕ НА НАЗВАНИЯ!! */
                                    new []  {InlineKeyboardButton.WithCallbackData(text: "📝Текущая успеваемость", callbackData: "usp")},
                                    new []  {InlineKeyboardButton.WithWebApp("🗓Расписание", new WebAppInfo() {Url = "https://sielom.ru/schedule"})},
                                    new []  {InlineKeyboardButton.WithCallbackData(text: "📖Зачетная книжка", callbackData: "zach")},
                                    new []  {InlineKeyboardButton.WithCallbackData(text: "📬Предложения", callbackData: "predloz")},
                                    new []  {InlineKeyboardButton.WithCallbackData(text: "⚙️Настройки", callbackData: "settings")},
                                    new []  {InlineKeyboardButton.WithCallbackData(text: "💔Выход", callbackData: "exit")}
                                    });
                                    await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, $"Чем я могу вам помочь?", replyMarkup: mainmenu);
                                    return;
                                };
                            case "profile":
                                {
                                    await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, $"Вы: {user.FirstName}\n\nВаш тег: {user.Username}\n\nВаш ID: {user.Id}\n\nНаш ID чата: {chat.Id}", replyMarkup: back);
                                    return;
                                }
                            case "usp":
                                {
                                    await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, "Ваши текущие предметы.\n\nПредметы показаны в формате:\n  Средний балл - Предмет\n н/о -  оценки ещё не выставлялись\n\nВыберите предмет, чтобы увидеть вашу успеваемость по нему:", replyMarkup: back);
                                    return;
                                }
                            case "zach":
                                {
                                    await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, "Выберите за какой период вы хотите увидеть информацию о зачетах:", replyMarkup: back);
                                    return;
                                }
                            case "settings":
                                {
                                    InlineKeyboardMarkup setmenu = new(new[]
                                    {new []  {InlineKeyboardButton.WithCallbackData(text: "👨‍👩‍👧Родительский контроль", callbackData: "rod_control")},
                                        new []  {InlineKeyboardButton.WithCallbackData(text: "↩️Вернуться в главное меню", callbackData: "mainmenu")}});
                                    await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, $"Что будем настраивать?", replyMarkup: setmenu);
                                    return;
                                }
                            case "rod_control":
                                {
                                    InlineKeyboardMarkup rodmenu = new(new[]
                                    {new []  {InlineKeyboardButton.WithCallbackData(text: "➕👤Добавить номера родителей", callbackData: "add_rod")},
                                        new []  {InlineKeyboardButton.WithCallbackData(text: "➖👤Очистить список номеров родителей", callbackData: "clear_rod")},
                                        new []  {InlineKeyboardButton.WithCallbackData(text: "↩️Вернуться к настройкам⚙️", callbackData: "settings")},
                                        new []  {InlineKeyboardButton.WithCallbackData(text: "↩️Вернуться в главное меню", callbackData: "mainmenu")},});
                                    await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, $"Выберите один из пунктов:", replyMarkup: rodmenu);
                                    return;
                                }
                            case "add_rod": // визуальное "добавление"
                                {
                                    InlineKeyboardMarkup add_rodi = new(new[]
                                    {new []  {InlineKeyboardButton.WithCallbackData(text: "↩️Вернуться к настройкам⚙️", callbackData: "settings")}});
                                    await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, "Сейчас вам необходимо отправить контакт одного из родителей, нажав на скрепочку и выбрав необходимый контакт в меню. Внимание! Можно добавить лишь 2 контакта.", replyMarkup: add_rodi);
                                    return;
                                }
                            case "clear_rod": // не привязывал бд, так что мне пофиг
                                {
                                    await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, "Бд отстутствует", replyMarkup: back);
                                    return;
                                }

                            case "auth": // не работает ваще
                                {
                                    await botClient.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId, cancellationToken: cancellationToken);
                                    ReplyKeyboardMarkup authorization = new(new[]
                                    {
                                    KeyboardButton.WithRequestContact("Поделиться контактом!")
                                    })
                                    { ResizeKeyboard = true, OneTimeKeyboard = true };

                                    await botClient.SendTextMessageAsync(chat.Id, $"Я не могу предоставить доступ к данным пока не узнаю кто их просит.\nПожалуйста поделитесь своим контактом что бы я мог определить вас по вашему номеру телефона\n\nКнопка снизу ввода сообщений, если у вас ее не появилось нажмите на кнопку 🎛 ", replyMarkup: authorization);
                                    break;
                                }

                            case "exit": // выход из проги
                                {
                                    InlineKeyboardMarkup firstmenu = new(new[]
                                    { new []  {InlineKeyboardButton.WithCallbackData(text: "Авторизоваться", callbackData: "auth")},});
                                    await botClient.EditMessageTextAsync(chat.Id, messageId: message.MessageId, "Необходимо авторизоваться!", replyMarkup: firstmenu);
                                    return;
                                }
                        }
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}// ну и все, конец. Если будут вопросы @mopcu4



