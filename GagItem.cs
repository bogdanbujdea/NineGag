using System.Windows.Media;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;

namespace NineGag
{
    public class GagItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string TextDescription { get; set; }
        public string URL { get; set; }
        public string User { get; set; }
        public string ImageLink { get; set; }
        private BitmapImage _image;
        public HtmlDocument Document { get; set; }
        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public GagType Type { get; set; }

        
        public GagItem()
        {
            
        }

        public int Height { get; set; }
        public int Width { get; set; }
        public Stretch StretchMode { get; set; }

        public void StretchItem()
        {
            
        }


    }
}
