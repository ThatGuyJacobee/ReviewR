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

            //ObservableCollection<GameListObject> dataList = new ObservableCollection<GameListObject>();
            //GameListObject g1 = new GameListObject() { GameID = "Cs", GameName = "idk", ReleaseDate = "01/01/0001" };
            //dataList.Add(g1);
            //gamehub_list.ItemsSource = dataList;
        }

        public partial class GameListObject
        {
            [JsonPropertyName("id")]
            public long GameID { get; set; }

            [JsonPropertyName("name")]
            public string GameName { get; set; }

            [JsonPropertyName("release_dates")]
            public ObservableCollection<ReleaseDate> ReleaseDates { get; set; }
        }

        public partial class ReleaseDate
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("human")]
            public string Human { get; set; }
        }

        //Object for credentials that will be used for deserialization
        //public class IGDBCredentials
        //{
        //    [JsonPropertyName("Client-ID")]
        //    public string Client_ID { get; set; }
        //    public string Authorization { get; set; }
        //    public string fields { get; set; }
        //}

        //On search box content change
        private async void gamehub_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            //ObservableCollection<GameListObject> dataList = new ObservableCollection<GameListObject>();
            //gamehub_list.ItemsSource = dataList;
            //dataList.Clear();

            if (string.IsNullOrEmpty(gamehub_search.Text))
            {
                start_text.Visibility= Visibility.Collapsed;
                notfound_text.Visibility = Visibility.Visible;
                gamehub_list.ItemsSource = null;
            }

            else
            {
                var SearchQuery = gamehub_search.Text;
                start_text.Visibility = Visibility.Collapsed;
                notfound_text.Visibility = Visibility.Collapsed;

                //var idgbcredentials = new IGDBCredentials
                //{
                //    Client_ID = $"{App.GlobalClientidIGDB}",
                //    Authorization = $"Bearer {App.GlobalAccessIGDB}",
                //    fields = $"search \"{SearchQuery}\"; fields name,release_date.human; limit 1"
                //};

                //Serialization occurs which converts the credentials above into a JSON format for POST
                //string igbdrequeststring = JsonSerializer.Serialize(idgbcredentials);

                try
                {
                    // Construct the HttpClient and Uri
                    HttpClient httpClient = new HttpClient();
                    Uri uri = new Uri("https://api.igdb.com/v4/games");

                    httpClient.DefaultRequestHeaders.Add("Client-ID", App.GlobalClientidIGDB);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.GlobalAccessIGDB);
                    //Debug.WriteLine("Request Headers: ");

                    // Construct the JSON to post
                    HttpStringContent content = new HttpStringContent($"search \"{SearchQuery}\"; fields name,release_dates.human,external_games.category;");
                    Debug.WriteLine("Request Contents: " + content);

                    // Post the JSON and wait for a response
                    HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                        uri,
                        content);

                    // Make sure the post succeeded, and write out the response
                    httpResponseMessage.EnsureSuccessStatusCode();
                    var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                    Debug.WriteLine("Request Response: " + httpResponseBody);

                    //Deserialise the return output into game id, game name and release date
                    List<GameListObject> gamelistobjects = JsonSerializer.Deserialize<List<GameListObject>>(httpResponseBody);

                    //Create ObservableCollection which uses the deserialized items
                    ObservableCollection<GameListObject> dataList = new ObservableCollection<GameListObject>(gamelistobjects);
                    ObservableCollection<GameListObject> GameList = new ObservableCollection<GameListObject>();

                    //For each item that is within the dataList
                    foreach (var item in dataList)
                    {
                        Debug.WriteLine($"id: {item.GameID}");
                        Debug.WriteLine($"name: {item.GameName}");

                        //Add the object and its items as a new item of the ListView
                        GameListObject add = new GameListObject() { GameID = item.GameID, GameName = item.GameName };
                        GameList.Add(add);

                        //If there are no release dates set, then don't run the statement
                        if (item.ReleaseDates != null)
                        {
                            //For each date that is found within releasedates
                            foreach (var date in item.ReleaseDates)
                            {
                                Debug.WriteLine($"releaseDate: {date.Human}");
                            }
                        }
                    }
                    //Add all the items once all of the items are ready to be added
                    gamehub_list.ItemsSource = GameList;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        //Set the GameID as a static long variable which I will use to request further data about the game in the game-specific pages
        public static long GameSpecificGameID = 0;

        //Set the Game Name as above
        public static string GameSpecificGameName = "";

        private void gamehub_list_ItemClick(object sender, ItemClickEventArgs e) //When an item in List View is pressed
        {
            var clickedItem = e.ClickedItem as GameListObject;
            Debug.WriteLine("Click Item GameID: " + clickedItem.GameID);
            Debug.WriteLine("Click Item Game Name: " + clickedItem.GameName);

            //Set the GameID as a static long variable
            GameSpecificGameID = clickedItem.GameID;
            GameSpecificGameName = clickedItem.GameName;

            this.Frame.Navigate(typeof(GameHubPages), null); //Switch to the game-specific page
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
