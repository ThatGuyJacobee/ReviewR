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
using Windows.UI.Xaml.Media.Imaging; //For using image to URL links

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
                cmd.CommandText = "SELECT lastlogon, creationdate, useravatar, email FROM user_data WHERE UserID=@UserID";
                cmd.Parameters.AddWithValue("@UserID", App.GlobalUserID);

                MySqlDataReader details = cmd.ExecuteReader();

                if (details.Read())
                {
                    var lastlogon = Convert.ToString(details["lastlogon"]);
                    var creationdate = Convert.ToString(details["creationdate"]);
                    var useravatar = Convert.ToString(details["useravatar"]);
                    PasswordResetDialog.ResetEmail = Convert.ToString(details["email"]);

                    lastlogon_date.Text = lastlogon;
                    accountcreated_date.Text = creationdate;

                    if (useravatar != "" && Uri.IsWellFormedUriString(useravatar, UriKind.Absolute)) //Added check to ensure URL is valid
                    {
                        user_avatar.Source = new BitmapImage(new Uri(useravatar));
                    }

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

                else
                {
                    notfound_text.Visibility = Visibility.Visible;
                    myreviews_list.ItemsSource = null;
                }
            }

            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = "SELECT UserID, IsAdmin FROM user_data WHERE UserID=@userid"; //Selects the email and password rows from user_data
                    cmd.Parameters.AddWithValue("@userid", App.GlobalUserID); //Sets them as variables
                    cmd.Connection = conn;

                    MySqlDataReader adminread = cmd.ExecuteReader();

                    if (adminread.Read())
                    {
                        var AdminCheck = Convert.ToString(adminread["IsAdmin"]);

                        if (AdminCheck == "Administrator")
                        {
                            admin_panel.Visibility = Visibility.Visible;
                        }
                    }
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private void myreviews_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void accountcreated_title_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private async void account_edit_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog usersettingschoice = new SettingsChooseDialog();
            
            await usersettingschoice.ShowAsync();
        }

        private void profile_preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();
                    notfound_text.Visibility = Visibility.Collapsed;

                    cmd.CommandText = "SELECT UserID, Username, UserBio, UserAvatar, LastLogon FROM user_data WHERE UserID=@userid"; //Searches for usernames and returns the needed values
                    cmd.Parameters.AddWithValue("@userid", App.GlobalUserID);
                    cmd.Connection = conn;

                    MySqlDataReader reviewfetch = cmd.ExecuteReader(); //Executes a read command for the table
                    if (reviewfetch.Read())
                    {
                        //Set results as variables
                        ProfilePages.ProfileSpecificUserID = Convert.ToInt32(reviewfetch["UserID"]);
                        ProfilePages.ProfileSpecificUsername = Convert.ToString(reviewfetch["Username"]);
                        ProfilePages.ProfileSpecificUserBio = Convert.ToString(reviewfetch["UserBio"]);
                        ProfilePages.ProfileSpecificUserAvatar = Convert.ToString(reviewfetch["UserAvatar"]);
                        ProfilePages.ProfileSpecificLastLogon = Convert.ToString(reviewfetch["LastLogon"]);
                        Debug.WriteLine("User ID: " + ProfilePages.ProfileSpecificUserID);
                        Debug.WriteLine("Username : " + ProfilePages.ProfileSpecificUsername);
                        Debug.WriteLine("User Bio: " + ProfilePages.ProfileSpecificUserBio);
                        Debug.WriteLine("User Avatar : " + ProfilePages.ProfileSpecificUserAvatar);
                        Debug.WriteLine("Last Logon : " + ProfilePages.ProfileSpecificLastLogon);

                        this.Frame.Navigate(typeof(ProfileSpecificPages), null); //Switch to the profile-specific page
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void admin_panel_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminPanel), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }
    }
}
