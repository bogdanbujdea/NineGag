using System.Windows.Media;

namespace NineGag
{
    public class GagItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string TextDescription { get; set; }
        public string URL { get; set; }
        public string User { get; set; }

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
