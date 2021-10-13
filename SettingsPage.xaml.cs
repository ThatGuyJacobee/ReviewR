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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private List<GameReview> listOfReview = new List<GameReview>();

        public SettingsPage()
        {
            this.InitializeComponent();

            listOfReview.Add(new GameReview { Name = "COD", Age = 20 });
            listOfReview.Add(new GameReview { Name = "Battlefield", Age = 21 });
            listOfReview.Add(new GameReview { Name = "CS:GO", Age = 19 });
            listOfReview.Add(new GameReview { Name = "Destiny", Age = 18 });
            listOfReview.Add(new GameReview { Name = "Rocket League", Age = 20 });
            listOfReview.Add(new GameReview { Name = "ETS2", Age = 20 });
            listOfReview.Add(new GameReview { Name = "Space Engineers", Age = 21 });
            listOfReview.Add(new GameReview { Name = "Cyberpunk", Age = 20 });
            listOfReview.Add(new GameReview { Name = "Forza Horizon 4", Age = 23 });
            listOfReview.Add(new GameReview { Name = "Minecraft", Age = 20 });

            myreviews_list.ItemsSource = listOfReview;
        }

        public class GameReview
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        private void myreviews_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
