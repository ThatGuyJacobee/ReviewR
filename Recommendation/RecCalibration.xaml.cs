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
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StartGrid.Visibility = Visibility.Visible;
            GameGrid.Visibility = Visibility.Collapsed;
            DislikeGrid.Visibility = Visibility.Collapsed;
            LikeGrid.Visibility = Visibility.Collapsed;

            calhelp_description.Text = $"Hey {App.GlobalUsername}! As this is your first time here, you will be required to calibrate your preferences through an algorithm before you receive personalised recommendations.\nPlease select which game platform you want to find games from and a date from which games should be searched from.";

            // Set minimum to the current year and maximum to five years from now.
            date_pick.MinYear = new DateTimeOffset(new DateTime(2005, 1, 1));
            date_pick.MaxYear = DateTimeOffset.Now;

            //Reset texts
            game_genre.Text = "";
            game_type.Text = "";
            game_platform.Text = "";
            game_summary.Text = "";
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

        //public static string FetchedGenre;

        public List<string> TempGenre = new List<string>();

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
                    game_waiting.Visibility = Visibility.Collapsed; //Collapse waiting text

                    //For each item that is within the dataList
                    foreach (var item in dataList)
                    {
                        //If there are no genres set, then don't run the statement
                        if (item.GameGenres != null)
                        {
                            //For each genre that is found within releasedates
                            foreach (var genre in item.GameGenres)
                            {
                                //FetchedGenre = Convert.ToString(genre.Id); //Select the genre ids rather than plain name
                                game_genre.Text = game_genre.Text + genre.Name + " [" + genre.Id + "], ";

                                //Add each genre to the TempGenre list
                                TempGenre.Add(genre.Name);

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
                                game_platform.Text = game_platform.Text + platform.Name + ", ";
                                Debug.WriteLine($"platforms: {platform.Name}");
                            }
                        }

                        for (int i = 0; i < TempGenre.Count; i++)
                            Debug.WriteLine("Temp Array item: " + TempGenre[i]);

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
            //Calculate the time
            DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(Convert.ToString(date_pick.Date));

            DateFilter = dateTimeOffset.ToUnixTimeMilliseconds();
            Debug.WriteLine("Unix calc result: " + DateFilter);

            //If none is on then don't allow the user to pass
            if (!pc_switch.IsOn && !playstation_switch.IsOn && !xbox_switch.IsOn || DateFilter == -11644473600000)
            {
                //Display the error requiring user to pick at least one
                noswitch_error.Visibility = Visibility.Visible;
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

                else if (pc_switch.IsOn && playstation_switch.IsOn && !xbox_switch.IsOn)
                {
                    UserGenres = "6,9,48,167";
                }

                else if (!pc_switch.IsOn && playstation_switch.IsOn && !xbox_switch.IsOn)
                {
                    UserGenres = "9,48,167";
                }

                //Set message to searching for game (only runs once rather than the random math)
                game_title.Text = "Awaiting API response...";

                //Get a total count for games to be able to do math random for random game display
                GetCount();
            }
        }

        public List<string> LikedGenres = new List<string>();

        public static string FinalGenre;

        private void like_button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < TempGenre.Count; i++)
            {
                //Add copy each genre into the Liked Genre
                LikedGenres.Add(TempGenre[i]);
                Debug.WriteLine("Liked Array item: " + TempGenre[i]);
            }

            //After copying all, clear TempGenre list
            TempGenre.Clear();

            //Count total occurances of Shooter genre
            int shooter = LikedGenres.Count(s => s == "Shooter");
            Debug.WriteLine("Total shooter genre: " + shooter);

            //Count total occurances of Adventure genre
            int adventure = LikedGenres.Count(s => s == "Adventure");
            Debug.WriteLine("Total adventure genre: " + adventure);

            //Count total occurances of Sport genre
            int sport = LikedGenres.Count(s => s == "Sport");
            Debug.WriteLine("Total sport genre: " + sport);

            //Count total occurances of Puzzle genre
            int puzzle = LikedGenres.Count(s => s == "Puzzle");
            Debug.WriteLine("Total puzzle genre: " + puzzle);

            //Count total occurances of Indie genre
            int indie = LikedGenres.Count(s => s == "Indie");
            Debug.WriteLine("Total indie genre: " + indie);

            //Count total occurances of Strategy genre
            int strategy = LikedGenres.Count(s => s == "Strategy");
            Debug.WriteLine("Total strategy genre: " + strategy);

            //Count total occurances of Arcade genre
            int arcade = LikedGenres.Count(s => s == "Arcade");
            Debug.WriteLine("Total arcade genre: " + arcade);

            //Count total occurances of Racing genre
            int racing = LikedGenres.Count(s => s == "Racing");
            Debug.WriteLine("Total racing genre: " + racing);

            //Count total occurances of Simulator genre
            int simulator = LikedGenres.Count(s => s == "Simulator");
            Debug.WriteLine("Total simulator genre: " + simulator);

            //Count total occurances of Music genre
            int music = LikedGenres.Count(s => s == "Music");
            Debug.WriteLine("Total music genre: " + music);

            if (shooter >= 5) //If shooter genre is equal or more than 3
            {
                FinalGenre = "Shooter";
                FinishedCalibration();
            }

            else if (adventure >= 5)
            {
                FinalGenre = "Adventure";
                FinishedCalibration();
            }

            else if (sport >= 5)
            {
                FinalGenre = "Sport";
                FinishedCalibration();
            }

            else if (puzzle >= 5)
            {
                FinalGenre = "Puzzle";
                FinishedCalibration();
            }

            else if (indie >= 5)
            {
                FinalGenre = "Indie";
                FinishedCalibration();
            }

            else if (strategy >= 5)
            {
                FinalGenre = "Strategy";
                FinishedCalibration();
            }

            else if (arcade >= 5)
            {
                FinalGenre = "Arcade";
                FinishedCalibration();
            }

            else if (racing >= 5)
            {
                FinalGenre = "Racing";
                FinishedCalibration();
            }

            else if (simulator >= 5)
            {
                FinalGenre = "Simulator";
                FinishedCalibration();
            }

            else if (music >= 5)
            {
                FinalGenre = "Music";
                FinishedCalibration();
            }

            else
            {
                //Perform the next game random search
                RandomMath();

                //Set message to searching for game (only runs once rather than the random math)
                game_title.Text = "Awaiting API response...";
                game_waiting.Visibility = Visibility.Visible;

                //Reset texts
                game_genre.Text = "";
                game_type.Text = "";
                game_platform.Text = "";
                game_summary.Text = "";
            }
        }

        private void FinishedCalibration()
        {
            GameGrid.Visibility = Visibility.Collapsed;
            DislikeGrid.Visibility = Visibility.Collapsed;
            LikeGrid.Visibility = Visibility.Collapsed;

            EndGrid.Visibility = Visibility.Visible;

            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Selects the user_data table to insert email and password into
                    cmd.CommandText = "INSERT INTO recommend_data (UserID, FinalGenre, PlatformPref) VALUES (@userid, @finalgenre, @platformpref)";
                    cmd.Parameters.AddWithValue("@userid", App.GlobalUserID); //Sets them as variables
                    cmd.Parameters.AddWithValue("@finalgenre", FinalGenre);
                    cmd.Parameters.AddWithValue("@platformpref", UserGenres); //Variable to set the creation date alongside the account.

                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    Debug.WriteLine(ex);
                }
            }
        }

        private void dislike_button_Click(object sender, RoutedEventArgs e)
        {
            //Clear TempGenre list since user doesn't like that game
            TempGenre.Clear();

            //Perform the next game random search
            RandomMath();

            //Set message to searching for game (only runs once rather than the random math)
            game_title.Text = "Awaiting API response...";
            game_waiting.Visibility = Visibility.Visible;

            //Reset texts
            game_genre.Text = "";
            game_type.Text = "";
            game_platform.Text = "";
            game_summary.Text = "";
        }

        private void return_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(RecommendationSystem), null); //Switch landing page
        }
    }
}
