using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using System.Net.NetworkInformation;

namespace NineGag
{
    public enum PageType
    {
        Hot,
        Trending,
        TopDay,
        TopWeek,
        TopMonth,
        TopAll
    };

    public enum GagType
    {
        Hot,
        YouTube
    };

    public enum DownloadType
    {
        Previous,
        Next,
        Page
    };

    public class NineGagPage
    {
        //private properties
        private readonly List<GagItem> _gags;
        private HtmlDocument _document;
        
        //public string properties
        public string Link { get; set; }
        public string PreviousPage { get; set; }
        public string NextPage { get; set; }
        public string Id { get; set; }
        public string FirstPageId { get; set; }
        
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

        public PageType Type { get; set; }

        public NineGagPage(List<GagItem> gags)
        {
            _gags = gags;
            GagCount = _gags.Count;
        }

        public NineGagPage()
        {
            Link = "http://www.9gag.com";
            _document = new HtmlDocument(); //allocate memory to the _document
            _gags = new List<GagItem>();
            IsLoaded = false;
        }

        //public int properties
        public int GagCount { get; set; }

        public int CurrentImageId { get; set; }

        public bool IsLoaded { get; set; }
        
        //public methods
        public void Restart()
        {
        }

        public void LoadGags()
        {
            if (_document == null)
                throw new ArgumentException("Not Connected");
            IEnumerable<HtmlNode> nodes; //create a variable for storing the nodes in the doc
            try
            {
                nodes = _document.DocumentNode.DescendantNodesAndSelf()
                    .Where(n => n.Name == "li")
                    .Where(n => n.GetAttributeValue("class", null) == " entry-item");
                //now, nodes has all the gags in it
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Null");
                Restart();
                return;
            }
            try
            {
                foreach (HtmlNode node in nodes)
                {
                    var gagItem = new GagItem
                                      {
                                          URL = node.Attributes["data-url"].Value,
                                          TextDescription = node.Attributes["data-text"].Value,
                                          Id = node.Attributes["gagId"].Value
                                      };

                    string[] type = node.Descendants("div").Select(
                        x => x.GetAttributeValue("class", "invalid")).ToArray(); //determine the type of GagItem
                    int i = 0;
                    if (type.Any() && type[i] != "invalid")
                        while (i < type.Count())
                        {
                            if (type[i] == "img-wrap")
                            {
                                //gagItem.Type = Type;
                                i = 99;
                                break;
                            }
                            if (type[i] == "video-post")
                            {
                                gagItem.Type = GagType.YouTube;
                                i = 99;
                                break;
                            }
                            i++;
                        }
                    if (gagItem.Type != GagType.YouTube) //if the GagItem it's an image
                    {
                        string[] imageLink = node.Descendants("img").Select(
                            x => x.GetAttributeValue("src", null)).ToArray(); //get it's image link
                        if (imageLink.Any())
                            gagItem.ImageLink = imageLink[0];
                        try
                        {
                            gagItem.Image = new BitmapImage(new Uri(gagItem.ImageLink, UriKind.RelativeOrAbsolute));
                            //create the image with the link
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Image wasn't created");
                            Restart();
                            throw;
                        }
                    }
                    else
                    {
                        //get the link of the video and get it's thumbnail
                        string[] videoLink = node.Descendants("embed").Select(
                            x => x.GetAttributeValue("src", "invalid")).ToArray();
                        if (videoLink.Any() && videoLink[0] != "invalid")
                            gagItem.ImageLink = videoLink[0];
                        string url = "http://img.youtube.com/vi/VIDEOID/0.jpg";
                        i = 0;
                        string tmp = gagItem.ImageLink;
                        tmp = tmp.Replace("/v/", "/watch?v=");
                        while (tmp[i] != '=') i++; //go with the index to the video ID
                        tmp = tmp.Remove(0, i + 1);
                        i = 0;
                        while (tmp[i] != '&') i++; //from here, go the end of the video ID
                        tmp = tmp.Substring(0, i); //copy the video ID in tmp
                        url = url.Replace("VIDEOID", tmp); //replace VIDEOID with the ID
                        try
                        {
                            gagItem.Image = new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute));
                            //copy the thumbnail in the image
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Thumbnail wasn't created");
                            Restart();
                        }
                    }
                    gagItem.User = "User"; //User not yet implemented
                    _gags.Add(gagItem); //add the gagitem to the list
                }
                if (_gags != null && _gags.Count > 0) //if all went well, carry on...
                {
                    GagCount = _gags.Count;
                }
                else
                {
                    //if no gags we're found, get out
                    MessageBoxResult result = MessageBox.Show("Error occurred while loading gags. Try again?", "ERROR",
                                                              MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                        LoadGags();
                    else
                    {
                        IsLoaded = false;
                        throw new ArgumentNullException();
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception is ArgumentNullException)
                    throw;
            }
        }

