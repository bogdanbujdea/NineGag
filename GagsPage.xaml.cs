using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace NineGag
{
    public partial class GagsPage : PhoneApplicationPage
    {
        private NineGagPage Page;
        private int Index;
        public GagsPage()
        {
            InitializeComponent();
            Page = new NineGagPage();
            Index = 0;
        }

        private void GagsPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationContext.QueryString.ContainsKey("Type"))
                {
                   
                    string type = NavigationContext.QueryString["Type"];
                   // MessageBox.Show("Page type is " + type + "link=" + Page.Link);
                    if(Page == null)
                        NavigationService.GoBack();
                    if (type == "HotPage")
                        Page.Type = GagType.Hot;
                    else if (type == "TrendingPage")
                        Page.Type = GagType.Trending;
                    else if (type == "VotePage")
                        Page.Type = GagType.Vote;
                    else if (type == "YouTubePage")
                        Page.Type = GagType.Youtube;
                    Page.PreviousPage = "FirstPage";
                    Page.CurrentImageId = 0;
                    MessageBox.Show(Page.GetFirstPage(GagType.Hot));
                }
                else
                {
                    NavigationService.GoBack();
                }
            }
            catch (ArgumentNullException)
            {
                NavigationService.GoBack();
            }
            
        }
    }
}