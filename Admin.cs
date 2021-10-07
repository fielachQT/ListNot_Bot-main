//#define TEST
#define HANDLE_MESSAGES
using System.Collections.Generic;
//using System.Windows.Forms;


namespace Telegram_Bot
{
    public class Admin
    {
        public static List<Admin> AllAdmins = new List<Admin>();
        int level;
        long chatID; //field
        public long ChatID   // property
        {
            get { return chatID; }   // get method
            set { chatID = value; }  // set method
        }
        public int Level   // property
        {
            get { return level; }   // get method
            set { level = value; }  // set method
        }
        public static bool IsAdmin(long _chatID)
        {
            bool isAdmin = false;
            foreach(var element in AllAdmins)
            {
                if (element.ChatID.Equals(_chatID))
                {
                    isAdmin = true;
                    break;
                } 
            }
            return isAdmin;
        }
        public static List<Admin> AddAdmins(List<long> _chatIDs, int _level)
        {
            foreach(long element in _chatIDs)
            {
                Admin admin = new Admin(element, _level);
            }
            return AllAdmins;
        }
        public static List<Admin> AddAdmin(long _chatID, int _level)
        {
            Admin admin = new Admin(_chatID, _level);
            return AllAdmins;
        }
        private Admin(long _chatID, int _level = 1)
        {
            level = _level;
            chatID = _chatID;
            AllAdmins.Add(this);
        }
        private Admin()
        {
            AllAdmins.Add(this);
        }
        ~Admin()
        {
            AllAdmins.Remove(this);
        }
    }

}
