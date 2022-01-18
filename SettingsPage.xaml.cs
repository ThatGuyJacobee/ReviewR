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
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel; //Used to notify listview values when objects are changed
using System.Diagnostics; //Debug

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        //Uses the static Connection String that was set in the Main App Class (private)
        private static string ConnectionString = App.ConnectionString;

        public SettingsPage()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }
        public partial class MyReviewObject
        {
            public string GameName { get; set; }
            public string GameTitle { get; set; }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Sets personalised elements on page load
            username_title.Text = App.GlobalUsername;

            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                //Sets variables and SQL command
                cmd.CommandText = "SELECT lastlogon, creationdate FROM user_data WHERE UserID=@UserID";
                cmd.Parameters.AddWithValue("@UserID", App.GlobalUserID);

                MySqlDataReader details = cmd.ExecuteReader();

                if (details.Read())
                {
                    var lastlogon = Convert.ToString(details["lastlogon"]);
                    var creationdate = Convert.ToString(details["creationdate"]);

                    lastlogon_date.Text = lastlogon;
                    accountcreated_date.Text = creationdate;

                    conn.Close();
                }
            }

            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT RevGame, RevTitle FROM review_data WHERE UserID=@UserID ORDER BY ReviewID DESC LIMIT 10"; //Selects the email and password rows from user_data
                cmd.Parameters.AddWithValue("@UserID", App.GlobalUserID); //Sets them as variables
                cmd.Connection = conn;

                MySqlDataReader reviewfetch = cmd.ExecuteReader(); //Executes a read command for the table
                if (reviewfetch.Read())
                {
                    ObservableCollection<MyReviewObject> ReviewList = new ObservableCollection<MyReviewObject>();

                    do
                    {
                        var ReviewGame = Convert.ToString(reviewfetch["RevGame"]);
                        var ReviewTitle = Convert.ToString(reviewfetch["RevTitle"]);
                        Debug.WriteLine("Game Reviewed: " + ReviewGame);
                        Debug.WriteLine("Game Title: " + ReviewTitle);

                        MyReviewObject add = new MyReviewObject() { GameName = ReviewGame, GameTitle = ReviewTitle };
                        ReviewList.Add(add); //Adds item to the temporary list
                    }

                    while (reviewfetch.Read());

                    myreviews_list.ItemsSource = ReviewList; //Inserts all items at once into the listview
                    conn.Close(); //Close connection
                }
            }
        }

        private void myreviews_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void account_avatar_Click(object sender, RoutedEventArgs e)
        {
            //When the Register Account button is clicked, display the Register Content Dialog
            ContentDialog uploaddialog = new UploadAvatar();
            await uploaddialog.ShowAsync();
        }

        private void accountcreated_title_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
