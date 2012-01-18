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
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            btnHotPage.Click += new RoutedEventHandler(ChangePage);
            btnTopPage.Click += ChangePage;
            btnTrendingPage.Click += ChangePage;
            btnVotePage.Click += ChangePage;
        }

        private void ChangePage(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string pageType = "HotPage";
            try
            {
                if(btn.Equals(btnHotPage))
                    pageType = "HotPage";
                else if(btn.Equals(btnTopPage))
                    pageType = "TopPage";
                else if(btn.Equals(btnTrendingPage))
                    pageType = "TrendingPage";
                else if(btn.Equals(btnVotePage))
                    pageType = "VotePage";
                MessageBox.Show("Page type is " + pageType);
                if (NavigationService.Navigate(new Uri("/GagsPage.xaml?Type=" + pageType, UriKind.RelativeOrAbsolute)) != true)
                    MessageBox.Show("Can't change page");
            }
            catch
            {
                MessageBox.Show("Can't change page");
            }
        }
       

        
    }
}