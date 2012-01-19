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
            var items = new List<TopType>();
            items.Add(new TopType() { Name = "Top 24" });
            items.Add(new TopType() { Name = "Top Week" });
            items.Add(new TopType() { Name = "Top Month" });
            items.Add(new TopType() { Name = "Top All" });
            TopMenu.ItemsSource = items;
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
                if (btn.Equals(btnHotPage))
                    pageType = "HotPage";
                else if (btn.Equals(btnTopPage))
                    //pageType = "TopPage";
                    return;
                else if (btn.Equals(btnTrendingPage))
                    pageType = "TrendingPage";
                else if (btn.Equals(btnVotePage))
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

        private void btnTopPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TopMenu.Visibility = Visibility.Visible;
                txtTop.Visibility = Visibility.Visible;
                btnGo.Visibility = Visibility.Visible;
                btnVotePage.Visibility = Visibility.Collapsed;
                btnHotPage.Visibility = Visibility.Collapsed;
                btnTopPage.Visibility = Visibility.Collapsed;
                btnTrendingPage.Visibility = Visibility.Collapsed;
            }
            catch 
            {
                
            }
        }

        private void TopTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            string top = "";
            try
            {
                if (TopMenu != null && TopMenu.ItemContainerGenerator != null)
                {
                    var selectedItem =
                        TopMenu.ItemContainerGenerator.ContainerFromItem(TopMenu.SelectedItem) as ListPickerItem;

                    if (selectedItem != null)
                    {
                        var data = selectedItem.DataContext as TopType;

                        if (data != null)
                        {
                            switch (data.Name)
                            {
                                case "Top 24":
                                    top = "day";
                                    break;
                                case "Top Week":
                                    top = "week";
                                    break;
                                case "Top Month":
                                    top = "month";
                                    break;
                                case "Top All":
                                    top = "all";
                                    break;
                                default:
                                    MessageBox.Show("Wrong Selection");
                                    return;
                            }
                            if (NavigationService.Navigate(new Uri("/GagsPage.xaml?Type=top&TopType=" + top, UriKind.RelativeOrAbsolute)) != true)
                                MessageBox.Show("Can't change page");
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Can't change page. Please try again");
                Reset();
            }
        }

        private void Reset()
        {
            TopMenu.Visibility = Visibility.Collapsed; //menu is collapsed
            txtTop.Visibility = Visibility.Collapsed; //txt is collapsed
            btnVotePage.Visibility = Visibility.Visible; //buttons are visible
            btnHotPage.Visibility = Visibility.Visible;
            btnTopPage.Visibility = Visibility.Visible;
            btnTrendingPage.Visibility = Visibility.Visible;
            btnGo.Visibility = Visibility.Collapsed;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {

        }

       
    }

   
}