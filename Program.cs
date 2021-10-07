//#define TEST
#define HANDLE_MESSAGES
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using CsvHelper;
using System.IO;
using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System;
using System.Threading;
using HtmlAgilityPack;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using System.Configuration;
using System.Collections.Specialized;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types;
//using System.Windows.Forms;


namespace Telegram_Bot
{
    public class Program
    {
        private static string token { get; set; } = "1988969752:AAEYRPn7CSn5CkRjm-RmI64OFN4qrzWYEdk";
        private static TelegramBotClient client;
        private static ListNotDBC listNotDBConnect;
        public delegate int EngineDelegate();
        public static EngineDelegate myEngineDelegate;

        
#if TEST

        //test Main
        static void Main(string[] args)
        {
            string textLink = "https://www.list.am/category?q=hdd";
            int i = 0;
            foreach(var row in Parser.AllItemsInAllPages(textLink))
            {
                Console.WriteLine(i++ +"\t"+row.Link);
            }
            
            Console.ReadLine();
        }
#else
        //Main
        static void Main(string[] args)
        {
            //var stic = await client.SendPhotoAsync(chatId: message.Chat.Id, photo: @"C:\Users\Harut\Pictures\Fig vam.jpg");
            listNotDBConnect = new ListNotDBC();
            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            client.OnCallbackQuery += BotOnCallbackQueryReceived;
            
            Admin.AddAdmin(1044239457, 0); //HarutyunUmrshatyan
            //Admin.AddAdmin(1085185447, 1); //LilitUmrshatyan
            //Admin.AddAdmin(1930054747, 1); //StatikMkhitaryan
            //Admin.AddAdmin(1443761044, 2); //Anna
            

            NewItemPool.botClient = client;
            NewItemPool.listNotDBConnect = listNotDBConnect;

            myEngineDelegate = NewItemPool.NewItemsCheck;
            int delayms = int.Parse(ConfigurationManager.AppSettings.Get("Delay"));
            _MyTask task = new _MyTask(delayms);
            task.StartExecution(myEngineDelegate);
            foreach (var element in Admin.AllAdmins)
                _MySendSimpleMessage(client, element.ChatID, "ListNot բոտը ակտիվ է\nՁեզ տրվել է ադմինիստրատորի կարգ N:" + element.Level, GetButtons(), element.Level);
            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                    break;
            }
            task.CancelExecution();
            client.StopReceiving();


        }
        
#endif
    
#if HANDLE_MESSAGES

        private static async void BotOnCallbackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            long chatID = e.CallbackQuery.From.Id;
            string linkText = e.CallbackQuery.Message.Text;
            IsRegexMatch irm_1 = new IsRegexMatch(@"https:\//www.list.am/*");
            irm_1.IsMatch(linkText);
            if (!(irm_1.matches.Count == 0))
                linkText = linkText.Substring(irm_1.matches[0].Index);

            int subsctiptionID = listNotDBConnect.GetSubscriptionIDbyChatIDAndLink((int)chatID, linkText);
            listNotDBConnect.UnsubscribeDB(subsctiptionID);
            string msgID = e.CallbackQuery.Message.MessageId.ToString();
            //client.EditMessageReplyMarkupAsync(msgID, InlineKeyboardMarkup : GetEmptyInlineButtons());
            _MySendSimpleMessage(client, chatID, "Բաժանորդագրությունը չեղարկվել է", GetButtons(), 0);
        }

        private static object GetKeyboardRemove()
        {
            return new ReplyKeyboardRemove();
        }

        private static void OnMessageHandler(object sender, MessageEventArgs e)
        {
            MessageProcessing(sender, e);
        }

        private static IReplyMarkup GetInlineButtons(long _id)
        {
            return
                new InlineKeyboardMarkup
                (
                    new InlineKeyboardButton { Text = "Չեղարկել", CallbackData = _id.ToString()}
                );
            
             
        }
        public static IReplyMarkup GetButtons()
        {

            var r = new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { 
                        new KeyboardButton { Text = "Իմ բաժանորդագրությունները" }, 
                        new KeyboardButton { Text = @"Օգնություն" } 
                    }
                }
                
            };
            r.ResizeKeyboard = true;
            return r; 
        }
