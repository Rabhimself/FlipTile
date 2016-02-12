using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FlipTile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        private Boolean animating = false;
        private int winFlag = 24;
        private Rectangle r;

        public GamePage()
        {
            this.InitializeComponent();
            Randomize();
        }

        private void tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!animating)
            {
                animating = true;
                r = (Rectangle)sender;
                Flip(r);
                String name = r.Name.Substring(1);
                int row = Convert.ToInt16(name.Substring(0, 1));
                int col = Convert.ToInt16(name.Substring(1));

                Grid g = (Grid)r.Parent;

                //down
                if (!(row - 1 < 0))
                {
                    Flip((Rectangle)g.FindName("r" + (row - 1) + "" + col));
                }
                //up
                if (!(row + 1 > 3))
                {
                    Flip((Rectangle)g.FindName("r" + (row + 1) + col));
                }
                //
                if (!(col - 1 < 0))
                {
                    Flip((Rectangle)g.FindName("r" + row + (col - 1)));
                }
                //down
                if (!(col + 1 > 5))
                {
                    Flip((Rectangle)g.FindName("r" + row + (col + 1)));
                }
            }
            
        }

        private void Flip(Rectangle r)
        {   
                 
            //Need to replace this method of animating multiple rectangles.
            //Too many Storyboard objects are being GCed
            Storyboard sb = new Storyboard();

            DoubleAnimation da = new DoubleAnimation();
            da.AutoReverse = true;
            da.EnableDependentAnimation = true;
            da.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
            da.To = 0;

            ColorAnimation ca = new ColorAnimation();
            ca.AutoReverse = false;
            ca.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 1));
            ca.BeginTime = new TimeSpan(0, 0, 0, 0, 125);

            SolidColorBrush faceDownColor = new SolidColorBrush(Windows.UI.Colors.AliceBlue);
            SolidColorBrush currentColor = (SolidColorBrush)r.Fill;

            if (currentColor.Color == faceDownColor.Color)
            {
                ca.To = (Windows.UI.Colors.Blue);
                winFlag++;
            }
            else
            {
                ca.To = (Windows.UI.Colors.AliceBlue);
                winFlag--;
            }           

            Storyboard.SetTargetProperty(da, "Height");
            Storyboard.SetTargetProperty(ca, "(Fill).(Color)");
            Storyboard.SetTarget(ca, r);
            Storyboard.SetTarget(da, r);
            sb.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
            sb.Children.Add(da);
            sb.Children.Add(ca);
            sb.Completed += new EventHandler<object>(ToggleAnim);

            sb.Begin();
        }

        private void ToggleAnim(object sender, object e)
        {    
            animating = false;
            if (winFlag == 0)
                Frame.Navigate(typeof(MainPage));
        }
        private void Randomize()
        {
            Random rand = new Random();
            int max = rand.Next(12);

            for(int i = 0; i< max; i++)
            {
                int row = rand.Next(3);
                int col = rand.Next(5);

                Flip((Rectangle)FindName("r" + row +"" + col));
            }
        }
    }
}
