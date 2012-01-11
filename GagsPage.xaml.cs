using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace NineGag
{
    public partial class GagsPage : PhoneApplicationPage
    {
        private readonly NineGagPage Page;
        private int Index;

        public GagsPage()
        {
            InitializeComponent();
            Page = new NineGagPage();

            Index = 0;
        }

        public void LoadGag()
        {
            Index = Page.CurrentImageId;
            GagImage.Source = Page.GagItem.Image;
        }

        private void GagsPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationContext.QueryString.ContainsKey("Type"))
                {
                    string type = NavigationContext.QueryString["Type"];
                    // MessageBox.Show("Page type is " + type + "link=" + Page.Link);
                    if (Page == null)
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
                    //Page.GetFirstPage(GagType.Hot);
                    //MessageBox.Show("First id is: " + Page.FirstPageId);
                    //MessageBox.Show(Page.GetFirstPage(GagType.Hot));
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

        private void StartBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Page.Load();
                StartBtn.Visibility = Visibility.Collapsed;
                GagImage.Source = Page.GagItem.Image;
            }
            catch (ArgumentException)
            {
                StartBtn.ClickMode = ClickMode.Release;

                NavigationService.GoBack();
            }
        }
    }
}