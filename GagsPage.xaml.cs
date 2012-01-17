using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private double TotalImageScale = 1d;
        private Point ImagePosition = new Point(0, 0);


        private const double MAX_IMAGE_ZOOM = 5;
        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;

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
        }

        private bool Connected()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                txtLoading.Visibility = Visibility.Collapsed;
                GagText.Visibility = Visibility.Visible;
                Page.Reset();
                Page.LoadGags();
                if (_work == BackgroundWork.LoadNextPage || _work == BackgroundWork.LoadPage)
                    Page.CurrentImageId = 0; //if we loaded the next page, then we load the first gag
                else Page.CurrentImageId = Page.GagCount - 1; //else, we load the last gag
                //Load the image in the control
                GagImage.Source = Page.GagItem.Image;
                
            }
            catch (Exception exception)
            {
                if (exception is ArgumentException && exception.Message == "Not Connected")
                    MessageBox.Show("You are not connected to the internet. Please try again");
                else
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
            Deployment.Current.Dispatcher.BeginInvoke(() => GagText.Visibility = Visibility.Collapsed);

            var text = "Loading";
            const string dot = ".";
            int count = 0;
            while (Page.IsLoaded == false)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => txtLoading.Text = text);

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

        private void LoadImage()
        {
            try
            {
                ResetImagePosition();
                Page.GagItem.SetStretch();
                GagImage.Stretch = Page.GagItem.StretchMode;
                GagText.Text = Page.GagItem.TextDescription;
            }
            catch
            {
                
            }
        }

        #region Image Gestures
        
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
                            txtLoading.Visibility = Visibility.Visible;
                            GagImage.Source = null;
                            Page.Load();
                            _backgroundWorker.RunWorkerAsync();
                        }
                    }
                    else if (Page.CurrentImageId >= 0)
                    {
                        Page.GagItem.Image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        GagImage.Source = Page.GagItem.Image;
                        LoadImage();
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
                        ImageIsLoading = true;
                        Page.GagItem.Image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        GagImage.Source = Page.GagItem.Image;
                        LoadImage();
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
                            txtLoading.Visibility = Visibility.Visible;
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

        public bool ImageIsLoading { get; set; }
        

        #endregion

        //private void GagsPageLoaded(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (NavigationContext.QueryString.ContainsKey("Type"))
        //        {
        //            string type = NavigationContext.QueryString["Type"];
        //            if (Page == null)
        //                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        //            else if (type == "HotPage")
        //                Page.Type = GagType.Hot;
        //            else if (type == "TrendingPage")
        //                Page.Type = GagType.Trending;
        //            else if (type == "VotePage")
        //                Page.Type = GagType.Vote;
        //            else if (type == "YouTubePage")
        //                Page.Type = GagType.Youtube;
        //            Page.PreviousPage = "FirstPage";
        //            Page.CurrentImageId = 0;
        //        }
        //        else
        //        {
        //            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        //        }
        //    }
        //    catch (ArgumentNullException)
        //    {
        //        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        //    }
        //}

        
        
        #region Utils

        /// <summary>
        /// Computes the translation needed to keep the image centered between your fingers.
        /// </summary>
        private Point GetTranslationDelta(
            Point currentFinger1, Point currentFinger2,
            Point oldFinger1, Point oldFinger2,
            Point currentPosition, double scaleFactor)
        {
            var newPos1 = new Point(
             currentFinger1.X + (currentPosition.X - oldFinger1.X) * scaleFactor,
             currentFinger1.Y + (currentPosition.Y - oldFinger1.Y) * scaleFactor);

            var newPos2 = new Point(
             currentFinger2.X + (currentPosition.X - oldFinger2.X) * scaleFactor,
             currentFinger2.Y + (currentPosition.Y - oldFinger2.Y) * scaleFactor);

            var newPos = new Point(
                (newPos1.X + newPos2.X) / 2,
                (newPos1.Y + newPos2.Y) / 2);

            return new Point(
                newPos.X - currentPosition.X,
                newPos.Y - currentPosition.Y);
        }

        /// <summary>
        /// Updates the scaling factor by multiplying the delta.
        /// </summary>
        private void UpdateImageScale(double scaleFactor)
        {
            TotalImageScale *= scaleFactor;
            ApplyScale();
        }

        /// <summary>
        /// Applies the computed scale to the image control.
        /// </summary>
        private void ApplyScale()
        {
            ((CompositeTransform)GagImage.RenderTransform).ScaleX = TotalImageScale;
            ((CompositeTransform)GagImage.RenderTransform).ScaleY = TotalImageScale;
        }

        /// <summary>
        /// Updates the image position by applying the delta.
        /// Checks that the image does not leave empty space around its edges.
        /// </summary>
        private void UpdateImagePosition(Point delta)
        {
            var newPosition = new Point(ImagePosition.X + delta.X, ImagePosition.Y + delta.Y);

            if (newPosition.X > 0) newPosition.X = 0;
            if (newPosition.Y > 0) newPosition.Y = 0;

            if ((GagImage.ActualWidth * TotalImageScale) + newPosition.X < GagImage.ActualWidth)
                newPosition.X = GagImage.ActualWidth - (GagImage.ActualWidth * TotalImageScale);

            if ((GagImage.ActualHeight * TotalImageScale) + newPosition.Y < GagImage.ActualHeight)
                newPosition.Y = GagImage.ActualHeight - (GagImage.ActualHeight * TotalImageScale);

            ImagePosition = newPosition;

            ApplyPosition();
        }

        /// <summary>
        /// Applies the computed position to the image control.
        /// </summary>
        private void ApplyPosition()
        {
            ((CompositeTransform)GagImage.RenderTransform).TranslateX = ImagePosition.X;
            ((CompositeTransform)GagImage.RenderTransform).TranslateY = ImagePosition.Y;
        }

        /// <summary>
        /// Resets the zoom to its original scale and position
        /// </summary>
        private void ResetImagePosition()
        {
            TotalImageScale = 1;
            ImagePosition = new Point(0, 0);
            ApplyScale();
            ApplyPosition();
        }

        /// <summary>
        /// Checks that dragging by the given amount won't result in empty space around the image
        /// </summary>
        private bool IsDragValid(double scaleDelta, Point translateDelta)
        {
            if (ImagePosition.X + translateDelta.X > 0 || ImagePosition.Y + translateDelta.Y > 0)
                return false;

            if ((GagImage.ActualWidth * TotalImageScale * scaleDelta) + (ImagePosition.X + translateDelta.X) < GagImage.ActualWidth)
                return false;

            if ((GagImage.ActualHeight * TotalImageScale * scaleDelta) + (ImagePosition.Y + translateDelta.Y) < GagImage.ActualHeight)
                return false;

            return true;
        }

        /// <summary>
        /// Tells if the scaling is inside the desired range
        /// </summary>
        private bool IsScaleValid(double scaleDelta)
        {
            return (TotalImageScale * scaleDelta >= 1) && (TotalImageScale * scaleDelta <= MAX_IMAGE_ZOOM);
        }

        #endregion
        #region Event handlers

        /// <summary>
        /// Initializes the zooming operation
        /// </summary>
        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            _oldFinger1 = e.GetPosition(GagImage, 0);
            _oldFinger2 = e.GetPosition(GagImage, 1);
            _oldScaleFactor = 1;
        }

        /// <summary>
        /// Computes the scaling and translation to correctly zoom around your fingers.
        /// </summary>
        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            var scaleFactor = e.DistanceRatio / _oldScaleFactor;
            if (!IsScaleValid(scaleFactor))
                return;

            var currentFinger1 = e.GetPosition(GagImage, 0);
            var currentFinger2 = e.GetPosition(GagImage, 1);

            var translationDelta = GetTranslationDelta(
                currentFinger1,
                currentFinger2,
                _oldFinger1,
                _oldFinger2,
                ImagePosition,
                scaleFactor);

            _oldFinger1 = currentFinger1;
            _oldFinger2 = currentFinger2;
            _oldScaleFactor = e.DistanceRatio;

            UpdateImageScale(scaleFactor);
            UpdateImagePosition(translationDelta);
        }

        /// <summary>
        /// Moves the image around following your finger.
        /// </summary>
        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            var translationDelta = new Point(e.HorizontalChange, e.VerticalChange);

            if (IsDragValid(1, translationDelta))
                UpdateImagePosition(translationDelta);
        }

        /// <summary>
        /// Resets the image scaling and position
        /// </summary>
        private new void DoubleTap(object sender, GestureEventArgs e)
        {
            ResetImagePosition();
        }

        #endregion

        private void GagImageOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadImage();
            }
            catch
            {
            }
        }
    }
}