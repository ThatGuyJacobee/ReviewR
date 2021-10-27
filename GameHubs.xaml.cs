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
using System.Collections.ObjectModel; //Used to notify listview values when objects are changed

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameHubs : Page
    {
        public GameHubs()
        {
            this.InitializeComponent();
            this.Loaded += Page_Loaded;

            ObservableCollection<GameObject> dataList = new ObservableCollection<GameObject>();
            GameObject g1 = new GameObject() { GameName = "Cs", ReleaseDate = "idk", GameIcon = "test" };
            dataList.Add(g1);
            gamehub_list.ItemsSource = dataList;
        }

        public class GameObject
        {
            public string GameName { get; set; }
            public string ReleaseDate { get; set; }
            public string GameIcon { get; set; }
        }

        private void ItemClick() //When an item in List View is pressed
        {

        }

        private void gamehub_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void continue_button_Click(object sender, RoutedEventArgs e)
        {
            //Sets the current elements to collapsed
            username_text.Visibility = Visibility.Collapsed;
            gamehub_description.Visibility = Visibility.Collapsed;
            continue_button.Visibility = Visibility.Collapsed;

            //Sets the game hub elements to visible
            gamehub_search.Visibility = Visibility.Visible;
            addfilters_button.Visibility = Visibility.Visible;
            gamehub_list.Visibility = Visibility.Visible;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Personalises the welcome message
            username_text.Text = "Hi again, " + App.GlobalUsername + "!";
        }
    }
}
