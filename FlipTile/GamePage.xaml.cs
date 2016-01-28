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
            String name = r.Name;
            
            //not converting properly
           int row = 2;
           int col = 3;
            Grid g = (Grid)r.Parent;
            Rectangle temp = (Rectangle)g.FindName("r" + (row - 1) + "" + col);
            //down
            if (!(row-1 < 0))
            {
                Flip(temp);
            }
            //up
            if (!(row + 1 > 3))
            {
                Flip((Rectangle)g.FindName("r" + (row + 1) + col));
            }
            //
            if(!(col-1 < 0))
            {
                Flip((Rectangle)g.FindName("r" + row + (col-1)));
            }
            //down
            if(!(col+1 > 5))
            {
                Flip((Rectangle)g.FindName("r" + row  + (col+1)));
            }
        }

        private void Flip(Rectangle r)
        {
            rectSB.Stop();
            Storyboard.SetTarget(rectSB, r);
            rectSB.Begin();
        }
    }
}
