using System;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;

namespace NineGag
{
    public partial class GagsPage
    {
        #region BackgroundWork enum

        public enum BackgroundWork
        {
            LoadPreviousPage,
            LoadNextPage,
            LoadPage
        };

        #endregion

        private const double MaxImageZoom = 5;

        private readonly BackgroundWorker _backgroundWorker;
        private Point _imagePosition = new Point(0, 0);
        private double _totalImageScale = 1d;


        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;
        private BackgroundWork _work;

        private NineGagPage _nineGagPage;
// ReSharper disable ConvertToAutoProperty
        public NineGagPage Page
// ReSharper restore ConvertToAutoProperty
        {
            get { return _nineGagPage; }
            set { _nineGagPage = value; }
        }

        public GagsPage()
        {
            InitializeComponent();
            Page = new NineGagPage();
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
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
                btnOPtions.Visibility = Visibility.Visible;
                txtLoading.Visibility = Visibility.Collapsed;
                GagTextBorder.Visibility = Visibility.Visible;
                GagText.Visibility = Visibility.Visible;
                Page.Reset();
                Page.LoadGags();
                if (_work == BackgroundWork.LoadNextPage || _work == BackgroundWork.LoadPage)
                    Page.CurrentImageId = 0; //if we loaded the next page, then we load the first gag
                else Page.CurrentImageId = Page.GagCount - 1; //else, we load the last gag
                //Load the image in the control
                GagImage.Source = Page.GagItem.Image;
                GagText.Text = Page.GagItem.TextDescription;
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
                                                          NavigationService.Navigate(new Uri("/MainPage.xaml",
                                                                                             UriKind.RelativeOrAbsolute)));
            }
            Deployment.Current.Dispatcher.BeginInvoke(() => GagText.Visibility = Visibility.Collapsed);
            Deployment.Current.Dispatcher.BeginInvoke(() => btnOPtions.Visibility = Visibility.Collapsed);
            string text = "Loading";
            const string dot = ".";
            int count = 0;
            while (Page.IsLoaded == false)
            {
// ReSharper disable AccessToModifiedClosure
                Deployment.Current.Dispatcher.BeginInvoke(() => txtLoading.Text = text);
// ReSharper restore AccessToModifiedClosure

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
            catch (Exception)
            {
                MessageBox.Show("Can't load a new image. Please try again!");
            }
        }