        private bool Connected()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        public void Reset()
        {
            _gags.Clear();
            GagCount = 0;
            CurrentImageId = 0;

            IsLoaded = false;
        }

        public void Load()
        {
            DateTime dateTime = DateTime.Now;
            int millisecond = dateTime.Millisecond;
            string tmp = Link;
            tmp += "?=" + millisecond.ToString();
            HtmlWeb.LoadAsync(Link, (sender, doc) =>
                                        {
                                            _document = doc.Document ?? null;
                                            IsLoaded = true;
                                        }
                );
        }

        public bool SaveState()
        {
            return true;
        }

        public bool LoadState()
        {
            return true;
        }

        public async void GetFirstPage(PageType type)
        {
            var firstPage = false;
            if (!Connected())
                return;
            try
            {
                string pageType = "";
                switch (Type)
                {
                    case PageType.Hot:
                        pageType = "hot";
                        break;
                    case PageType.Trending:
                        pageType = "trending";
                        break;
                }
                
                if(pageType.Any())
                {
                    string link = "http://9gag.com/" + pageType;
                    string result =
                        await new WebClient().DownloadStringTaskAsync(new Uri(link, UriKind.RelativeOrAbsolute));
                    _document.LoadHtml(result);
                    HtmlNode[] firstLinks = _document.DocumentNode.DescendantNodesAndSelf()
                        .Where(n => n.Name == "a")
                        .Where(n => n.GetAttributeValue("class", null) == "next").ToArray();
                    if (firstLinks.Any())
                    {
                        string id = firstLinks.ElementAt(0).Attributes["href"].Value;

                        link += id;

                        while (firstPage == false)
                        {
                            link = GetIdFromLink(link);
                            int pageId;
                            if (int.TryParse(link, out pageId) == false)
                                link = "0";
                            pageId++;
                            link = "http://9gag.com/" + pageType + "/";
                            link += pageId.ToString();
                            string doc =
                                await new WebClient().DownloadStringTaskAsync(new Uri(link, UriKind.RelativeOrAbsolute));
                            _document.LoadHtml(doc);
                            try
                            {
                                HtmlNode[] pageLinks = _document.DocumentNode.DescendantNodesAndSelf()
                                    .Where(n => n.Name == "a")
                                    .Where(n => n.GetAttributeValue("class", null) == "previous").ToArray();
                                string prevId = "";
                                if (pageLinks.Any())
                                    prevId = pageLinks.ElementAt(0).Attributes["href"].Value;
                                if (prevId.Length > 1)
                                {
                                    FirstPageId = prevId;
                                }
                                else
                                {
                                    firstPage = true;
                                    Link = "http://9gag.com" + FirstPageId;
                                    Id = GetIdFromLink(Link);
                                    Load();
                                    return;
                                }
                            }
                            catch
                            {
                                firstPage = true;
                                Link = "http://9gag.com";
                                FirstPageId = "/hot/0";
                            }
                        }
                    }
                }
            }
            catch
            {
                Link = "http://9gag.com";
                FirstPageId = "/hot/0";
            }
        }
        
        //private methods
        public string GetIdFromLink(string link)
        {
            string[] words = link.Split('/');
            int i;
            return words.FirstOrDefault(word => Int32.TryParse(word, out i));
        }
    }
}