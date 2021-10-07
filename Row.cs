//#define TEST
using System.Globalization;
using System;
using System.Collections.Generic;
//using System.Windows.Forms;


namespace Telegram_Bot
{
    public class Row
    {
        string title;
        public string Title {
            get { return title; }   // get method
            set { title = value; }  // set method
        }

        string link;
        public string Link {
            get { return link; }   // get method
            set { link = value; }  // set method
        }
        int itemNumber;
        public int ltemNumber
        {
            get { return itemNumber; }   // get method
            set { itemNumber = value; }  // set method
        }
        public Row(string _title, string _link)
        {
            link = _link;
            title = _title;
            itemNumber = Parser.ParseItemNumberFromLink(link);
        }
        public Row(string _link)
        {
            link = _link;
            title = Parser.PageTitleByLink(link);
            itemNumber = Parser.ParseItemNumberFromLink(link);
        }
    }
    class RowEqualityComparer : IEqualityComparer<Row>
    {
        public bool Equals(Row r1, Row r2)
        {
            if (r2 == null && r1 == null)
                return true;
            else if (r1 == null || r2 == null)
                return false;
            else if (r1.ltemNumber == r2.ltemNumber || r1.Link == r2.Link)
                return true;
            else
                return false;
        }

        public int GetHashCode(Row rx)
        {
            int hCode = rx.ltemNumber;
            return hCode.GetHashCode();
        }
    }
}
