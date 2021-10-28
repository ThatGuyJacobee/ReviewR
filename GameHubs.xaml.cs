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
using Windows.Web.Http; //For POST method
using System.Threading.Tasks; //For POST method
using System.Collections.ObjectModel; //Used to notify listview values when objects are changed
using System.Diagnostics; //Debug
using System.Text.Json; //Used for (de)serizalisation and JSON manipulation
using System.Text.Json.Serialization; //Used as serialization library

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

        //Object for credentials that will be used for deserialization
        public class IGDBCredentials
        {
            [JsonPropertyName("Client-ID")]
            public string Client_ID { get; set; }
            public string Authorization { get; set; }
            public string fields { get; set; }
        }

        //On search box content change
        private async void gamehub_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ObservableCollection<GameObject> dataList = new ObservableCollection<GameObject>();
            gamehub_list.ItemsSource = dataList;
            dataList.Clear();

            var SearchQuery = gamehub_search.Text;

            var idgbcredentials = new IGDBCredentials
            {
                Client_ID = $"{App.GlobalClientidIGDB}",
                Authorization = $"Bearer {App.GlobalAccessIGDB}",
                fields = $"search \"{SearchQuery}\"; fields name,release_date.human"
            };

            //Serialization occurs which converts the credentials above into a JSON format for POST
            string igbdrequeststring = JsonSerializer.Serialize(idgbcredentials);

            try
            {
                // Construct the HttpClient and Uri
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri("https://api.igdb.com/v4/games");

                httpClient.DefaultRequestHeaders.Add("Client-ID", App.GlobalClientidIGDB);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.GlobalAccessIGDB);
                //Debug.WriteLine("Request Headers: ");

                // Construct the JSON to post
                HttpStringContent content = new HttpStringContent(igbdrequeststring);
                Debug.WriteLine("Request Contents: " + igbdrequeststring);

                // Post the JSON and wait for a response
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                    uri,
                    content);

                // Make sure the post succeeded, and write out the response
                httpResponseMessage.EnsureSuccessStatusCode();
                var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                Debug.WriteLine("Request Response: " + httpResponseBody);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void ItemClick() //When an item in List View is pressed
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
