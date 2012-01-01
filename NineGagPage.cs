using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using HtmlAgilityPack;
using System.Xml;

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
                else
                {
                    return _gags[CurrentImageId];
                }
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
            string link, id;
            try
            {
                if (Type == GagType.Hot)
                {
                    link = "http://9gag.com";
                    string result;
                    result = await new WebClient().DownloadStringTaskAsync(new Uri(link, UriKind.RelativeOrAbsolute));
                    document.LoadHtml(result);
                    var firstLinks = document.DocumentNode.DescendantNodesAndSelf()
                        .Where(n => n.Name == "a")
                        .Where(n => n.GetAttributeValue("class", null) == "next").ToArray();
                    if (firstLinks.Any())
                    {
                        id = firstLinks.ElementAt(0).Attributes["href"].Value;

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
                            string doc;
                            doc = await new WebClient().DownloadStringTaskAsync(new Uri(link, UriKind.RelativeOrAbsolute));
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
            return "0";
        }

    }
}