using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public BitmapImage Image { get; set; }


        public GagType Type { get; set; }

        public double Height { get; set; }
        public double Width { get; set; }
        public Stretch StretchMode { get; set; }

        public void SetStretch()
        {
            if(Height < 750 && Width < 600)
                StretchMode = Stretch.Fill;
            else
            {

                StretchMode = Stretch.None;
            }
        }
    }
}