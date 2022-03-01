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
using MySqlConnector;
using System.Collections.ObjectModel; //Used to notify listview values when objects are changed
using System.Diagnostics; //Debug
using Windows.Web.Http; //For POST method
using System.Threading.Tasks; //For POST method
using System.Text.Json; //Used for (de)serizalisation and JSON manipulation
using System.Text.Json.Serialization; //Used as serialization library

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RecommendationSystem : Page
    {
        public RecommendationSystem()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }

        public partial class GameListObject
        {
            [JsonPropertyName("id")]
            public long GameID { get; set; }

            [JsonPropertyName("name")]
            public string GameName { get; set; }

            [JsonPropertyName("platforms")]
            public ObservableCollection<GamePlatform> GamePlatforms { get; set; }
        }

        public partial class GamePlatform
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public static string GameGenre;

        public static string FinalPlatform;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Check if user has previously calibrated and retrieve the result genre and platforms
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                //Check whether the user has previously used rec system
                //Sets variables and SQL command
                cmd.CommandText = "SELECT RecID, FinalGenre, PlatformPref FROM recommend_data WHERE UserID=@UserID";
                cmd.Parameters.AddWithValue("@UserID", App.GlobalUserID);

                MySqlDataReader details = cmd.ExecuteReader();

                if (!details.Read())
                {
                    //Navigate to the Calibration page if a ReviewID doesn't already exist
                    this.Frame.Navigate(typeof(RecCalibration), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                }

                else
                {
                    //Set results as variables
                    GameGenre = Convert.ToString(details["FinalGenre"]);
                    FinalPlatform = Convert.ToString(details["PlatformPref"]);

                    //Display the result game genre
                    game_genre.Text = "Alogrithm result genre: " + GameGenre;

                    conn.Close();

                    try
                    {
                        // Construct the HttpClient and Uri
                        HttpClient httpClient = new HttpClient();
                        Uri uri = new Uri("https://api.igdb.com/v4/games");

                        httpClient.DefaultRequestHeaders.Add("Client-ID", App.GlobalClientidIGDB);
                        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.GlobalAccessIGDB);
                        //Debug.WriteLine("Request Headers: ");

                        // Construct the JSON to post
                        HttpStringContent content = new HttpStringContent($"fields id,name,platforms.name,websites.url; where rating >= 85 & platforms = ({FinalPlatform}); limit 500;");
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
                        }
                        //Add all the items once all of the items are ready to be added
                        recsys_list.ItemsSource = GameList;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        private void recsys_list_ItemClick(object sender, ItemClickEventArgs e) //When an item in List View is pressed
        {

        }

        private void recalibrate_button_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = "DELETE FROM recommend_data WHERE UserID=@userid"; //Deletes the VoteID in the review_votes table
                    cmd.Parameters.AddWithValue("@userid", App.GlobalUserID); //Sets them as variables
                    cmd.ExecuteScalar();

                    conn.Close();
                    this.Frame.Navigate(typeof(RecommendationSystem), null); //Switch landing page
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }
}
