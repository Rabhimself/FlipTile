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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FlipTile
{

    public sealed partial class WinPage : Page
    {
        int diff;
        public WinPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            string[] param = (string[])e.Parameter;
            txtTime.Text += param[0];
            txtFlips.Text += param[1];

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
