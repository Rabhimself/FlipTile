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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FlipTile
{
    //TODO: Where to begin?
    //      Load best times&flips from localstore
    //      Localization/lang select (?)
    //      
    public sealed partial class MainPage : Page
    {
        private static int diff;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void EasyButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            diff = 1;
            Frame.Navigate(typeof(GamePage), diff);
        }

        private void MedButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            diff = 2;
            Frame.Navigate(typeof(GamePage), diff);
        }

        private void HardButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            diff = 3;
            Frame.Navigate(typeof(GamePage), diff);
        }
    }
}
