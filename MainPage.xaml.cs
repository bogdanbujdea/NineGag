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
            
        }

        private void ChangePage(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if (NavigationService.Navigate(new Uri("/GagsPage.xaml", UriKind.RelativeOrAbsolute)) != true)
                    MessageBox.Show("Can't change page");
            }
            catch
            {
                MessageBox.Show("Can't change page");
            }
            
        }
       

        
    }
}