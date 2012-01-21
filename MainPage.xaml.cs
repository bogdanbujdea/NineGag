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
        private string top;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            var items = new List<FunType>
                            {
                                new FunType() {Name = "Hot Page"},
                                new FunType() {Name = "Trending Page"},
                                new FunType() {Name = "Top 24"},
                                new FunType() {Name = "Top Week"},
                                new FunType() {Name = "Top Month"},
                                new FunType() {Name = "Top All"}
                            };
            top = "hot";
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
                                    top = "hot";
                                    break;
                                case "Trending Page":
                                    top = "trending";
                                    break;
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
                            
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Can't change page. Please try again");
                
            }
        }

      

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Type is " + top);
            if (NavigationService.Navigate(new Uri("/GagsPage.xaml?Type=" + top, UriKind.RelativeOrAbsolute)) != true)
                MessageBox.Show("Can't change page");
        }

       
    }

   
}