using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Net.NetworkInformation;
using GestureEventArgs = Microsoft.Phone.Controls.GestureEventArgs;

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
        #region fields

        private double zoom = 1;
        private bool duringDrag = false;
        private bool mouseDown = false;
        private Point lastMouseDownPos = new Point();
        private Point lastMousePos = new Point();
        private Point lastMouseViewPort = new Point();
        private Uri seadragonUrl = new Uri("http://static.seadragon.com/content/misc/");

       
        private NineGagPage Page;
        private readonly BackgroundWorker _backgroundWorker;
        private BackgroundWork _work;
        private double TotalImageScale = 1d;
        private Point ImagePosition = new Point(0, 0);


        private const double MAX_IMAGE_ZOOM = 5;
        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;
        #endregion
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
            this.GagImage.Loaded += new RoutedEventHandler(msi_Loaded);

            //
            // Firing an event when all of the images have been Loaded
            //
            this.GagImage.ImageOpenSucceeded += new RoutedEventHandler(msi_ImageOpenSucceeded);

            //
            // Handling all of the mouse and keyboard functionality
            //
            this.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                lastMousePos = e.GetPosition(GagImage);

                if (duringDrag)
                {
                    Point newPoint = lastMouseViewPort;
                    newPoint.X += (lastMouseDownPos.X - lastMousePos.X) / GagImage.ActualWidth * GagImage.ViewportWidth;
                    newPoint.Y += (lastMouseDownPos.Y - lastMousePos.Y) / GagImage.ActualWidth * GagImage.ViewportWidth;
                    GagImage.ViewportOrigin = newPoint;
                }
            };

            this.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                lastMouseDownPos = e.GetPosition(GagImage);
                lastMouseViewPort = GagImage.ViewportOrigin;

                mouseDown = true;

                GagImage.CaptureMouse();
            };

            this.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
            {
                if (!duringDrag)
                {
                    bool shiftDown = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                    double newzoom = zoom;

                    if (shiftDown)
                    {
                        newzoom /= 2;
                    }
                    else
                    {
                        newzoom *= 2;
                    }

                    Zoom(newzoom, GagImage.ElementToLogicalPoint(this.lastMousePos));
                }
                duringDrag = false;
                mouseDown = false;

                GagImage.ReleaseMouseCapture();
            };

            this.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                lastMousePos = e.GetPosition(GagImage);
                if (mouseDown && !duringDrag)
                {
                    duringDrag = true;
                    double w = GagImage.ViewportWidth;
                    Point o = new Point(GagImage.ViewportOrigin.X, GagImage.ViewportOrigin.Y);
                    GagImage.UseSprings = false;
                    GagImage.ViewportOrigin = new Point(o.X, o.Y);
                    GagImage.ViewportWidth = w;
                    zoom = 1 / w;
                    GagImage.UseSprings = true;
                }

                if (duringDrag)
                {
                    Point newPoint = lastMouseViewPort;
                    newPoint.X += (lastMouseDownPos.X - lastMousePos.X) / GagImage.ActualWidth * GagImage.ViewportWidth;
                    newPoint.Y += (lastMouseDownPos.Y - lastMousePos.Y) / GagImage.ActualWidth * GagImage.ViewportWidth;
                    GagImage.ViewportOrigin = newPoint;
                }
            }; 
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string selectedIndex = "";
            if (NavigationContext.QueryString.TryGetValue("selectedItem", out selectedIndex))
            {
                int index = int.Parse(selectedIndex);
              
                    //this.GagImage.Source = new DeepZoomImageTileSource(new Uri())
                
            }
        }

        private bool Connected()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        #region GagImage events and methods

        void msi_ImageOpenSucceeded(object sender, RoutedEventArgs e)
        {
            //If collection, this gets you a list of all of the MultiScaleSubImages
            //
            //foreach (MultiScaleSubImage subImage in GagImage.SubImages)
            //{
            //    // Do something
            //}
            Point point = this.GagImage.ViewportOrigin;
            GagImage.ViewportWidth = 1;
            GagImage.ViewportOrigin = new Point(0, -0.5);
        }

        void msi_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Zoom(double newzoom, Point p)
        {
            if (newzoom < 0.5)
            {
                newzoom = 0.5;
            }

            GagImage.ZoomAboutLogicalPoint(newzoom / zoom, p.X, p.Y);
            zoom = newzoom;
        }

        private void ZoomInClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Zoom(zoom * 1.3, GagImage.ElementToLogicalPoint(new Point(.5 * GagImage.ActualWidth, .5 * GagImage.ActualHeight)));
        }

        private void ZoomOutClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Zoom(zoom / 1.3, GagImage.ElementToLogicalPoint(new Point(.5 * GagImage.ActualWidth, .5 * GagImage.ActualHeight)));
        }

        private void GoHomeClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.GagImage.ViewportWidth = 1;
            this.GagImage.ViewportOrigin = new Point(0, -0.55);
            ZoomFactor = 1;
        }

        public double ZoomFactor
        {
            get { return zoom; }
            set { zoom = value; }
        }

        private void GoFullScreenClick(object sender, System.Windows.RoutedEventArgs e)
        {

            GagImage.ZoomAboutLogicalPoint(1.5, 0, 0);
        }

        // Handling the VSM states
        private void LeaveMovie(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "FadeOut", true);
        }

        private void EnterMovie(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "FadeIn", true);
        }


        // unused functions that show the inner math of Deep Zoom
        public Rect GetImageRect()
        {
            return new Rect(-GagImage.ViewportOrigin.X / GagImage.ViewportWidth, -GagImage.ViewportOrigin.Y / GagImage.ViewportWidth, 1 / GagImage.ViewportWidth, 1 / GagImage.ViewportWidth * GagImage.AspectRatio);
        }

        public Rect ZoomAboutPoint(Rect img, double zAmount, Point pt)
        {
            return new Rect(pt.X + (img.X - pt.X) / zAmount, pt.Y + (img.Y - pt.Y) / zAmount, img.Width / zAmount, img.Height / zAmount);
        }

        public void LayoutDZI(Rect rect)
        {
            double ar = GagImage.AspectRatio;
            GagImage.ViewportWidth = 1 / rect.Width;
            GagImage.ViewportOrigin = new Point(-rect.Left / rect.Width, -rect.Top / rect.Width);
        }

        #endregion
        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                textBlock1.Visibility = Visibility.Collapsed;
                GagText.Visibility = Visibility.Visible;
                Page.Reset();
                Page.LoadGags();
                if (_work == BackgroundWork.LoadNextPage || _work == BackgroundWork.LoadPage)
                    Page.CurrentImageId = 0; //if we loaded the next page, then we load the first gag
                else Page.CurrentImageId = Page.GagCount - 1; //else, we load the last gag
                ////GagImage.Stretch = Stretch.None;
                GagImage.Source = new DeepZoomImageTileSource(new Uri(Page.GagItem.ImageLink));
                //GagImage.Source = Page.GagItem.Image;

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
                        GagText.Text = Page.GagItem.TextDescription;
                        //GagImage.Stretch = Stretch.None;

                        GagImage.Source = new DeepZoomImageTileSource(new Uri(Page.GagItem.ImageLink));
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
                        //GagImage.Stretch = Stretch.None;
                        GagImageOpened(null, null);

                        GagImage.Source = new DeepZoomImageTileSource(new Uri(Page.GagItem.ImageLink));
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
                ResetImagePosition();
                Page.GagItem.Height = GagImage.ActualHeight;
                Page.GagItem.Width = GagImage.ActualWidth;
                Page.GagItem.SetStretch();
                if(Page.GagItem.StretchMode == Stretch.None)
                {
                    GagImage.Width = Page.GagItem.Width;
                }
                else
                    //GagImage.Stretch = Page.GagItem.StretchMode;
                GagText.Text = Page.GagItem.TextDescription;
            }
            catch
            {
            }
        }
    }
}