using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using HtmlAgilityPack;

namespace NineGag
{
    public enum GagType
    {
        Hot,
        Trending,
        Vote,
        Youtube
    };

    public class NineGagPage
    {
        //public string properties
        public string Link { get; set; }
        public string PreviousPage { get; set; }
        public string NextPage { get; set; }
        public string Id { get; set; }
        private List<GagItem> _gags;
        private HtmlDocument document;
        

        public NineGagPage(List<GagItem> gags)
        {
            _gags = gags;
            GagCount = _gags.Count;
        }

        public NineGagPage()
        {
            Link = "http://www.9gag.com";
            document = new HtmlDocument();
            WebClient client = new WebClient();
            HtmlWeb.LoadAsync(Link, (sender, doc) =>
                                        {
                                            document = doc.Document;
                                            IsLoaded = true;
                                            MessageBox.Show("Public ctor loaded");
            })
            ;
        
            
            
        }

        public string GetGagLink()
        {
            return null;
        }

        public string GetPreviousLink()
        {
            try
            {
                var nextLink = document.DocumentNode.DescendantNodesAndSelf()
                        .Where(n => n.Name == "a")
                        .Where(n => n.GetAttributeValue("id", null) == "prev_post").ToArray();
                if (nextLink.Any())
                    return nextLink.ElementAt(0).Attributes["href"].Value;
            }
            catch (ArgumentNullException)
            {
                return "Null Error";
            }
            return null;
        }

        public string GetNextLink()
        {
            try
            {
                var nextLink = document.DocumentNode.DescendantNodesAndSelf()
                        .Where(n => n.Name == "a")
                        .Where(n => n.GetAttributeValue("id", null) == "next_post").ToArray();
                if (nextLink.Any())
                    return nextLink.ElementAt(0).Attributes["href"].Value;
            }
            catch (ArgumentNullException)
            {
                return "Null Error";
            }
            return null;
        }

        public string GetImageLink()
        {
            try
            {
                var imgLink = document.DocumentNode.Descendants("img").Select(
                        x => x.GetAttributeValue("src", "")).ToArray();
                if (imgLink.Any())
                    return imgLink[0];
            }
            catch (ArgumentNullException)
            {
                return "Null Error";
            }
            return null;
        }

        public void AddGag()
        {
            GagItem gagItem = new GagItem();
            string tmp = null;
            tmp = GetImageLink();
            if (tmp == "Null Error" || tmp == null)
                MessageBox.Show("Try again please");
            else
                gagItem.URL = tmp;
            tmp = GetIdFromLink(Link);

        }

        public void LoadPage(string link)
        {
            HtmlWeb.LoadAsync(link, (sender, completed) =>
                                        {
                                            document = completed.Document;
                                            
                                        });   
        }

        public GagItem GagItem
        {
            get
            {
                if (_gags.Count <= CurrentImageId || CurrentImageId < 0)
                {
                    throw new IndexOutOfRangeException("Gag Image with index " + CurrentImageId.ToString() +
                                                       " does not exist");
                }
                return _gags[CurrentImageId];
            }
            set { _gags[CurrentImageId] = value; }
        }

        public GagType Type { get; set; }

        //public int properties
        public int GagCount { get; set; }
        public int CurrentImageId { get; set; }
        public string FirstPageId { get; set; }
        public bool IsLoaded { get; set; }


        public void Load()
        {

        }

        public bool SaveState()
        {

            return true;
        }

        public bool LoadState()
        {
            return true;
        }

        public async void GetFirstPage(GagType type)
        {
            
            bool firstPage = false;
            try
            {
                if (Type == GagType.Hot)
                {
                    string link = "http://9gag.com";
                    string result = await new WebClient().DownloadStringTaskAsync(new Uri(link, UriKind.RelativeOrAbsolute));
                    document.LoadHtml(result);
                    var firstLinks = document.DocumentNode.DescendantNodesAndSelf()
                        .Where(n => n.Name == "a")
                        .Where(n => n.GetAttributeValue("class", null) == "next").ToArray();
                    if (firstLinks.Any())
                    {
                        string id = firstLinks.ElementAt(0).Attributes["href"].Value;

                        link += id;
                       
                        while (firstPage == false)
                        {
                            link = GetIdFromLink(link);
                            int pageId = 0;
                            if (int.TryParse(link, out pageId) == false)
                                link = "0";
                            pageId++;
                            link = "http://9gag.com/hot/";
                            link += pageId.ToString();
                            string doc = await new WebClient().DownloadStringTaskAsync(new Uri(link, UriKind.RelativeOrAbsolute));
                            document.LoadHtml(doc);
                            try
                            {
                                var pageLinks = document.DocumentNode.DescendantNodesAndSelf()
                                    .Where(n => n.Name == "a")
                                    .Where(n => n.GetAttributeValue("class", null) == "previous").ToArray();
                                string prevId = "";
                                if (pageLinks.Any())
                                    prevId = pageLinks.ElementAt(0).Attributes["href"].Value;
                                if (prevId.Length > 1)
                                    MessageBox.Show("prev id is" + prevId);
                                else
                                {
                                    FirstPageId = prevId;
                                    return;
                                }
                            }
                            catch (Exception exception)
                            {
                                firstPage = true;
                                MessageBox.Show(exception.Message + "  link=" + link);

                            }

                        }
                    }
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            

        }

        private void ClientDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            MessageBox.Show("downloaded");
            IsLoaded = true;
            document.LoadHtml(e.Result);
        }

        private async void DownloadPage(string Url)
        {
            var client = new WebClient();
            //client.DownloadStringCompleted += ClientDownloadStringCompleted;
            try
            {
                IsLoaded = false;
                string result = await client.DownloadStringTaskAsync(new Uri(Url, UriKind.RelativeOrAbsolute));
                MessageBox.Show("Downloading...");
                
            }
            catch (Exception exception)
            {
                if (exception is ArgumentNullException ||
                    exception is UriFormatException ||
                    exception is ArgumentException)
                {
                    Link = "http://www.9gag.com";
                    GetFirstPage(Type);
                    return;
                }
                if (exception is OutOfMemoryException)
                {
                    MessageBox.Show("The application is out of memory. Try to restart it!");
                }
                if (exception is StackOverflowException ||
                    exception is System.Threading.ThreadAbortException)
                {
                    throw;
                }
            }
        }

        private string GetIdFromLink(string link)
        {
            string[] words = link.Split('/');
            int i = 0;
            foreach (string word in words)
            {
                if (Int32.TryParse(word, out i) == true)
                    return word;
            }
            return null;
        }

    }
}