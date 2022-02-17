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
    public sealed partial class RecCalibration : Page
    {
        public RecCalibration()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;

            calhelp_description.Text = "Hey username! As this is your first time here, you will be required to calibrate your preferences through an algorithm before you receive personalised recommendations.\nPlease select which game platform you want to find games from and a date from which games should be searched from.";

            // Set minimum to the current year and maximum to five years from now.
            date_pick.MinYear = new DateTimeOffset(new DateTime(2005, 1, 1));
            date_pick.MaxYear = DateTimeOffset.Now;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StartGrid.Visibility = Visibility.Visible;
            GameGrid.Visibility = Visibility.Collapsed;
            DislikeGrid.Visibility = Visibility.Collapsed;
            LikeGrid.Visibility = Visibility.Collapsed;
        }

        public partial class GameListObject
        {
            [JsonPropertyName("id")]
            public long GameID { get; set; }

            [JsonPropertyName("name")]
            public string GameName { get; set; }

            [JsonPropertyName("genres")]
            public ObservableCollection<GameGenre> GameGenres { get; set; }

            [JsonPropertyName("summary")]
            public string GameSummary { get; set; }

            [JsonPropertyName("platforms")]
            public ObservableCollection<GamePlatform> GamePlatforms { get; set; }

            //[JsonPropertyName("multiplayer_modes")]
            //public string GameType { get; set; }
        }

        public partial class GameGenre
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public partial class GamePlatform
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public partial class GameCount
        {
            [JsonPropertyName("count")]
            public int TotalCount { get; set; }
        }

        public static int TotalGameCount;

        private async void GetCount()
        {
            try
            {
                // Construct the HttpClient and Uri
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri("https://api.igdb.com/v4/games/count");

                httpClient.DefaultRequestHeaders.Add("Client-ID", App.GlobalClientidIGDB);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.GlobalAccessIGDB);
                //Debug.WriteLine("Request Headers: ");

                // Construct the JSON to post
                HttpStringContent content = new HttpStringContent($"where rating >= 1;");
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
                GameCount gamelistobjects = JsonSerializer.Deserialize<GameCount>(httpResponseBody);

                TotalGameCount = Convert.ToInt32(gamelistobjects.TotalCount);
                Debug.WriteLine($"count: {gamelistobjects.TotalCount}");
                
                //Perform random math for game id
                RandomMath();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static int RandomGameID;

        private void RandomMath()
        {
            Random rnd = new Random();
            RandomGameID = rnd.Next(1, TotalGameCount);
            Debug.WriteLine("Random Game Calc result: " + RandomGameID);
            RandomGameFetch();
        }

        public static string FetchedGenre;

        private async void RandomGameFetch()
        {
            try
            {
                // Construct the HttpClient and Uri
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri("https://api.igdb.com/v4/games");

                httpClient.DefaultRequestHeaders.Add("Client-ID", App.GlobalClientidIGDB);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.GlobalAccessIGDB);
                //Debug.WriteLine("Request Headers: ");

                // Construct the JSON to post
                HttpStringContent content = new HttpStringContent($"fields id,name,genres.name,summary,platforms.name; where rating >= 75 & id = {RandomGameID} & platforms = ({UserGenres}) & first_release_date > {DateFilter}; limit 1;");
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

                //If the response is empty (no game under that ID exists) then math random again
                if (httpResponseBody == "[]")
                {
                    Debug.WriteLine("Recalculating random Game ID as current is empty...");
                    RandomMath();
                }

                else
                {
                    //For each item that is within the dataList
                    foreach (var item in dataList)
                    {
                        //If there are no genres set, then don't run the statement
                        if (item.GameGenres != null)
                        {
                            //For each genre that is found within releasedates
                            foreach (var genre in item.GameGenres)
                            {
                                FetchedGenre = Convert.ToString(genre.Id); //Select the genre ids rather than plain name
                                game_genre.Text = genre.Name + " [" + genre.Id + "]";
                                Debug.WriteLine($"genres: {genre.Name}");
                                Debug.WriteLine($"genreid: {genre.Id}");
                            }
                        }

                        //If there are no platforms set, then don't run the statement
                        if (item.GamePlatforms != null)
                        {
                            //For each genre that is found within releasedates
                            foreach (var platform in item.GamePlatforms)
                            {
                                game_platform.Text = platform.Name;
                                Debug.WriteLine($"platforms: {platform.Name}");
                            }
                        }

                        game_title.Text = item.GameName + " [" + item.GameID + "]";
                        game_summary.Text = item.GameSummary;
                        //game_type.Text = item.GameType;

                        Debug.WriteLine($"id: {item.GameID}");
                        Debug.WriteLine($"name: {item.GameName}");
                        //Debug.WriteLine($"genres: {item.GameGenre}");
                        Debug.WriteLine($"summary: {item.GameSummary}");
                        //Debug.WriteLine($"platforms: {item.GamePlatform}");
                        //Debug.WriteLine($"multiplayer_modes: {item.GameType}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static string UserGenres = "";

        public static long DateFilter;

        private void startrec_button_Click(object sender, RoutedEventArgs e)
        {
            //If none is on then don't allow the user to pass
            if (!pc_switch.IsOn && !playstation_switch.IsOn && !xbox_switch.IsOn)
            {
                //Display the error requiring user to pick at least one
                noswitch_error.Visibility = Visibility.Visible;

                DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(Convert.ToString(date_pick.Date));

                DateFilter = dateTimeOffset.ToUnixTimeMilliseconds();
                Debug.WriteLine("Unix calc result: " + DateFilter);
            }

            else
            {
                StartGrid.Visibility = Visibility.Collapsed; //Hide Start Grid
                GameGrid.Visibility = Visibility.Visible; //Displaay the main Calibration Grid
                DislikeGrid.Visibility = Visibility.Visible;
                LikeGrid.Visibility = Visibility.Visible;

                if (pc_switch.IsOn && playstation_switch.IsOn && xbox_switch.IsOn)
                {
                    UserGenres = "6,9,48,167,12,49,169";
                }

                else if (!pc_switch.IsOn && playstation_switch.IsOn && xbox_switch.IsOn)
                {
                    UserGenres = "9,48,167,12,49,169";
                }

                else if (!pc_switch.IsOn && !playstation_switch.IsOn && xbox_switch.IsOn)
                {
                    UserGenres = "12,49,169";
                }

                else if (pc_switch.IsOn && !playstation_switch.IsOn && xbox_switch.IsOn)
                {
                    UserGenres = "6,12,49,169";
                }

                else if (pc_switch.IsOn && !playstation_switch.IsOn && !xbox_switch.IsOn)
                {
                    UserGenres = "6";
                }

                //Set message to searching for game (only runs once rather than the random math)
                game_title.Text = "Awaiting API response...";
                game_genre.Text = "...";
                game_type.Text = "...";
                game_platform.Text = "...";
                game_summary.Text = "...";

                //Get a total count for games to be able to do math random for random game display
                GetCount();
            }
        }

        private void like_button_Click(object sender, RoutedEventArgs e)
        {
            //Perform the next game random search
            RandomMath();

            //Set message to searching for game (only runs once rather than the random math)
            game_title.Text = "Awaiting API response...";
            game_genre.Text = "...";
            game_type.Text = "...";
            game_platform.Text = "...";
            game_summary.Text = "...";
        }

        private void dislike_button_Click(object sender, RoutedEventArgs e)
        {
            //Perform the next game random search
            RandomMath();

            //Set message to searching for game (only runs once rather than the random math)
            game_title.Text = "Awaiting API response...";
            game_genre.Text = "...";
            game_type.Text = "...";
            game_platform.Text = "...";
            game_summary.Text = "...";
        }
    }
}
