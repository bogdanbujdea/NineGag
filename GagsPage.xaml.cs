using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using System.Net.NetworkInformation;
namespace NineGag
{
    public partial class GagsPage : PhoneApplicationPage
    {
        public enum BackgroundWork
        {
            LoadPreviousPage,
            LoadNextPage,
            LoadPage
        };

        private NineGagPage Page;
        private readonly BackgroundWorker _backgroundWorker;
        private BackgroundWork _work;
        public GagsPage()
        {
            InitializeComponent();
            Page = new NineGagPage();

            try
            {
                _work = BackgroundWork.LoadPage;
                Page.GetFirstPage(GagType.Hot);
            }
            catch (Exception exception)
            {
                if (exception is ArgumentException)
                {
                    MessageBox.Show("You are not connected to the internet. Please try again!");
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                }
                
            }

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;

            _work = BackgroundWork.LoadPage;
            _backgroundWorker.RunWorkerAsync();

            GestureListener gestureListener = GestureService.GetGestureListener(LayoutRoot);
            gestureListener.Flick += GestureListenerFlick;
            gestureListener.Tap += gestureListener_Tap;
            gestureListener.Hold += gestureListener_Hold;
            gestureListener.DragStarted += gestureListener_DragStarted;
            gestureListener.DragDelta += gestureListener_DragDelta;
            gestureListener.DragCompleted += gestureListener_DragCompleted;
        }

        private bool Connected()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                textBlock1.Visibility = Visibility.Collapsed;
                Page.Reset();
                Page.LoadGags();
                if (_work == BackgroundWork.LoadNextPage || _work == BackgroundWork.LoadPage)
                    Page.CurrentImageId = 0; //if we loaded the next page, then we load the first gag
                else Page.CurrentImageId = Page.GagCount - 1; //else, we load the last gag
                GagImage.Stretch = Stretch.None;
                GagImage.Source = Page.GagItem.Image;
                
            }
            catch (Exception exception)
            {
                if (exception is ArgumentException && exception.Message == "Not Connected")
                    MessageBox.Show("You are not connected to the internet. Please try again");
                else if (exception is IndexOutOfRangeException)
                    MessageBox.Show(exception.Message);
            }
        }
        
        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            if (!Connected())
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>

                                                          MessageBox.Show(
                                                              "You are not connected to the internet. Please connect to\r\n to internet and try again"));
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute)));
            }


            var text = "Loading";
            const string dot = ".";
            int count = 0;
            while (Page.IsLoaded == false)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => textBlock1.Text = text);

                Thread.Sleep(300);
                if (count < 3)
                {
                    count++;
                    text += dot;
                }
                else
                {
                    count = 0;
                    text = "Loading";
                }
                if (!Connected())
                    break;

            }
        }

        #region Image Gestures
        private void gestureListener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
        }

        private void gestureListener_DragDelta(object sender, DragDeltaGestureEventArgs e)
        {
        }

        private void gestureListener_DragStarted(object sender, DragStartedGestureEventArgs e)
        {
        }

        private void gestureListener_Hold(object sender, GestureEventArgs e)
        {
        }

        private void gestureListener_Tap(object sender, GestureEventArgs e)
        {
        }

        private void GestureListenerFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction != System.Windows.Controls.Orientation.Horizontal) return;
            if (e.Angle > 270 || e.Angle < 90) //Previous Image
            {
                try
                {
                    Page.CurrentImageId--;
                    if (Page.CurrentImageId == -1)
                    {
                        if(!Connected())
                            throw new ArgumentException();
                        string link = Page.Id;
                        int i;
                        if (Int32.TryParse(link, out i))
                        {
                            string tmp = "/hot/" + i;
                            if (tmp == Page.FirstPageId)
                            {
                                Page.CurrentImageId = 0;
                                MessageBox.Show("There are no newer gags");
                                return;
                            }
                            i++;
                            Page.Id = i.ToString();
                            Page.Link = "http://9gag.com/hot/" + Page.Id;
                            _work = BackgroundWork.LoadPreviousPage;
                            Page.IsLoaded = false;
                            textBlock1.Visibility = Visibility.Visible;
                            GagImage.Source = null;
                            Page.Load();
                            _backgroundWorker.RunWorkerAsync();
                        }
                    }
                    else if (Page.CurrentImageId >= 0)
                    {
                        GagImage.Stretch = Stretch.None;
                        GagImage.Source = Page.GagItem.Image;
                    }
                }
                catch (Exception exception)
                {
                    if (exception is IndexOutOfRangeException)
                        MessageBox.Show("There are no previous images");
                    else if (exception is ArgumentException)
                    {
                        MessageBox.Show("You are not connected to the internet!");

                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                    }
                    Page.CurrentImageId = 1;
                }
            }
            else //Next image
            {
                try
                {
                    Page.CurrentImageId++;
                    if (Page.CurrentImageId < Page.GagCount)
                    {
                        GagImage.Stretch = Stretch.None;
                        GagImage.Source = Page.GagItem.Image;
                    }
                    else
                    {
                        if (!Connected())
                            throw new ArgumentException();
                        string link = Page.Id;
                        int i;
                        if (Int32.TryParse(link, out i))
                        {
                            i--;
                            Page.Id = i.ToString();
                            Page.Link = "http://9gag.com/hot/" + Page.Id;
                            _work = BackgroundWork.LoadNextPage;
                            Page.IsLoaded = false;
                            textBlock1.Visibility = Visibility.Visible;
                            GagImage.Source = null;
                            Page.Load();
                            _backgroundWorker.RunWorkerAsync();
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (exception is ArgumentException)
                    {
                        MessageBox.Show("You are not connected to the internet!");

                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                    }
                    Page.CurrentImageId--;
                }
            }
        }
        #endregion

        private void GagsPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationContext.QueryString.ContainsKey("Type"))
                {
                    string type = NavigationContext.QueryString["Type"];
                    // MessageBox.Show("Page type is " + type + "link=" + Page.Link);
                    if (Page == null)
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                    else if (type == "HotPage")
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
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                }
            }
            catch (ArgumentNullException)
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        #region Pinch Events

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
        }

        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
        }

        private new void DoubleTap(object sender, GestureEventArgs e)
        {
        }
        #endregion

        

        private void GagImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                Page.GagItem.Height = GagImage.ActualHeight;
                Page.GagItem.Width = GagImage.ActualWidth;
                Page.GagItem.SetStretch();
                GagImage.Stretch = Page.GagItem.StretchMode;
                GagText.Text = Page.GagItem.TextDescription;
            }
            catch
            {
            }
        }
    }
}