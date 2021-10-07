using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram_Bot
{
    public class NewItemPool
    {
        public static ListNotDBC listNotDBConnect;
        public static TelegramBotClient botClient;
        NewItemPool(ListNotDBC dBC, TelegramBotClient client, Parser Parser)
        {
            listNotDBConnect = dBC;
            botClient = client;
        }
        
        static  List<Row> CheckNewAndOddItems(Parser parser, ListNotDBC listNotDBC, int subscriptionID, out List<int> odds)
        {
            
            HashSet<int> itemIDs = listNotDBC.GetSubitems(subscriptionID);
            List<Row> rows = new List<Row>();

            HashSet<int> parserItems = new HashSet<int>();
            odds = new List<int>();
            Random random = new Random();
            foreach (var element in parser.items)
            {
                
                System.Threading.Tasks.Task.Delay(random.Next(0, 3));
                //System.Threading.Thread.Sleep(random.Next(0, 3));
                if (!itemIDs.Contains(element.ltemNumber)) 
                    rows.Add(element);
                parserItems.Add(element.ltemNumber);
            }
            foreach(int element in itemIDs)
            {
                if (!parserItems.Contains(element))
                    odds.Add(element);
            }
            rows.Distinct();
            return rows;
        }
        private static async Task<Tuple<int, bool>> CheckOneSubscriptionForNewItems(ListNotDBC.Subscription element, List<ListNotDBC.SubItem> subItems) 
        {
            await Task.Run(()=>{
                Random random = new Random();
                List<Row> newItems;
                DateTime start = DateTime.UtcNow;
                //Console.WriteLine("Checking Subscription N:\t" + element.SubscriptionID);
                //Console.WriteLine(string.Format("*Waited randomly ms\t{0}", v));
                Parser parser = new Parser(element.Link, false);
                List<int> oddItems = new List<int>();
                newItems = CheckNewAndOddItems(parser, listNotDBConnect, element.SubscriptionID, out oddItems);
                //Console.WriteLine(string.Format("\tNumber of New Items in Subscription N:{0}\t is\t{1}", element.SubscriptionID, newItems.Count));
                //Console.WriteLine(string.Format("\tNumber of Odd Items in Subscription N:{0}\t is\t{1}", element.SubscriptionID, oddItems.Count));
                ListNotDBC.SubItem subItem;
                for (int i = 0; i < newItems.Count; i++)
                {

                    System.Threading.Tasks.Task.Delay(random.Next(0, 3));
                    //System.Threading.Thread.Sleep(random.Next(0, 3));
                    if (newItems.Count > 1000)
                    {
                        
                        if (!listNotDBConnect.GetSubscriptionIsComplete(element.SubscriptionID))
                        break;
                    }
                        
                    subItem = new ListNotDBC.SubItem(Parser.ParseItemNumberFromLink(newItems[i].Link), element.SubscriptionID);
                    
                    subItem.SubscriptionTitle = element.Title;
                    subItems.Add(subItem);
                }
                List<ListNotDBC.SubItem> odds = new List<ListNotDBC.SubItem>();
                ListNotDBC.SubItem vv;
                foreach (var el in oddItems)
                {
                    vv = new ListNotDBC.SubItem(el, element.SubscriptionID);
                    odds.Add(vv);
                }
                listNotDBConnect.DeleteSubitems(odds);
                listNotDBConnect.UpdateLastCheckToNow(element.SubscriptionID);
                //Console.WriteLine(string.Format("\t\tCompleted checking Subscription N:{0}\t SubItems Added \t{1}", element.SubscriptionID, subItems.Count));
                //Console.WriteLine(string.Format("\t\tCompleted checking Subscription N:{0}\t SubItems deleted \t{1}", element.SubscriptionID, odds.Count));
                DateTime end = DateTime.UtcNow;
                TimeSpan timeDiff = end - start;

                Console.WriteLine(string.Format("\t\t\tTime elapsed Checking Subscription N:{0}\t\t ms\t{1}", element.SubscriptionID, Convert.ToInt32(timeDiff.TotalMilliseconds)));

            });
            return Tuple.Create(element.SubscriptionID, true);
        }
        private static async Task<int> NewSubItemsAsync(List<ListNotDBC.SubItem> _subItems)
        {
            
            DateTime startAll = DateTime.UtcNow;
            List<ListNotDBC.Subscription> subscriptions = new List<ListNotDBC.Subscription>();
            subscriptions = listNotDBConnect.GetSubscriptions();
            List<Row> newItems;// = new List<Row>();
            Random random = new Random();
            var series = Enumerable.Range(1, subscriptions.Count).ToList();
            var tasks = new List<Task<Tuple<int, bool>>>();
            for (int i = 0; i< subscriptions.Count; i++)
            {
                if(listNotDBConnect.GetSubscriptionIsComplete(subscriptions[i].SubscriptionID))
                    tasks.Add(CheckOneSubscriptionForNewItems(subscriptions[i], _subItems));
            }
            foreach (var task in await Task.WhenAll(tasks))
            {
                if (task.Item2)
                {
                    //Console.WriteLine("Ending Process {0}", task.Item1);
                }
            }

            Console.WriteLine("\n!!!!Subscriptions' check cycle completed!!!\n");
            //Console.WriteLine("\n!!!!Inserting new Items in db!!!\n");
            List<ListNotDBC.SubItem> subItems = _subItems.Distinct().ToList();
            listNotDBConnect.AddSubitems(subItems);
            //Console.WriteLine("\n!!!!Inserting new Items in db complete!!!\n");
            DateTime endAll = DateTime.UtcNow;
            TimeSpan timeDiffAll = endAll - startAll;
            int time_elapsed = Convert.ToInt32(timeDiffAll.TotalMilliseconds);
            _subItems = subItems;
            Console.WriteLine(string.Format("!!!Time elapsed Checking All Subscriptions: ms\t{0}!!!", time_elapsed));
            return 0;
        }
       
        public static int NewItemsCheck()
        {
            List<ListNotDBC.SubItem> subItems = new List<ListNotDBC.SubItem>();
            //List<ListNotDBC.SubItem> subItemsNoDupes;
            
            var task = NewSubItemsAsync(subItems);
            task.Wait();
            //Console.WriteLine("\n!!!***NewSubItemsFound:\t"+subItems.Count);
            int subscriberID;
            int chatID;
            string text;
            
            foreach (var element in subItems)
            {
                if (listNotDBConnect.GetSubscriptionIsComplete(element.SubscriptionID)) 
                {
                    subscriberID = listNotDBConnect.GetSubscriberIDbySubscriptionID(element.SubscriptionID);
                    chatID = listNotDBConnect.GetChatIDBySubscriberID(subscriberID);
                    
                    text = string.Format("Նոր հայտարարություն որոնման արդյունքներում\n{0}\n{1}", element.SubscriptionTitle, string.Format(@"https://www.list.am/item/{0}", element.ItemNumber));
                    
                    Program._MySendSimpleMessage(botClient, chatID, text, Program.GetButtons(), 1);
                    //Console.WriteLine("\nSending message to {0}:\n\t{1}", listNotDBConnect.GetNamebyChatID(chatID), text);
                }
            }
            return 0;
        }
    }
}