#endif
#if !TEST
        public static void _Notify(TelegramBotClient _client, string notificationString, IReplyMarkup _replyMarkup, int _adminNotificationLevel = -1)
        {
            foreach (var element in Admin.AllAdmins)
            {
                if (element.Level <= _adminNotificationLevel)
                {
                    _client.SendTextMessageAsync(element.ChatID, notificationString, replyMarkup: _replyMarkup);
                    //Console.WriteLine("**Notified " + element.ChatID);
                }
            }

        }
        public static void _MySendSimpleMessage(TelegramBotClient _client, long _chatID, string text, IReplyMarkup _replyMarkup, int _adminNotificationLevel = -1)
        {
            _client.SendTextMessageAsync(_chatID, text, replyMarkup: _replyMarkup);
            
            string mName = listNotDBConnect.GetNamebyChatID((int)_chatID);
            string notificationString = mName + "-ին ուղարկվել է \n"+text;
            //Console.WriteLine("*Messaged " + mName);
            foreach (var element in Admin.AllAdmins)
            {
                if(element.Level<=_adminNotificationLevel && _adminNotificationLevel>=0 && element.ChatID!= _chatID)
                {
                    _client.SendTextMessageAsync(element.ChatID, notificationString, replyMarkup: _replyMarkup);
                    //Console.WriteLine("**Notified " + element.ChatID);
                }
                    
            }
            
        }
        private static async void BotStartMessage(MessageEventArgs e)
        {
            string[] screenshotNames = new string[]
            {
                @"\Screenshot_1.jpg",
                @"\Screenshot_2.jpg",
                @"\Screenshot_3.jpg",
                @"\Screenshot_4.jpg",
                @"\Screenshot_5.jpg",
                @"\Screenshot_6.jpg",
                @"\Screenshot_7.jpg",
                @"\Screenshot_8.jpg",
                @"\Screenshot_9.jpg"
            };
            string ScreenshotsPath = ConfigurationManager.AppSettings.Get("ScreenshotsPath");
            foreach(var element in screenshotNames)
            {
                using (var stream = System.IO.File.OpenRead(ScreenshotsPath + element))
                {
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream);
                    await client.SendPhotoAsync(e.Message.Chat.Id, inputOnlineFile, replyMarkup: GetButtons());
                }
            }
            ListNotDBC.Subscriber subscriber = new ListNotDBC.Subscriber(e.Message.Chat.Id.ToString(), e.Message.Chat.FirstName + e.Message.Chat.LastName, -1);
            listNotDBConnect.AddSubsctiber(subscriber);
            string n_string = e.Message.Chat.FirstName + e.Message.Chat.LastName + "Started\nChatId: " + e.Message.Chat.Id;
            _Notify(client, n_string, GetButtons(), 1);
        }
        private static async void MessageProcessing(object sender, MessageEventArgs e)
        {
            await Task.Run(() => {
                var msg = e.Message;
                
                Parser parser = new Parser(msg.Text, true);
                if (msg.Text == @"/Start" || msg.Text == @"/start" || msg.Text == @"/START")
                {
                    BotStartMessage(e);
                }
                    
                else if (msg.Text == @"Օգնություն")
                {
                    string res = "Ողջույն" +
                        "\nList.am կայքում որոնեք Ձեզ հետաքրքրող հայտարարությունները՝ անհրաժեշտ ֆիլտրերով " +
                        "և ինձ ուղարկեք որոնման հղումը։ " +
                        "\n\nԵս մշտապես կփնտրեմ և Ձեզ կուղարկեմ բոլոր նոր հայտարարությունները:" +
                        "\n\nՀայտարարության հղում ուղարկելու դեպքում կկատարվի բաժանորդագրություն ամբողջ բաժնին՝ առանց ֆիլտրերի։" +
                        "\n\nՀարցերի, կարծիքների, առաջարկների կամ բողոքների դեպքում \nխնդրում ենք գրել էլ․ հասցեին list.not.bot@gmail.com";

                    //client.SendTextMessageAsync(msg.Chat.Id, res, replyMarkup: GetButtons());
                    _MySendSimpleMessage(client, msg.Chat.Id, res, _replyMarkup: GetButtons(), 0);


                }
                else if (msg.Text == "Իմ բաժանորդագրությունները")
                {
                    var _ss = listNotDBConnect.GetSubscriptionsByChatID((int)msg.Chat.Id);
                    if (_ss.Count == 0)
                        _MySendSimpleMessage(client, msg.Chat.Id, "Դուք դեռ չունեք բաժանորդագրություններ", GetButtons(), 0);
                    else
                        _MySendSimpleMessage(client, msg.Chat.Id, "Դուք ունեք "+ _ss.Count+" բաժանորդագրություն", GetButtons(), 0);
                    for (int i = 0; i<_ss.Count; i++)
                    {
                        string txt = _ss[i].Title + "\n" + _ss[i].Link;
                        _MySendSimpleMessage(client, msg.Chat.Id, txt, GetInlineButtons(msg.Chat.Id), 0);
                    }
                }
                else if (!parser.correctLink)
                {
                    
                    
                    //Wrong input -> search 
                        
                    string messageText = "Ճշգրիտ ֆիլտրեր կիրառելու համար ուղարկեք ինձ որոնման հղումը List.am կայքից";
                    _MySendSimpleMessage(client, msg.Chat.Id, messageText, GetButtons(), 0);
                    
                    

                }
                else
                {   _MySendSimpleMessage(client, msg.Chat.Id, "Խնդրում ենք սպասել․․․", GetButtons(), 0);
                    IsRegexMatch irm_2 = new IsRegexMatch(@"https:\//www.list.am/item/*");
                    if (irm_2.IsMatch(msg.Text))
                    {
                        //ուղարկվել է հայտարարություն
                        //_MySendSimpleMessage(client, msg.Chat.Id, "Խնդրում եմ ուղարկեք ինձ որոնման հղումը՝ միայն ոչ հայտարարության", GetButtons(), 0);
                        string categoryLink = Parser.ParseCategoryLink(msg.Text);
                        parser = new Parser(categoryLink, true);
                        string messageText = "Ճշգրիտ ֆիլտրեր կիրառելու համար ուղարկեք ինձ որոնման հղումը List.am կայքից";
                        _MySendSimpleMessage(client, msg.Chat.Id, messageText, GetButtons(), 0);
                    }
                   
                    if (NewSubscription(listNotDBConnect, msg.Chat.FirstName + msg.Chat.LastName, msg.Chat.Id.ToString(), parser) == 0)
                    {

                        string messageText = string.Format("Դուք բաժանորդագրվել եք \n{0}", parser.title) + "\n\nԵս կգտնեմ և Ձեզ կուղարկեմ այս պահից սկսած բոլոր նոր հայտարարությունները";
                        _MySendSimpleMessage(client, msg.Chat.Id, messageText, GetButtons(), 1);
                    }

                }
            });
        }
        static int NewSubscription(ListNotDBC listNotDBC, string chatName, string chatID, Parser par)
            {
                /*
                 * new subscription 
                 * 1. +check if subscriber is registered in db, add if not
                 * 2. +add new subscription
                 * 3. +insert SubItems
                 * 4. -add Alarm Log
                 */
                int ret = 0;
                int subscriberID = -1;
                //listNotDBC.SubscriberExists(chatName);
                ListNotDBC.Subscriber subscriber = new ListNotDBC.Subscriber(chatID, chatName, subscriberID);
                if (!listNotDBC.SubscriberExists(subscriberID))
                {
                    listNotDBC.AddSubsctiber(subscriber);
                }
                string sID = listNotDBC.GetSubscriberIDbyName(chatName);
                string subscriptionID = "";
                if (sID != null)
                {
                    try
                    {
                        subscriberID = int.Parse(sID);
                        ListNotDBC.Subscription subscription = new ListNotDBC.Subscription(null, par.textLink, subscriberID);
                        subscriptionID = listNotDBC.AddSubscriptionBySubscriberChatID(subscription, subscriber);
                        List<ListNotDBC.SubItem> items = new List<ListNotDBC.SubItem>();
                        
                        foreach(var element in par.items)
                        {
                            IsRegexMatch irm = new IsRegexMatch(@"https:\//www.list.am/item/*");
                            irm.IsMatch(element.Link);
                            string itemNumber = element.Link.Substring(irm.matches[0].Length);
                            ListNotDBC.SubItem subItem = new ListNotDBC.SubItem( int.Parse(itemNumber), int.Parse(subscriptionID));
                            items.Add(subItem);
                        }
                        listNotDBC.AddSubitems(items);
                        listNotDBC.SetSubscriptionIsComplete(int.Parse(subscriptionID), true);

                    }
                    catch { ret = 1; }


                }
                return ret;
            }
#endif




        }

    }