        private void GagsPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationContext.QueryString.ContainsKey("Type"))
                {
                    string type = NavigationContext.QueryString["Type"];
                    if (Page == null)
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                    else
                        switch (type)
                        {
                            case "hot":
                                Page.Type = PageType.Hot;
                                break;
                            case "trending":
                                Page.Type = PageType.Trending;
                                break;
                            default:
                                switch (type)
                                {
                                    case "day":
                                        Page.Type = PageType.TopDay;
                                        break;
                                    case "week":
                                        Page.Type = PageType.TopWeek;
                                        break;
                                    case "month":
                                        Page.Type = PageType.TopMonth;
                                        break;
                                    case "all":
                                        Page.Type = PageType.TopAll;
                                        break;
                                }
                                Page.FirstPageId = "1";
                                Page.Link = "http://9gag.com/top/" + type + "/1";
                                Page.Id = "1";
                                _work = BackgroundWork.LoadPage;
                                _backgroundWorker.RunWorkerAsync();
                                Page.Load();
                                return;
                        }


// ReSharper disable PossibleNullReferenceException
                    Page.PreviousPage = "FirstPage";
// ReSharper restore PossibleNullReferenceException
                    Page.CurrentImageId = 0;
                    try
                    {
                        _work = BackgroundWork.LoadPage;
                        _backgroundWorker.RunWorkerAsync();
                        Page.GetFirstPage(Page.Type);
                    }
                    catch (Exception exception)
                    {
                        if (exception is ArgumentException)
                        {
                            MessageBox.Show("You are not connected to the internet. Please try again!");
                            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                        }
                    }
                }
            }
            catch (Exception)
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void GagImageOpened(object sender, RoutedEventArgs e)
        {
        }

        private void ShowOptions(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Save this image?", "Save", MessageBoxButton.OKCancel);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                txtFileName.Visibility = Visibility.Visible;
                txtSave.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                txtFileName.Text = Page.GagItem.Id;
                GagImage.Visibility = Visibility.Collapsed;
                GagText.Visibility = Visibility.Collapsed;
                GagTextBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveImage(string fileName)
        {
            try
            {
                using (var library = new MediaLibrary())
                {
                    var bitmap = new WriteableBitmap(Page.GagItem.Image.PixelWidth, Page.GagItem.Image.PixelHeight);
                    var ms = new MemoryStream();
                    bitmap.SaveJpeg(ms, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
                    ms.Seek(0, SeekOrigin.Begin);
                    fileName += ".jpg";
                    library.SavePicture(fileName, ms);
                    MessageBox.Show("The picture was saved to your media library!");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Unable to save photo. Please try again!" + exception.Message);
            }
            finally
            {
                txtFileName.Visibility = Visibility.Collapsed;
                txtSave.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e)
        {
            string name = txtFileName.Text;
            SaveImage(name);

            GagImage.Visibility = Visibility.Visible;
            GagText.Visibility = Visibility.Visible;
            GagTextBorder.Visibility = Visibility.Visible;
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            txtFileName.Visibility = Visibility.Collapsed;
            txtSave.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;
            GagImage.Visibility = Visibility.Visible;
            GagText.Visibility = Visibility.Visible;
            GagTextBorder.Visibility = Visibility.Visible;
        }

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
                currentFinger1.X + (currentPosition.X - oldFinger1.X)*scaleFactor,
                currentFinger1.Y + (currentPosition.Y - oldFinger1.Y)*scaleFactor);

            var newPos2 = new Point(
                currentFinger2.X + (currentPosition.X - oldFinger2.X)*scaleFactor,
                currentFinger2.Y + (currentPosition.Y - oldFinger2.Y)*scaleFactor);

            var newPos = new Point(
                (newPos1.X + newPos2.X)/2,
                (newPos1.Y + newPos2.Y)/2);

            return new Point(
                newPos.X - currentPosition.X,
                newPos.Y - currentPosition.Y);
        }

        /// <summary>
        /// Updates the scaling factor by multiplying the delta.
        /// </summary>
        private void UpdateImageScale(double scaleFactor)
        {
            _totalImageScale *= scaleFactor;
            ApplyScale();
        }

        /// <summary>
        /// Applies the computed scale to the image control.
        /// </summary>
        private void ApplyScale()
        {
            ((CompositeTransform) GagImage.RenderTransform).ScaleX = _totalImageScale;
            ((CompositeTransform) GagImage.RenderTransform).ScaleY = _totalImageScale;
        }

        /// <summary>
        /// Updates the image position by applying the delta.
        /// Checks that the image does not leave empty space around its edges.
        /// </summary>
        private void UpdateImagePosition(Point delta)
        {
            var newPosition = new Point(_imagePosition.X + delta.X, _imagePosition.Y + delta.Y);

            if (newPosition.X > 0) newPosition.X = 0;
            if (newPosition.Y > 0) newPosition.Y = 0;

            if ((GagImage.ActualWidth*_totalImageScale) + newPosition.X < GagImage.ActualWidth)
                newPosition.X = GagImage.ActualWidth - (GagImage.ActualWidth*_totalImageScale);

            if ((GagImage.ActualHeight*_totalImageScale) + newPosition.Y < GagImage.ActualHeight)
                newPosition.Y = GagImage.ActualHeight - (GagImage.ActualHeight*_totalImageScale);

            _imagePosition = newPosition;

            ApplyPosition();
        }

        /// <summary>
        /// Applies the computed position to the image control.
        /// </summary>
        private void ApplyPosition()
        {
            ((CompositeTransform) GagImage.RenderTransform).TranslateX = _imagePosition.X;
            ((CompositeTransform) GagImage.RenderTransform).TranslateY = _imagePosition.Y;
        }

        /// <summary>
        /// Resets the zoom to its original scale and position
        /// </summary>
        private void ResetImagePosition()
        {
            _totalImageScale = 1;
            _imagePosition = new Point(0, 0);
            ApplyScale();
            ApplyPosition();
        }

        /// <summary>
        /// Checks that dragging by the given amount won't result in empty space around the image
        /// </summary>
        private bool IsDragValid(double scaleDelta, Point translateDelta)
        {
            if (_imagePosition.X + translateDelta.X > 0 || _imagePosition.Y + translateDelta.Y > 0)
                return false;

            if ((GagImage.ActualWidth*_totalImageScale*scaleDelta) + (_imagePosition.X + translateDelta.X) <
                GagImage.ActualWidth)
                return false;

            if ((GagImage.ActualHeight*_totalImageScale*scaleDelta) + (_imagePosition.Y + translateDelta.Y) <
                GagImage.ActualHeight)
                return false;

            return true;
        }

        /// <summary>
        /// Tells if the scaling is inside the desired range
        /// </summary>
        private bool IsScaleValid(double scaleDelta)
        {
            return (_totalImageScale*scaleDelta >= 1) && (_totalImageScale*scaleDelta <= MaxImageZoom);
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
            double scaleFactor = e.DistanceRatio/_oldScaleFactor;
            if (!IsScaleValid(scaleFactor))
                return;

            Point currentFinger1 = e.GetPosition(GagImage, 0);
            Point currentFinger2 = e.GetPosition(GagImage, 1);

            Point translationDelta = GetTranslationDelta(
                currentFinger1,
                currentFinger2,
                _oldFinger1,
                _oldFinger2,
                _imagePosition,
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

        #region Image Gestures

        public bool ImageIsLoading { get; set; }

        private void GestureListenerFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction != System.Windows.Controls.Orientation.Horizontal) return;
            if (e.Angle > 270 || e.Angle < 90) //Previous Image
            {
                try
                {
                    Page.CurrentImageId--;
                    if (Page.CurrentImageId <= -1)
                    {
                        Page.CurrentImageId = 0;
                        if (!Connected())
                            throw new ArgumentException();
                        if (Page.Type != PageType.Hot && Page.Type != PageType.Trending)
                        {
                            int id;
                            Int32.TryParse(Page.GetIdFromLink(Page.Link), out id);
                            id--;
                            if (id == 0)
                            {
                                ReachedFirstPage();
                                return;
                            }
                            string topType = "day";
                            if (Page.Type == PageType.TopDay)
                                topType = "day";
                            else if (Page.Type == PageType.TopAll)
                                topType = "all";
                            else if (Page.Type == PageType.TopMonth)
                                topType = "month";
                            else if (Page.Type == PageType.TopWeek)
                                topType = "week";
                            Page.Link = "http://9gag.com/top/" + topType + "/" + id.ToString();
                            Page.Id = id.ToString();
                            _work = BackgroundWork.LoadPreviousPage;
                            Page.IsLoaded = false;
                            txtLoading.Visibility = Visibility.Visible;
                            GagTextBorder.Visibility = Visibility.Collapsed;
                            GagText.Visibility = Visibility.Collapsed;
                            GagImage.Source = null;
                            Page.Load();
                            _backgroundWorker.RunWorkerAsync();
                            return;
                        }
                        string link = Page.Id;
                        int i;
                        if (Int32.TryParse(link, out i))
                        {
                            string caps = Page.Type.ToString();
                            caps = caps.ToLower();
                            string tmp = "/" + caps + "/" + i.ToString();
                            if (tmp == Page.FirstPageId)
                            {
                                ReachedFirstPage();
                                return;
                            }
                            i++;
                            Page.Id = i.ToString();
                            Page.Link = "http://9gag.com/" + caps + "/" + Page.Id;
                            _work = BackgroundWork.LoadPreviousPage;
                            Page.IsLoaded = false;
                            txtLoading.Visibility = Visibility.Visible;
                            GagTextBorder.Visibility = Visibility.Collapsed;
                            GagText.Visibility = Visibility.Collapsed;
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
                            if (Page.Type != PageType.Hot && Page.Type != PageType.Trending)
                            {
                                int id;
                                Int32.TryParse(Page.GetIdFromLink(Page.Link), out id);
                                id++;
                                string topType = "day";
                                if(Page.Type == PageType.TopDay)
                                    topType = "day";
                                else if(Page.Type==PageType.TopAll)
                                    topType = "all";
                                else if(Page.Type==PageType.TopMonth)
                                    topType = "month";
                                else if(Page.Type==PageType.TopWeek)
                                    topType = "week";
                                Page.Link = "http://9gag.com/top/" + topType + "/" + id.ToString();
                                Page.Id = id.ToString();
                            }
                            else
                            {
                                string caps = Page.Type.ToString();
                                caps = caps.ToLower();
                                Page.Link = "http://9gag.com/" + caps + "/" + Page.Id;
                            }
                            
                            _work = BackgroundWork.LoadNextPage;
                            Page.IsLoaded = false;
                            txtLoading.Visibility = Visibility.Visible;
                            GagTextBorder.Visibility = Visibility.Collapsed;
                            GagText.Visibility = Visibility.Collapsed;
                            GagImage.Source = null;
                            Page.IsLoaded = false;
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

        private void ReachedFirstPage()
        {
            Page.CurrentImageId = 0;
            MessageBoxResult messageBoxResult =
                MessageBox.Show("There are no newer gags. Do you want to refresh this page?", "This is the first page",
                                MessageBoxButton.OKCancel);
            if (messageBoxResult == MessageBoxResult.OK)
                RefreshPage();
        }

        private void RefreshPage()
        {
            GagImage.Source = null;
            Page.Reset();
            Page.IsLoaded = false;
            Page.Load();
            _work = BackgroundWork.LoadPage;
            _backgroundWorker.RunWorkerAsync();
            Page.Load();
        }

        #endregion
    }
}