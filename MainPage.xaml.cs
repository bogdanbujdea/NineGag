using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace NineGag
{
    public partial class MainPage
    {
        private string _top;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            var items = new List<FunType>
                            {
                                new FunType {Name = "Hot Page"},
                                new FunType {Name = "Trending Page"},
                                new FunType {Name = "Top 24"},
                                new FunType {Name = "Top Week"},
                                new FunType {Name = "Top Month"},
                                new FunType {Name = "Top All"}
                            };
            _top = "hot";
            FunMenu.ItemsSource = items;
        }




        private void FunTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            
            try
            {
                if (FunMenu != null && FunMenu.ItemContainerGenerator != null)
                {
                    var selectedItem =
                        FunMenu.ItemContainerGenerator.ContainerFromItem(FunMenu.SelectedItem) as ListPickerItem;

                    if (selectedItem != null)
                    {
                        var data = selectedItem.DataContext as FunType;

                        if (data != null)
                        {
                            switch (data.Name)
                            {
                                case "Hot Page":
                                    _top = "hot";
                                    break;
                                case "Trending Page":
                                    _top = "trending";
                                    break;
                                case "Top 24":
                                    _top = "day";
                                    break;
                                case "Top Week":
                                    _top = "week";
                                    break;
                                case "Top Month":
                                    _top = "month";
                                    break;
                                case "Top All":
                                    _top = "all";
                                    break;
                                default:
                                    MessageBox.Show("Wrong Selection");
                                    return;
                            }
                            
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Can't change page. Please try again");
                
            }
        }

      

        private void PhoneApplicationPageBackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void BtnGoClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.Navigate(new Uri("/GagsPage.xaml?Type=" + _top, UriKind.RelativeOrAbsolute)) != true)
                MessageBox.Show("Can't change page");
        }

        private void BtnHelpClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "1. You have many ways to browse 9gag.\r\n2.Drag the image to the left or to the right in order to browse the site\r\n3.The app has pinch-to-zoom :)\r\n4.There is a small button in the bottom-right corner with a troll face icon, use it to save pictures from 9gag\r\n5.This is not an official 9gag app");
        }

       
    }

   
}