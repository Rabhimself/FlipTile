using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using Windows.Data.Json;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Xml.Serialization;
using System.IO;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FlipTile
{
    
    public sealed partial class GamePage : Page
    {
        #region Global Vars
        //global variable for checking if an animation is running
        private Boolean animating = false;
        //Use a grid to ref tiles, reduce findname calls
        private static int rows = 4, cols = 6, winFlag = 24, flipCount = 0, max, animatingTiles = 0;
        private Tile[,] tileGrid = new Tile[rows, cols];
        //Timer stuff
        DispatcherTimer dispatcherTimer;
        Stopwatch stopWatch;
        private long ms = 0, ss = 0, mm = 0, hh = 0;
        //
        int[,] GameboardImprint;

        //
        
        #endregion

        public GamePage()
        {
            this.InitializeComponent();
                     
            Loaded += PageLoaded;
            

        }


        #region Initialization
        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            Init();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            int diff = (int)e.Parameter;
            max = diff * 5;
            GameboardImprint = new int [max,2];
        }

        //sets up the tilegrid for use later on
        private void Init()
        {
            
            winFlag = 24;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    Tile tile = new Tile();
                    tile.Rect = (Rectangle)FindName("r" + row + "" + col);
                    tile.Rect.Fill = new SolidColorBrush(Tile.FaceDownColor);
                    tileGrid[row, col] = tile;
                }
            }
            GoInstant();
            Randomize();            

        }

        private void GetSpeedSetting()
        {
            object setting = Windows.Storage.ApplicationData.Current.RoamingSettings.Values["DesiredSpeed"];
            string speed = "normal";

            if (setting != null)
            {
                speed = setting.ToString();
            }

            switch (speed)
            {
                case "fast":
                    GoFast();
                    break;
                case "normal":
                    GoNormal();
                    break;
                case "slow":
                    GoSlow();
                    break;
                default:
                    GoNormal();
                    break;
            }
        }

        private void Dispatcher_Tick(object sender, object e)
        {
            ms = stopWatch.ElapsedMilliseconds;

            ss = ms / 1000;
            ms = ms % 1000;

            mm = ss / 60;
            ss = ss % 60;

            hh = mm / 60;
            mm = mm % 60;

            txtTimer.Text = hh.ToString("00") + ":" +
                                   mm.ToString("00") + ":" +
                                   ss.ToString("00");
        }


        //Picks 2-12 tiles and flips them, runs once the page is loaded to add some degree of difficulty
        private async void Randomize()
        {

            Random rand = new Random();
            int prevCol = -1, col;
            int prevRow = -1, row;
            for (int i = 0; i < max; i++)
            {

                do
                {
                    row = rand.Next(3);
                    col = rand.Next(5);
                }
                while ((row == prevRow) && (col == prevCol));

                prevCol = col;
                prevRow = row;
                
                //wait for the previous animation to finish
                while (animating)
                {
                    //poll every 25ms
                    await Task.Delay(5);
                }
                
                BatchFlip(row, col);
                GameboardImprint[i, 0] = row;
                GameboardImprint[i, 1] = col;
            }
            //StoreImprint(GameboardImprint);
            GetSpeedSetting();
        }

        private async void StoreImprint(int[,] imprint)
        {
            var serializer = new XmlSerializer(typeof(List<>));
            StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync("Imprints.xml");
            List<Array> arrays;
            Stream stream;
            

            if (file != null)
            {
                var readStream = await file.OpenReadAsync();
                stream = readStream.AsStreamForRead();
                arrays = (List<Array>)serializer.Deserialize(stream);
            }
            else
            {
                file = await ApplicationData.Current.RoamingFolder.CreateFileAsync("Imprints.xml");
                arrays = new List<Array>();
            }

            arrays.Add(imprint);
            stream = await file.OpenStreamForWriteAsync();          
            serializer.Serialize(stream, imprint);
        }


        #endregion

        #region Storyboard Garbage
        private void Flip(Tile tile, Storyboard sb)
        {
            sb.Stop();
            animating = true;

            Storyboard.SetTargetName(sb, tile.Rect.Name);

            if (tile.Faceup)
            {
                //getting a color animation from a storyboard *could* be easier...
                sb.Children.OfType<ColorAnimation>().First().To = Tile.FaceDownColor;
            }
            else
            {
                sb.Children.OfType<ColorAnimation>().First().To = Tile.FaceUpColor;
            }
            //Override the stretch with an actual height to allow the storyboard to run
            tile.Rect.Height = tile.Rect.ActualHeight;
            tile.Rect.Width = tile.Rect.ActualWidth;
            sb.Begin();
        }


        private void ToggleAnim(object sender, object e)
        {
            //get the target of the storyboard
            char[] name = Storyboard.GetTargetName(((Storyboard)sender)).ToCharArray();
            
            //row and column are stored in the name
            int row = Convert.ToInt16(name[1].ToString());
            int col = Convert.ToInt16(name[2].ToString());

            //determine if the tile is faceup, and set the approriate color
            //this prevents the color from reverting once the storyboard stops
            if (tileGrid[row, col].Faceup)
            {
                //Cast is necessary to change the color of the rectangle's fill property
                ((SolidColorBrush)tileGrid[row, col].Rect.Fill).Color = Tile.FaceDownColor;
            }
            else
            {
                //so many parenthesis
                ((SolidColorBrush)tileGrid[row, col].Rect.Fill).Color = Tile.FaceUpColor;
            }

            //using yet another cast to reduce local variables on the heap
            ((Storyboard)sender).Stop();

            //Drop the previously assigned height/width to allow alignment stretch
            tileGrid[row, col].Rect.Height = Double.NaN;
            tileGrid[row, col].Rect.Width = Double.NaN;

            //Invert tile faceup boolean
            tileGrid[row, col].Faceup = !tileGrid[row, col].Faceup;

            //inc/dec winflag
            if (tileGrid[row, col].Faceup)
                winFlag--;
            else
                winFlag++;

            
            //The only storyboard with 3 children is the center, Since the outside tiles take longer to animate, only flip the animating boolean if one of them is calling this
            if (((Storyboard)sender).Children.Count < 3)
                animating = false;


            //Win condition
            --animatingTiles;
            if ((winFlag == 0 || winFlag == 24) && animatingTiles == 0)
            {
                GoToWinPage();
            }

        }



        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //get form dimensions
            //determine smaller one
            //set correct aspect ratio
            //align on page
        }


        #endregion

        #region Tapped Events
        private async void Pause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            stopWatch.Stop();
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Paused";
            dialog.Content = "Tap or Click the Continue Button to resume your game!";       
            dialog.VerticalContentAlignment = VerticalAlignment.Center;
            dialog.PrimaryButtonText = "Continue";
            dialog.PrimaryButtonClick += Unpause;
            await dialog.ShowAsync();

        }

        private void Unpause(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            dispatcherTimer.Start();
            stopWatch.Start();
        }

        private void Fast_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (animating == false)
                StoreAnimSpeed(2);
        }

        private void Medium_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (animating == false)
                StoreAnimSpeed(3);
        }
        private void Slow_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (animating == false)
                StoreAnimSpeed(4);
        }

        //Flips everything over, then randomizes the board
        private void Reset_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BeginButtonGrid.Visibility = Visibility.Visible;

            winFlag = 24;

            for (int i = 0; i < tileGrid.GetLength(0); i++)
            {
                for (int j = 0; j < tileGrid.GetLength(1); j++)
                {
                    tileGrid[i, j].Faceup = false;
                    ((SolidColorBrush)tileGrid[i, j].Rect.Fill).Color = Tile.FaceDownColor;
                }
            }
            RecallImprint();
            
        }

        private void tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!animating)
            {
                flipCount++;
                //get the target of the storyboard
                char[] name = ((Rectangle)sender).Name.ToCharArray();

                //row and column are stored in the name
                int row = Convert.ToInt16(name[1].ToString());
                int col = Convert.ToInt16(name[2].ToString());

                BatchFlip(row, col);
            }

        }

        private void BatchFlip(int row, int col)
        {
            Flip(tileGrid[row, col], rectTappedSB);
            animatingTiles++;
            //down
            if (!(row - 1 < 0))
            {
                animatingTiles++;
                Flip(tileGrid[row - 1, col], rectDownSB);

            }
            //up
            if (!(row + 1 > 3))
            {
                animatingTiles++;
                Flip(tileGrid[row + 1, col], rectUpSB);
            }
            //Left
            if (!(col - 1 < 0))
            {
                animatingTiles++;
                Flip(tileGrid[row, col - 1], rectLeftSB);
            }
            //right
            if (!(col + 1 > 5))
            {
                animatingTiles++;
                Flip(tileGrid[row, col + 1], rectRightSB);
            }
        }

        private void BeginButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            dispatcherTimer = new DispatcherTimer();
            stopWatch = new Stopwatch();
            flipCount = 0;
            dispatcherTimer.Tick += Dispatcher_Tick;
            dispatcherTimer.Start();
            stopWatch.Start();
            BeginButtonGrid.Visibility = Visibility.Collapsed;

        }


        #endregion
        private void StoreAnimSpeed(int speed)
        {
            string setting = "normal";

            switch (speed)
            {
                //fastest(1ms), just for doing the randomizing. Doesn't have to look pretty.
                case 1:
                    GoInstant();
                    break;
                //fast
                case 2:
                    setting = "fast";
                    GoFast();
                    break;

                //slow
                case 4:
                    setting = "slow";
                    GoSlow();
                    break;

                default:
                    GoNormal();
                    break;

            }

            Windows.Storage.ApplicationData.Current.RoamingSettings.Values["DesiredSpeed"] = setting;
        }

        private void GoToWinPage()
        {

            string time = txtTimer.Text.ToString();
            string flips = flipCount.ToString();
            string[] param = new String[2];

            param[0] = time;
            param[1] = flips;
            dispatcherTimer.Stop();
            stopWatch.Stop();
            Frame.Navigate(typeof(WinPage), param);
        }

        private async void RecallImprint()//pass in the array in the future
        {
            GoInstant();
            int count = GameboardImprint.GetLength(0);
            {
                for (int i = 0; i < count; i++)
                {
                    while (animating)
                    {
                        //poll every 25ms
                        await Task.Delay(5);
                    }
                    BatchFlip(GameboardImprint[i, 0], GameboardImprint[i, 1]);
                }
            }

            GetSpeedSetting();
        }

        private int GetFlipCount(int[,] arr)
        {

            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);
            int count = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (arr[i, j] == 1)
                    {
                        count++;
                    }
                }
            }

            return count;

        }
        #region Pain
        private void GoInstant()
        {
            rectTappedSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 0));
            rectLeftSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 0));
            rectRightSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 0));
            rectUpSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 0));
            rectDownSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 0));
        }

        private void GoNormal()
        {
            rectTappedSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
            rectTappedSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 250);
            rectTappedSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 250);
            rectTappedSB.Children.OfType<DoubleAnimation>().Last().Duration = new TimeSpan(0, 0, 0, 0, 250);
            rectTappedSB.Children.OfType<DoubleAnimation>().Last().Duration = new TimeSpan(0, 0, 0, 0, 250);
            rectTappedSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 250);

            rectLeftSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1000));
            rectLeftSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 250);
            rectLeftSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 500);
            rectLeftSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 750);

            rectRightSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1000));
            rectRightSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 250);
            rectRightSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 500);
            rectRightSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 750);

            rectUpSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1000));
            rectUpSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 250);
            rectUpSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 500);
            rectUpSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 750);

            rectDownSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1000));
            rectDownSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 250);
            rectDownSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 500);
            rectDownSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 750);
        }

        private void GoSlow()
        {
            rectTappedSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 750));
            rectTappedSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 375);
            rectTappedSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 375);
            rectTappedSB.Children.OfType<DoubleAnimation>().Last().Duration = new TimeSpan(0, 0, 0, 0, 375);
            rectTappedSB.Children.OfType<DoubleAnimation>().Last().Duration = new TimeSpan(0, 0, 0, 0, 375);
            rectTappedSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 375);

            rectLeftSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1500));
            rectLeftSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 375);
            rectLeftSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 750);
            rectLeftSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 1125);

            rectRightSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1500));
            rectRightSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 375);
            rectRightSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 750);
            rectRightSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 1125);

            rectUpSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1500));
            rectUpSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 375);
            rectUpSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 750);
            rectUpSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 1125);

            rectDownSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1500));
            rectDownSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 375);
            rectDownSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 750);
            rectDownSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 1125);
        }

        private void GoFast()
        {
            rectTappedSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 240));
            rectTappedSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 120);
            rectTappedSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 120);
            rectTappedSB.Children.OfType<DoubleAnimation>().Last().Duration = new TimeSpan(0, 0, 0, 0, 120);
            rectTappedSB.Children.OfType<DoubleAnimation>().Last().Duration = new TimeSpan(0, 0, 0, 0, 120);
            rectTappedSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 120);

            rectLeftSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 480));
            rectLeftSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 120);
            rectLeftSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 240);
            rectLeftSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 360);

            rectRightSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 480));
            rectRightSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 120);
            rectRightSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 240);
            rectRightSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 360);

            rectUpSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 480));
            rectUpSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 120);
            rectUpSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 240);
            rectUpSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 360);

            rectDownSB.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 480));
            rectDownSB.Children.OfType<DoubleAnimation>().First().Duration = new TimeSpan(0, 0, 0, 0, 120);
            rectDownSB.Children.OfType<DoubleAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 240);
            rectDownSB.Children.OfType<ColorAnimation>().First().BeginTime = new TimeSpan(0, 0, 0, 0, 360);
        }
        #endregion

 
    }
}
