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
        public GamePage()
        {
            this.InitializeComponent();
        }

        private void tapped(object sender, TappedRoutedEventArgs e)
        {
            Rectangle r = (Rectangle)sender;
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

        //Works but is VERY hackey, as there is no way of stopping each animation once started
        //either look into awaiting each storyboard, or possibly do one animation and bind each other affected
        //tile's height&color.
        private void Flip(Rectangle r)
        {        
            Storyboard sb = new Storyboard();

            DoubleAnimation da = new DoubleAnimation();
            da.AutoReverse = true;
            da.EnableDependentAnimation = true;
            da.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 120));
            da.From = 200;
            da.To = 0;

            ColorAnimation ca = new ColorAnimation();
            ca.AutoReverse = false;
            ca.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 60));

            SolidColorBrush faceDownColor = new SolidColorBrush(Windows.UI.Colors.AliceBlue);
            SolidColorBrush currentColor = (SolidColorBrush)r.Fill;

            if (currentColor.Color == faceDownColor.Color)
                ca.To = (Windows.UI.Colors.Blue);
            else
                ca.To = (Windows.UI.Colors.AliceBlue);

            Storyboard.SetTargetProperty(da, "Height");
            Storyboard.SetTargetProperty(ca, "(Fill).(Color)");
            Storyboard.SetTarget(ca, r);
            Storyboard.SetTarget(da, r);
            
            sb.Children.Add(da);
            sb.Children.Add(ca);

            sb.Begin();
        }
    }
}
