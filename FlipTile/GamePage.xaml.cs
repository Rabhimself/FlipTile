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


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FlipTile
{
    //TODO: Figure out why the ui becomes unresponsive at times, for about 1sec. May coincide with GC, reduction of local variables didn't help much.
    //      Move everything from loaded to a start button
    public sealed partial class GamePage : Page
    {
        #region Global Vars
        //global variable for checking if an animation is running
        private Boolean animating = false;
        //Use a grid to ref tiles, reduce findname calls
        private static int rows = 4, cols = 6, winFlag = 24;
        private Tile[,] tileGrid = new Tile[rows, cols];
        //Timer stuff
        DispatcherTimer dispatcherTimer;
        Stopwatch stopWatch;
        private long ms = 0, ss = 0, mm = 0, hh = 0, dd = 0;
        //
        #endregion

        public GamePage()
        {
            this.InitializeComponent();         
            Loaded += PageLoaded;
            

        }

        #region Storyboard Garbage
        private void Flip(Tile tile, Storyboard sb)
        {
            sb.Stop();
            animating = true;

            Storyboard.SetTargetName(sb, tile.Rect.Name);

            if (tile.Faceup)
            {
                //getting a color animation from a storyboard *could* be easier...
                sb.Children.OfType<ColorAnimation>().First().To = Windows.UI.Colors.Blue;
            }
            else
            {
                sb.Children.OfType<ColorAnimation>().First().To = Windows.UI.Colors.AliceBlue;
            }
            //Override the stretch with an actual height to allow the storyboard to run
            tile.Rect.Height = tile.Rect.ActualHeight;
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
                ((SolidColorBrush)tileGrid[row, col].Rect.Fill).Color = Windows.UI.Colors.Blue;
            }
            else
            {
                //so many parenthesis
                ((SolidColorBrush)tileGrid[row, col].Rect.Fill).Color = Windows.UI.Colors.AliceBlue;
            }

            //using yet another cast to reduce local variables on the heap
            ((Storyboard)sender).Stop();

            tileGrid[row, col].Faceup = !tileGrid[row, col].Faceup;
            animating = false;

            if (winFlag == 0)
                Frame.Navigate(typeof(MainPage));
        }
        #endregion

        #region Initialization

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            Init();
            Randomize();
        }

        //sets up the tilegrid for use later on
        private void Init()
        {
            dispatcherTimer = new DispatcherTimer();
            stopWatch = new Stopwatch();
            dispatcherTimer.Tick += Dispatcher_Tick;
            dispatcherTimer.Start();
            stopWatch.Start();

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    Tile tile = new Tile();
                    tile.Rect = (Rectangle)FindName("r" + row + "" + col);

                    tileGrid[row, col] = tile;
                }
            }
        }

        private void Dispatcher_Tick(object sender, object e)
        {
            ms = stopWatch.ElapsedMilliseconds;

            ss = ms / 1000;
            ms = ms % 1000;

            mm = ss / 60;
            ss = ss % 60;

            hh = mm % 60;
            mm = mm % 60;

            dd = hh / 24;
            hh = hh % 24;

            txtTimer.Text = hh.ToString("00") + ":" +
                                   mm.ToString("00") + ":" +
                                   ss.ToString("00") + ":" +
                                   ms.ToString("000");
        }


        //Picks 2-12 tiles and flips them, runs once the page is loaded to add some degree of difficulty
        private async void Randomize()
        {

            Random rand = new Random();
            int max = rand.Next(2, 12);

            for (int i = 0; i < max; i++)
            {
                int row = rand.Next(3);
                int col = rand.Next(5);

                //wait for the previous animation to finish
                while (animating)
                {
                    //poll every 25ms
                    await Task.Delay(25);
                }

                Flip(tileGrid[row, col], rectTappedSB);
            }

        }
        #endregion

        #region Tapped Events
        private async void Pause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            stopWatch.Stop();
            ContentDialog dialog = new ContentDialog();
            dialog.Content = "Paused";
            
            dialog.PrimaryButtonText = "Continue";
            dialog.PrimaryButtonClick += Unpause;
            await dialog.ShowAsync();

        }

        private void Unpause(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            dispatcherTimer.Start();
            stopWatch.Start();
        }

        //Flips everything over, then randomizes the board
        private async void Reset_Tapped(object sender, TappedRoutedEventArgs e)
        {
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (tileGrid[j, i].Faceup == true)
                    {
                        //wait for the previous animation to finish
                        while (animating)
                        {
                            //poll every 25ms
                            await Task.Delay(25);
                        }
                        Flip(tileGrid[j, i], rectTappedSB);
                    }
                }
            }

            Randomize();
        }
        private void tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!animating)
            {
                //get the target of the storyboard
                char[] name = ((Rectangle)sender).Name.ToCharArray();

                //row and column are stored in the name
                int row = Convert.ToInt16(name[1].ToString());
                int col = Convert.ToInt16(name[2].ToString());

                Flip(tileGrid[row, col], rectTappedSB);

                //down
                if (!(row - 1 < 0))
                {
                    Flip(tileGrid[row - 1, col], rectDownSB);
                }
                //up
                if (!(row + 1 > 3))
                {
                    Flip(tileGrid[row + 1, col], rectUpSB);
                }
                //Left
                if (!(col - 1 < 0))
                {
                    Flip(tileGrid[row, col - 1], rectLeftSB);
                }
                //right
                if (!(col + 1 > 5))
                {
                    Flip(tileGrid[row, col + 1], rectRightSB);
                }
            }

        }
        #endregion
    }
}
