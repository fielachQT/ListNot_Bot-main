//#define TEST
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;

namespace Telegram_Bot
{
    public class ListNotDBC
    {
        public struct SubItem
        {
            public int SubscriptionID;
            public int ItemNumber;
            //public string PriceString;
            public string SubscriptionTitle;
            public SubItem(int itemNumber, int subscriptionID)
            {
                SubscriptionID = subscriptionID;
                ItemNumber = itemNumber;
                //PriceString = priceString;
                SubscriptionTitle = "";
            }
        }
        public struct Subscriber
        {
            public int SubscriberID;
            public string ChatID;
            public string Name;
            public Subscriber(string chatID = null, string name = null, int subscriberID = -1)
            {
                SubscriberID = subscriberID;
                ChatID = chatID;
                Name = name;
            }
        }
            public struct Subscription
            {
                public int SubscriptionID;
                public int SubscriberID;
                public string Link;
                public string LastCheck;
                public string Title;
                public Subscription(string lastCheck = null, string link = null, int subscriberID = -1, int subscriptionID = -1)
                {
                    SubscriberID = subscriberID;
                    SubscriptionID = subscriptionID;
                    Link = link;
                    LastCheck = lastCheck;
                    //Title = "Unknown";
                    Title = Parser.PageTitleByLink(Link);
                }
            }
        string connetionString;
        SqlConnection cnn;
        public void SetSubscriptionIsComplete(int subscriptionID, bool isComplete)
        {
            SqlCommand command;
            String sql;
            int _isComplete = 0; if (isComplete) { _isComplete = 1; }
            sql = string.Format("UPDATE [dbo].[Subscriptions] SET [IsComplete] = {0} WHERE SubscriptionID = {1}", _isComplete, subscriptionID);
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.UpdateCommand = new SqlCommand(sql, cnn);
            command.ExecuteNonQuery();
            command.Dispose();
            adapter.Dispose();
            
        }
        public bool GetSubscriptionIsComplete(int subscriptionID)
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql;
            bool Output = false;
            sql = string.Format("SELECT [IsComplete] FROM [ListNotDB].[dbo].[Subscriptions] WHERE SubscriptionID = {0}", subscriptionID);
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
                var v = dataReader["IsComplete"];
                string s = v.ToString();
                Output = bool.Parse(dataReader["IsComplete"].ToString());
            }
            command.Dispose();
            dataReader.Close();
            return Output;
        }
        public void UnsubscribeDB(int _subscriptionID)
        {
            string sql = string.Format("DECLARE @_subscriptionid AS INT={0} UPDATE[dbo].[Subscriptions] SET [IsComplete] = 0 WHERE SubscriptionID = @_subscriptionid DELETE FROM[dbo].[SubItems] WHERE SubscriptionID = @_subscriptionid DELETE FROM[dbo].[Subscriptions] WHERE SubscriptionID = @_subscriptionid", _subscriptionID);
            SqlCommand command;
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.UpdateCommand = new SqlCommand(sql, cnn);
            try
            {
                adapter.UpdateCommand.ExecuteNonQuery();
                command.Dispose();
            }
            catch { }
        }
        public int GetSubscriptionIDbyChatIDAndLink(int _chatID, string _link)
        {


            string sql = string.Format("SELECT [Subscriptions].[SubscriptionID] FROM[ListNotDB].[dbo].[Subscriptions] INNER JOIN Subscribers ON Subscriptions.SubscriberID = Subscribers.SubscriberID WHERE Link = '{0}' AND ChatID = {1}", _link, _chatID);
            SqlCommand command;
            SqlDataReader dataReader;
            command = new SqlCommand(sql, cnn);
            int output = 0;
            try
            {
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    output = int.Parse(dataReader["SubscriptionID"].ToString());
                }
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return output;
        }
        public List<Subscription> GetSubscriptionsByChatID(int _chatID)
        {
            List<Subscription> items = new List<Subscription>();
            
            string sql = "SELECT [SubscriptionID],[Link],[LastCheck],[IsComplete] FROM[ListNotDB].[dbo].[Subscriptions] INNER JOIN Subscribers ON Subscriptions.SubscriberID = Subscribers.SubscriberID WHERE ChatID = "+_chatID;
            SqlCommand command;
            SqlDataReader dataReader;
            command = new SqlCommand(sql, cnn);
            Subscription item = new Subscription();
            try
            {
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    item = new Subscription(dataReader["LastCheck"].ToString(), dataReader["Link"].ToString(), int.Parse(dataReader["SubscriptionID"].ToString()));
                    items.Add(item);
                }
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return items;
        }
        public List<Subscription> GetSubscriptions()
        {
            List<Subscription> items = new List<Subscription>();
            string sql = "SELECT [SubscriptionID],[SubscriberID],[Link],[LastCheck],[IsComplete] FROM [ListNotDB].[dbo].[Subscriptions]";
            SqlCommand command;
            SqlDataReader dataReader;
            command = new SqlCommand(sql, cnn);
            Subscription item = new Subscription();
            try
            {
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    item = new Subscription(dataReader["LastCheck"].ToString(), dataReader["Link"].ToString(), int.Parse(dataReader["SubscriberID"].ToString()), int.Parse(dataReader["SubscriptionID"].ToString()));
                    items.Add(item);
                }
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return items;
        }
        public HashSet<int> GetSubitems(int subscriptionID)
        {
            HashSet<int> items = new HashSet<int>();
            string sql = string.Format("SELECT [ItemNumber], [Price] FROM [ListNotDB].[dbo].[SubItems] WHERE SubscriptionID = {0}", subscriptionID);
            SqlCommand command;
            SqlDataReader dataReader;
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                items.Add(int.Parse(dataReader["ItemNumber"].ToString()));
            }
            try
            {
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return items;
        }
        public void DeleteSubitems(List<SubItem> subItems)
        {
            SqlCommand command = new SqlCommand();
            String sql;
            SqlDataAdapter adapter = new SqlDataAdapter();

            foreach (SubItem element in subItems)
            {
                sql = string.Format("DELETE FROM [dbo].[SubItems] WHERE ItemNumber = {0} AND SubscriptionID = {1}",element.ItemNumber, element.SubscriptionID);
                command = new SqlCommand(sql, cnn);
                adapter.InsertCommand = new SqlCommand(sql, cnn);
                try
                {
                    adapter.InsertCommand.ExecuteNonQuery();

                }
                catch { }
            }
            command.Dispose();
        }
        public void AddSubitems(List<SubItem> subItems)
        {
            SqlCommand command = new SqlCommand();
            String sql;
            SqlDataAdapter adapter = new SqlDataAdapter();


            //for(int i = 0; i<subItems.Count; i++)
            foreach (SubItem element in subItems)
            {
                sql = string.Format("INSERT INTO [dbo].[SubItems]([SubscriptionID], [ItemNumber]) VALUES ({0},{1})", element.SubscriptionID, element.ItemNumber);
                command = new SqlCommand(sql, cnn);
                adapter.InsertCommand = new SqlCommand(sql, cnn);
                try
                {
                    adapter.InsertCommand.ExecuteNonQuery();
                    
                }
                catch { }
            }
            command.Dispose();
        }
        public bool SubscriberExists(string name)
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql;
            bool Output = false;
            sql = "SELECT [Name] FROM [dbo].[Subscribers]";
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                if (dataReader.GetValue(0).ToString() == name)
                {
                    Output = true;
                    break;
                }
            }
            try
            {
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return Output;
        }
        public bool SubscriberExists(int subscriberID)
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql;
            bool Output = false;
            sql = "SELECT [SubscriberID] FROM [dbo].[Subscribers]";
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                if(dataReader.GetValue(0).ToString() == subscriberID.ToString())
                {
                    Output = true;
                    break;
                }
            }
            try
            {
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return Output;
        }
        public string GetSubscriberNameByID(int subscriberID)
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql, Output = "";
            sql = string.Format("SELECT  [Name] FROM [ListNotDB].[dbo].[Subscribers] WHERE [SubscriberID]= '{0}'", subscriberID);
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                Output = Output + dataReader.GetValue(0);
            }
            try
            {
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return Output;
        }
        public void UpdateLastCheckToNow(int subscriptionID)
        {
            SqlCommand command;
            String sql = string.Format("UPDATE [dbo].[Subscriptions] SET[LastCheck] = CURRENT_TIMESTAMP WHERE SubscriptionID = {0}", subscriptionID);
            command = new SqlCommand(sql, cnn);
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.UpdateCommand = new SqlCommand(sql, cnn);
            try
            {
                adapter.UpdateCommand.ExecuteNonQuery();
                command.Dispose();
            }
            catch { }
        }
        public int GetChatIDBySubscriberID(int subscriberID)
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql;
            int Output = -1;
            sql = string.Format("SELECT [ChatID] FROM [ListNotDB].[dbo].[Subscribers] WHERE SubscriberID = '{0}'", subscriberID);
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                Output = int.Parse(dataReader["ChatID"].ToString());
            }
            try
            {
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return Output;
        }
        public string GetNamebyChatID(int chatID)
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql;
            string Output = string.Empty;
            sql = string.Format("SELECT [Name] FROM [ListNotDB].[dbo].[Subscribers] WHERE ChatID = {0}", chatID);
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                Output = dataReader["Name"].ToString();
            }
            try
            {
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return Output;
        }
        public int GetSubscriberIDbySubscriptionID(int subscriptionID)
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql;
            int Output = -1;
            sql = string.Format("SELECT [SubscriberID] FROM [ListNotDB].[dbo].[Subscriptions] WHERE SubscriptionID = {0}", subscriptionID);
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                Output = int.Parse(dataReader["SubscriberID"].ToString());
            }
            try
            {
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return Output;
        }
        public string GetSubscriberIDbyName(string name)
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql, Output = "";
            sql = string.Format("SELECT  [SubscriberID] FROM [ListNotDB].[dbo].[Subscribers] WHERE [Name]= '{0}'", name);
            command = new SqlCommand(sql, cnn);
            try
            {
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    Output = Output + dataReader.GetValue(0);
                }
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return Output;
        }
        public void AddSubsctiber(Subscriber subscriber)
        {
            SqlCommand command;
            String sql;
            SqlDataAdapter adapter = new SqlDataAdapter();
            sql = string.Format("INSERT INTO [dbo].[Subscribers]([ChatID],[Name]) VALUES('{0}','{1}')", subscriber.ChatID, subscriber.Name);
            command = new SqlCommand(sql, cnn);
            adapter.InsertCommand = new SqlCommand(sql, cnn);
            try
            {
                adapter.InsertCommand.ExecuteNonQuery();
                command.Dispose();
            }
            catch { }
        }
        public String AddSubscriptionBySubscriberName(Subscription subscription, Subscriber subscriber)
        {
            SqlDataReader dataReader;
            SqlCommand command;
            String sql, Output = "";
            SqlDataAdapter adapter = new SqlDataAdapter();
            sql = string.Format("INSERT INTO [dbo].[Subscriptions]([SubscriberID],[Link],[LastCheck]) VALUES((SELECT[SubscriberID] FROM[dbo].[Subscribers] WHERE Name = '{0}'), '{1}', CURRENT_TIMESTAMP)", subscriber.Name, subscription.Link);
            command = new SqlCommand(sql, cnn);
            adapter.InsertCommand = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            if (dataReader.Read())
            {
               Output = Output + dataReader.GetValue(0);
            }
            try
            {
                adapter.InsertCommand.ExecuteNonQuery();
                command.Dispose();
            }
            catch { }
            return Output;
        }
        public string AddSubscriptionBySubscriberChatID(Subscription subscription, Subscriber subscriber)
        {
            String Output = "";
            SqlCommand command;
            SqlDataReader dataReader;
            String sql;
            SqlDataAdapter adapter = new SqlDataAdapter();
            sql = string.Format("INSERT INTO [dbo].[Subscriptions]([SubscriberID],[Link],[LastCheck]) VALUES((SELECT[SubscriberID] FROM[dbo].[Subscribers] WHERE ChatID = '{0}'), '{1}', CURRENT_TIMESTAMP); SELECT SCOPE_IDENTITY()", subscriber.ChatID, subscription.Link);
            command = new SqlCommand(sql, cnn);
            adapter.InsertCommand = new SqlCommand(sql, cnn);
            try
            {
                dataReader = command.ExecuteReader();
                if (dataReader.Read())
                {
                    Output = Output + dataReader.GetValue(0);
                }
                adapter.InsertCommand.ExecuteNonQuery();
                command.Dispose();
            }
            catch { }
            return Output;
        }
        public string GetSubscribersCount()
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql, Output = "";
            sql = "SELECT COUNT( [SubscriberID] ) FROM [dbo].[Subscribers]";
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                Output = Output + dataReader.GetValue(0);
            }
            try
            {
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return Output;
        }
        public string GetSubscriptionsCount()
        {
            SqlCommand command;
            SqlDataReader dataReader;
            String sql, Output = "";
            sql = "SELECT COUNT( [SubscriptionID] ) FROM [dbo].[Subscriptions]";
            command = new SqlCommand(sql, cnn);
            dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                Output = Output + dataReader.GetValue(0);
            }
            try
            {
                command.Dispose();
                dataReader.Close();
            }
            catch { }
            return Output;
        }
        private static string InitConnectionString()
        {
            return ConfigurationManager.AppSettings.Get("ConnectionString");
        }
        public ListNotDBC()
        {
            connetionString = @"Data Source=DESKTOP-84KJH2C\SQLEXPRESS;Initial Catalog=ListNotDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=true";
            connetionString = InitConnectionString();
            //connetionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            cnn = new SqlConnection(connetionString);
            cnn.Open();
            cnn.ChangeDatabase("ListNotDB");
            
        }
        ~ListNotDBC()
        {
            try { cnn.Close(); }
            catch { }
        }
        
    }
}
