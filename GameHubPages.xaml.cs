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
    public sealed partial class GameHubPages : Page
    {
        public GameHubPages()
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

            [JsonPropertyName("release_dates")]
            public ObservableCollection<ReleaseDate> ReleaseDates { get; set; }

            [JsonPropertyName("url")]
            public string WebsiteLink { get; set; }

            [JsonPropertyName("genres")]
            public ObservableCollection<GameGenre> GameGenres { get; set; }

            [JsonPropertyName("summary")]
            public string GameSummary { get; set; }

            [JsonPropertyName("platforms")]
            public ObservableCollection<GamePlatform> GamePlatforms { get; set; }
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

        public partial class ReleaseDate
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("human")]
            public string Human { get; set; }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            username_text.Text = GameHubs.GameSpecificGameName + " [" + GameHubs.GameSpecificGameID + "]";

            try
            {
                // Construct the HttpClient and Uri
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri("https://api.igdb.com/v4/games");

                httpClient.DefaultRequestHeaders.Add("Client-ID", App.GlobalClientidIGDB);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.GlobalAccessIGDB);
                //Debug.WriteLine("Request Headers: ");

                // Construct the JSON to post
                HttpStringContent content = new HttpStringContent($"fields name,genres.name,summary,platforms.name,release_dates.human,external_games.category,url; where id = {GameHubs.GameSpecificGameID};");
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
                    Debug.WriteLine($"summary: {item.GameSummary}");
                    Debug.WriteLine($"url: {item.WebsiteLink}");

                    WebsiteURI = item.WebsiteLink;
                    game_summary.Text = item.GameSummary;

                    //If there are no genres set, then don't run the statement
                    if (item.GameGenres != null)
                    {
                        //For each genre that is found within releasedates
                        foreach (var genre in item.GameGenres)
                        {
                            //FetchedGenre = Convert.ToString(genre.Id); //Select the genre ids rather than plain name
                            game_genre.Text = game_genre.Text + genre.Name + " [" + genre.Id + "], ";

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
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static string WebsiteURI;

        private async void website_button_Click(object sender, RoutedEventArgs e)
        {
            string uriToLaunch = $"{WebsiteURI}";
            var uri = new Uri(uriToLaunch);

            // Set the option to show a warning
            var options = new Windows.System.LauncherOptions();
            options.TreatAsUntrusted = true;

            // Launch the URI with a warning prompt
            await Windows.System.Launcher.LaunchUriAsync(uri, options);
        }

        private void reviews_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ReviewSystem), null); //Switch to the review landing page for search
        }
    }
}
