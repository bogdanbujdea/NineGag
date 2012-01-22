using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NineGag
{
    public class GagItem
    {
        private BitmapImage _image;
        private string _textDescription;
        public string Name { get; set; }
        public string Id { get; set; }
        public string TextDescription
        {
            get { return _textDescription; }
            set { _textDescription = value.Replace("&#039;", "'");
                _textDescription = _textDescription.Replace("&quot;", "\"");
            }
        }
        public string URL { get; set; }
        public string User { get; set; }
        public string ImageLink { get; set; }

        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                
            } }

        public GagType Type { get; set; }

        public double Height { get; set; }
        public double Width { get; set; }
        public Stretch StretchMode { get; set; }

        public void SetStretch()
        {
            Height = Image.PixelHeight;
            Width = Image.PixelWidth;
            if (Height < 600 && Width < 600)
                StretchMode = Stretch.Uniform;
            else
            {
                StretchMode = Stretch.Uniform;
            }
        }
    }
}