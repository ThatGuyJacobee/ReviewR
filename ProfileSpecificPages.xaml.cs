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
    public sealed partial class ProfileSpecificPages : Page
    {
        public ProfileSpecificPages()
        {
            this.InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        public partial class UserReviewObject
        {
            public string GameName { get; set; }
            public string GameTitle { get; set; }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            username_title.Text = ProfilePages.ProfileSpecificUsername + " [" + ProfilePages.ProfileSpecificUserID + "]";
            profilebio_text.Text = ProfilePages.ProfileSpecificUserBio;

            if (ProfilePages.ProfileSpecificUserAvatar != "" && Uri.IsWellFormedUriString(ProfilePages.ProfileSpecificUserAvatar, UriKind.Absolute))
            {
                user_avatar.Source = new BitmapImage(new Uri(ProfilePages.ProfileSpecificUserAvatar));
            }

            lastonline_text.Text = "Last Online: " + ProfilePages.ProfileSpecificLastLogon;

            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT RevGame, RevTitle FROM review_data WHERE UserID=@UserID ORDER BY ReviewID DESC LIMIT 10"; //Selects the email and password rows from user_data
                cmd.Parameters.AddWithValue("@UserID", ProfilePages.ProfileSpecificUserID); //Sets them as variables
                cmd.Connection = conn;

                MySqlDataReader reviewfetch = cmd.ExecuteReader(); //Executes a read command for the table
                if (reviewfetch.Read())
                {
                    ObservableCollection<UserReviewObject> ReviewList = new ObservableCollection<UserReviewObject>();

                    do
                    {
                        var ReviewGame = Convert.ToString(reviewfetch["RevGame"]);
                        var ReviewTitle = Convert.ToString(reviewfetch["RevTitle"]);
                        Debug.WriteLine("Game Reviewed: " + ReviewGame);
                        Debug.WriteLine("Game Title: " + ReviewTitle);

                        UserReviewObject add = new UserReviewObject() { GameName = ReviewGame, GameTitle = ReviewTitle };
                        ReviewList.Add(add); //Adds item to the temporary list
                    }

                    while (reviewfetch.Read());

                    userreviews_list.ItemsSource = ReviewList; //Inserts all items at once into the listview
                    conn.Close(); //Close connection
                }

                else
                {
                    notfound_text.Visibility = Visibility.Visible;
                    userreviews_list.ItemsSource = null;
                }
            }
        }
    }
}
