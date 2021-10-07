//#define TEST
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
//using System.Windows.Forms;


namespace Telegram_Bot
{
    public class Parser
    {
        HtmlWeb web;
        HtmlDocument doc;
        public HashSet<Row> items;
        public string title;
        public string textLink;
        public bool correctLink;
        public bool subscribe;
        
        public Parser(string tLink, bool Subscribe)
        {
            
            subscribe = Subscribe;
            textLink = tLink;
            web = new HtmlWeb();
            
            title = "Unknown Item";
            
            correctLink = false;
            items = new HashSet<Row>();
            IsRegexMatch irm_1 = new IsRegexMatch(@"https:\//www.list.am/*");
            IsRegexMatch irm_2 = new IsRegexMatch(@"https:\//www.list.am/item/*");
            correctLink = (irm_1.IsMatch(textLink) 
                //&& !irm_2.IsMatch(textLink) 
                && textLink != null 
                && textLink != "https://www.list.am/"
                && textLink != "https://www.list.am/category?q="
                && textLink != "https://www.list.am/category?q");
            if (!(irm_1.matches.Count==0))
                textLink = textLink.Substring(irm_1.matches[0].Index);
            if (correctLink)
            {
                doc = web.Load(textLink);
                title = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
                items = AllItemsInAllPages(textLink);
            }
        }
        public void UpdateTitle()
        {
            title = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
        }
        public static string PageTitleByLink(string textLink)
        {
            HtmlDocument doc;
            HtmlWeb web;
            web = new HtmlWeb();
            string Output = "";
            try
            {
                doc = web.Load(textLink);
                Output = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
            }
            catch { }
            return Output;
        }
        public static HashSet<Row> AllItemsInAllPages(string _textLink)
        {
            HashSet<Row> items = new HashSet<Row>();
            HtmlDocument _doc;
            HtmlWeb _web;
            bool correctLink = false;
            IsRegexMatch irm_1 = new IsRegexMatch(@"https:\//www.list.am/*");
            correctLink = (irm_1.IsMatch(_textLink) && _textLink != null);
            if (!(irm_1.matches.Count == 0))
                _textLink = _textLink.Substring(irm_1.matches[0].Index);
            if (correctLink)
            {
                _web = new HtmlWeb();
                IsRegexMatch irm_2 = new IsRegexMatch(@"/item/*");
                
                string templink = _textLink;
                string linkString;
                Row row;
                Random random = new Random();
                while (templink != string.Empty)
                {
                    _doc = _web.Load(templink);

                    System.Threading.Tasks.Task.Delay(random.Next(0, 30));
                    //System.Threading.Thread.Sleep(random.Next(0, 10));
                    //Console.WriteLine("Loading:\t" + templink);
                    var nodes = _doc.DocumentNode.SelectNodes("//a[@href]");
                    //check this!!!
                    for(int i = 0; i<nodes.Count; i++)
                    {
                        string hrefValue = nodes[i].GetAttributeValue("href", string.Empty);
                        if (irm_2.IsMatch(hrefValue))
                        {
                            linkString = @"https://www.list.am"+hrefValue;
                            items.Add(new Row(hrefValue, linkString));
                        }
                        
                    }
                    templink = string.Empty;
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (nodes[i].InnerHtml == "Հաջորդը >")
                        {
                            templink = @"https://www.list.am" + nodes[i].GetAttributeValue("href", string.Empty);
                            break;
                        }

                    }
                }
            }
            return items;
        }
        public static string ParseCategoryLink(string link)
        {
            string categoryLink = string.Empty;
            if(link != null)
            {
                
                IsRegexMatch irm_2 = new IsRegexMatch(@"/category/*");
                HtmlDocument _doc;
                HtmlWeb _web = new HtmlWeb();
                _doc = _web.Load(link);
                var nodes = _doc.DocumentNode.SelectNodes("//a[@href]");
                for (int i = 0; i < nodes.Count; i++)
                {
                    string hrefValue = nodes[i].GetAttributeValue("href", string.Empty);
                    if (irm_2.IsMatch(hrefValue) && hrefValue.Length>categoryLink.Length)
                        categoryLink = hrefValue;
                }
                categoryLink = @"https://www.list.am" + categoryLink;
            }

            return categoryLink;
        }
        public static int ParseItemNumberFromLink(string link)
        {
            int output = 0;
            IsRegexMatch irm = new IsRegexMatch(@"https:\//www.list.am/item/*");
            if (irm.IsMatch(link))
            {
                output = int.Parse(link.Substring(irm.matches[0].Length));
            }
            return output;

        }

    }
}
