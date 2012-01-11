using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
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
        private readonly List<GagItem> _gags;
        private HtmlDocument _document;


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
            HtmlWeb.LoadAsync(Link, (sender, doc) => { _document = doc.Document; //copy the Downloaded document
            }
                );
        }

        public string Link { get; set; }
        public string PreviousPage { get; set; }
        public string NextPage { get; set; }
        public string Id { get; set; }

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

        public void Restart()
        {
        }

        private void LoadGags()
        {
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
                        while (true)
                        {
                            if (type[i] == "img-wrap")
                            {
                                gagItem.Type = Type;
                                i = 99;
                                break;
                            }
                            if (type[i] == "video-post")
                            {
                                gagItem.Type = GagType.Youtube;
                                i = 99;
                                break;
                            }
                            i++;
                        }
                    if (i != 99)
                        MessageBox.Show("Error");
                    if (gagItem.Type != GagType.Youtube) //if the GagItem it's an image
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
                    IsLoaded = true;
                }
                else
                {
                    //if no gags we're found, get out
                    MessageBoxResult result = MessageBox.Show("Error occured while loading gags. Try again?", "ERROR",
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

        public string GetGagLink()
        {
            return null;
        }

        public string GetPreviousLink()
        {
            try
            {
                HtmlNode[] nextLink = _document.DocumentNode.DescendantNodesAndSelf()
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
                HtmlNode[] nextLink = _document.DocumentNode.DescendantNodesAndSelf()
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
                string[] imgLink = _document.DocumentNode.Descendants("img").Select(
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
            var gagItem = new GagItem();
            string tmp = GetImageLink();
            if (tmp == "Null Error" || tmp == null)
                MessageBox.Show("Try again please");
            else
                gagItem.URL = tmp;
            GetIdFromLink(Link);
            gagItem.Type = Type;
        }

        public void LoadPage(string link)
        {
            HtmlWeb.LoadAsync(link, (sender, completed) => { _document = completed.Document; });
        }


        public void Load()
        {
            LoadGags();
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
                            link = "http://9gag.com/hot/";
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                throw;
            }
        }

/*
        private void ClientDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            MessageBox.Show("downloaded");
            IsLoaded = true;
            _document.LoadHtml(e.Result);
        }
*/

        private async void DownloadPage(string url)
        {
            var client = new WebClient();
            //client.DownloadStringCompleted += ClientDownloadStringCompleted;
            try
            {
                IsLoaded = false;
                string result = await client.DownloadStringTaskAsync(new Uri(url, UriKind.RelativeOrAbsolute));
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
                    exception is ThreadAbortException)
                {
                    throw;
                }
            }
        }

        private string GetIdFromLink(string link)
        {
            string[] words = link.Split('/');
            int i;
            return words.FirstOrDefault(word => Int32.TryParse(word, out i));
        }
    }
}