using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace NineGag
{
    public partial class GagsPage : PhoneApplicationPage
    {
        public enum BackgroundWork
        {
            LoadPreviousGag,
            LoadNextGag,
            LoadPage
        };

        private BackgroundWork _work;
        private readonly NineGagPage Page;
        private int Index;
        private bool LoadingCompleted;
        private BackgroundWorker backgroundWorker;
        public GagsPage()
        {
            InitializeComponent();
            Page = new NineGagPage();

            try
            {
                Page.Load();
            }
            catch (Exception exception)
            {
                if (exception is ArgumentException)
                    MessageBox.Show("You are not connected to the internet. Please try again!");
                NavigationService.GoBack();
            }
            
            LoadingCompleted = false;
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorkerDoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorkerRunWorkerCompleted);
            _work = BackgroundWork.LoadPage;
            backgroundWorker.RunWorkerAsync();
            Index = 0;
            var gestureListener = GestureService.GetGestureListener(LayoutRoot);
            gestureListener.Flick += new EventHandler<FlickGestureEventArgs>(GestureListenerFlick);
            gestureListener.Tap += new EventHandler<GestureEventArgs>(gestureListener_Tap);
            gestureListener.Hold += new EventHandler<GestureEventArgs>(gestureListener_Hold);
            gestureListener.DragStarted += new EventHandler<DragStartedGestureEventArgs>(gestureListener_DragStarted);
            gestureListener.DragDelta += new EventHandler<DragDeltaGestureEventArgs>(gestureListener_DragDelta);
            gestureListener.DragCompleted += new EventHandler<DragCompletedGestureEventArgs>(gestureListener_DragCompleted);
        }

       
        void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                string id = null;
                if (_work == BackgroundWork.LoadPage)
                {
                    Page.LoadGags();
                    id = Page.GagItem.Id;
                    GagImage.Source = Page.GagItem.Image;
                    //for (int i = 0; i < 10; i++)
                    //    Page.LoadPreviousGag();
                }
                
            }
            catch (Exception exception)
            {
                if (exception is ArgumentException && exception.Message == "Not Connected")
                    MessageBox.Show("You are not connected to the internet. Please try again");
                else if (exception is IndexOutOfRangeException)
                    MessageBox.Show(exception.Message);
            }
        }


        void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            string text = "Loading";
            string dot = ".";
            int count = 0;
            while (Page.IsLoaded == false)
            {
                Deployment.Current.Dispatcher.BeginInvoke(()=> textBlock1.Text = text);
                
                System.Threading.Thread.Sleep(500);
                if(count<3)
                {
                    count++;
                    text += dot;
                }
                else
                {
                    count = 0;
                    text = "Loading";
                }

            }
            
        }

        void gestureListener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            
        }

        void gestureListener_DragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            
        }

        void gestureListener_DragStarted(object sender, DragStartedGestureEventArgs e)
        {
            
        }

        void gestureListener_Hold(object sender, GestureEventArgs e)
        {
           
        }

        void gestureListener_Tap(object sender, GestureEventArgs e)
        {
            
        }

        void GestureListenerFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction != System.Windows.Controls.Orientation.Horizontal) return;
            if (e.Angle > 270 || e.Angle < 90) //Previous Image
            {
                try
                {
                    Page.CurrentImageId--;
                    if (Page.CurrentImageId == -1)
                    {
                        Page.CurrentImageId = 0;
                        MessageBox.Show("There are no newer gags");
                        return;
                    }
                    _work = BackgroundWork.LoadPreviousGag;
                    Page.LoadPreviousGag();
                   if(Page.CurrentImageId >= 0)
                        GagImage.Source = Page.GagItem.Image;
                    
                }
                catch (Exception exception)
                {
                    if (exception is IndexOutOfRangeException)
                        MessageBox.Show("There are no previous images");
                    Page.CurrentImageId = 1;
                }
            }
            else
            {
                try
                {
                    Page.CurrentImageId++;
                    GagImage.Source = Page.GagItem.Image; //Next image
                    Page.LoadNextGag();
                    Index++;
                }
                catch (Exception exception)
                {
                    Page.LoadNextGag();
                    Page.CurrentImageId--;
                }
            }
        }

        public void LoadGag()
        {
            Index = Page.CurrentImageId;
            GagImage.Source = Page.GagItem.Image;
        }

        private void GagsPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationContext.QueryString.ContainsKey("Type"))
                {
                    string type = NavigationContext.QueryString["Type"];
                    // MessageBox.Show("Page type is " + type + "link=" + Page.Link);
                    if (Page == null)
                        NavigationService.GoBack();
                    else
                    if (type == "HotPage")
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
                    NavigationService.GoBack();
                }
            }
            catch (ArgumentNullException)
            {
                NavigationService.GoBack();
            }
        }

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            
        }

        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            
        }

        private void DoubleTap(object sender, GestureEventArgs e)
        {
            
        }
    }
}