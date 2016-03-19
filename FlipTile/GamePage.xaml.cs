using System;
using System.Collections.Generic;
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
    //TODO:
    public sealed partial class GamePage : Page
    {
        private Boolean animating = false;
        private int winFlag = 24;
        private Tile tile;
        SolidColorBrush brush;
        //
        private Tile[,] tileGrid = new Tile[4, 6];
        //
        public GamePage()
        {
            this.InitializeComponent();
            Init();
            Loaded += Randomize;
            
        }

        private async void Randomize(object sender, RoutedEventArgs e)
        {


            Random rand = new Random();
            int max = rand.Next(2, 12);

            for (int i = 0; i < max; i++)
            {
                int row = rand.Next(3);
                int col = rand.Next(5);

                while (animating)
                {
                    await Task.Delay(25);
                }
                Flip(tileGrid[row, col], rectTappedSB);
            }

        }

        private void tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!animating)
            {

                int row = Convert.ToInt16(((Rectangle)sender).Name.Substring(1, 1));
                int col = Convert.ToInt16(((Rectangle)sender).Name.Substring(2));
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
                //down
                if (!(col + 1 > 5))
                {
                    Flip(tileGrid[row, col + 1], rectRightSB);
                }
            }

        }

        private void Flip(Tile tile, Storyboard sb)
        {
            sb.Stop();
            animating = true;

            Storyboard.SetTargetName(sb, tile.Rect.Name);

            if (tile.Faceup)
            {
                sb.Children.OfType<ColorAnimation>().First().To = Windows.UI.Colors.Blue;
            }
            else
            {
                sb.Children.OfType<ColorAnimation>().First().To = Windows.UI.Colors.AliceBlue;
            }
            tile.Rect.Height = tile.Rect.ActualHeight;
            sb.Begin();
        }

        private void ToggleAnim(object sender, object e)
        {
            char[] name = Storyboard.GetTargetName(((Storyboard)sender)).ToCharArray();

            int row = Convert.ToInt16(name[1].ToString());
            int col = Convert.ToInt16(name[2].ToString());

            if (tileGrid[row, col].Faceup)
            {
                ((SolidColorBrush)tileGrid[row, col].Rect.Fill).Color = Windows.UI.Colors.Blue;
            }
            else
            {
                ((SolidColorBrush)tileGrid[row, col].Rect.Fill).Color = Windows.UI.Colors.AliceBlue;
            }

            ((Storyboard)sender).Stop();

            tileGrid[row, col].Faceup = !tileGrid[row, col].Faceup;
            animating = false;
            if (winFlag == 0)
                Frame.Navigate(typeof(MainPage));
        }

        private void Init()
        {
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
    }
}
