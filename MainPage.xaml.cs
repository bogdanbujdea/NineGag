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
            btnTrendingPage.Click += ChangePage;
            btnVotePage.Click += ChangePage;
            btnYouTubePage.Click += ChangePage;
        }

        private void ChangePage(object sender, RoutedEventArgs e)
        {
            string type = "WrongPage";
            if (sender.Equals(btnHotPage))
                type = "HotPage";
            else if (sender.Equals(btnTrendingPage))
                type = "TrendingPage";
            else if (sender.Equals(btnVotePage))
                type = "VotePage";
            else if (sender.Equals(btnYouTubePage))
                type = "YouTubePage";
            else
            {
                MessageBox.Show("WrongPage");
            }
            MessageBox.Show("Selected Page is " + type);
            try
            {
                if (NavigationService.Navigate(new Uri("/GagsPage.xaml?Type=" + type, UriKind.RelativeOrAbsolute)) != true)
                    MessageBox.Show("Can't change page");
            }
            catch (ArgumentException argumentException)
            {
                MessageBox.Show(argumentException.Message);
            }
            
            catch (UriFormatException uriFormat)
            {
                MessageBox.Show(uriFormat.Message);
            }
            
        }
       

        
    }
}