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
    public sealed partial class ProfilePages : Page
    {
        public ProfilePages()
        {
            this.InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        public partial class ProfileObject
        {
            public int UserID { get; set; }
            public string Username { get; set; }
            public string UserBio { get; set; }
            public string UserAvatar { get; set; }
            public string LastLogon { get; set; }
        }

        //Set the UserID as a static int variable
        public static int ProfileSpecificUserID = 0;

        public static string ProfileSpecificUsername = "";

        public static string ProfileSpecificUserBio = "";

        public static string ProfileSpecificUserAvatar = "";

        public static string ProfileSpecificLastLogon = "";

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool profilefetchsuccess = DataFetch();

            if (profilefetchsuccess)
            {
                Debug.WriteLine("Profile Page Fetching Successful!");
            }

            else
            {
                Debug.WriteLine("Profile Page Fetching Unsuccessful!");
            }
        }

        private void profile_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            //If the search box is empty, then run the latest reviews method
            if (string.IsNullOrEmpty(profile_search.Text))
            {
                DataFetch();
            }

            else
            {
                using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();
                    notfound_text.Visibility = Visibility.Collapsed;
                    var sqlsearch = profile_search.Text;

                    cmd.CommandText = "SELECT UserID, Username, UserBio, UserAvatar, LastLogon FROM user_data WHERE Username LIKE '%" + sqlsearch + "%' ORDER BY UserID DESC LIMIT 10"; //Searches for usernames and returns the needed values
                    cmd.Connection = conn;

                    MySqlDataReader reviewfetch = cmd.ExecuteReader(); //Executes a read command for the table
                    if (reviewfetch.Read())
                    {
                        ObservableCollection<ProfileObject> ProfileList = new ObservableCollection<ProfileObject>();

                        do
                        {
                            //Set results as variables
                            var UserID = Convert.ToInt32(reviewfetch["UserID"]);
                            var Username = Convert.ToString(reviewfetch["Username"]);
                            var UserBio = Convert.ToString(reviewfetch["UserBio"]);
                            var UserAvatar = Convert.ToString(reviewfetch["UserAvatar"]);
                            var LastLogon = Convert.ToString(reviewfetch["LastLogon"]);
                            Debug.WriteLine("User ID: " + UserID);
                            Debug.WriteLine("Username : " + Username);
                            Debug.WriteLine("User Bio: " + UserBio);
                            Debug.WriteLine("User Avatar : " + UserAvatar);
                            Debug.WriteLine("Last Logon : " + LastLogon);

                            ProfileObject add = new ProfileObject() { UserID = UserID, Username = Username, UserBio = UserBio, UserAvatar = UserAvatar, LastLogon = LastLogon };
                            ProfileList.Add(add); //Adds item to the temporary list
                        }

                        while (reviewfetch.Read());

                        profile_list.ItemsSource = ProfileList; //Inserts all items at once into the listview
                        conn.Close(); //Close connection
                    }

                    else
                    {
                        notfound_text.Visibility = Visibility.Visible;
                        profile_list.ItemsSource = null;
                    }
                }
            }
        }

        private bool DataFetch()
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                notfound_text.Visibility = Visibility.Collapsed;
                var sqlsearch = profile_search.Text;

                cmd.CommandText = "SELECT UserID, Username, UserBio, UserAvatar, LastLogon FROM user_data ORDER BY UserID ASC LIMIT 25"; //Searches for usernames and returns the needed values
                cmd.Connection = conn;

                MySqlDataReader reviewfetch = cmd.ExecuteReader(); //Executes a read command for the table
                if (reviewfetch.Read())
                {
                    ObservableCollection<ProfileObject> ProfileList = new ObservableCollection<ProfileObject>();

                    do
                    {
                        //Set results as variables
                        var UserID = Convert.ToInt32(reviewfetch["UserID"]);
                        var Username = Convert.ToString(reviewfetch["Username"]);
                        var UserBio = Convert.ToString(reviewfetch["UserBio"]);
                        var UserAvatar = Convert.ToString(reviewfetch["UserAvatar"]);
                        var LastLogon = Convert.ToString(reviewfetch["LastLogon"]);
                        Debug.WriteLine("User ID: " + UserID);
                        Debug.WriteLine("Username : " + Username);
                        Debug.WriteLine("User Bio: " + UserBio);
                        Debug.WriteLine("User Avatar : " + UserAvatar);
                        Debug.WriteLine("Last Logon : " + LastLogon);

                        ProfileObject add = new ProfileObject() { UserID = UserID, Username = Username, UserBio = UserBio, UserAvatar = UserAvatar, LastLogon = LastLogon };
                        ProfileList.Add(add); //Adds item to the temporary list
                    }

                    while (reviewfetch.Read());

                    profile_list.ItemsSource = ProfileList; //Inserts all items at once into the listview
                    conn.Close(); //Close connection
                    return true;
                }

                else
                {
                    notfound_text.Visibility = Visibility.Visible;
                    profile_list.ItemsSource = null;
                    return false;
                }
            }
        }

        private void profile_list_ItemClick(object sender, ItemClickEventArgs e) //When an item in List View is pressed
        {
            var clickedItem = e.ClickedItem as ProfileObject;
            Debug.WriteLine("Click Item UserID: " + clickedItem.UserID);
            Debug.WriteLine("Click Item Username: " + clickedItem.Username);

            //Set the GameID as a static long variable
            ProfileSpecificUserID = clickedItem.UserID;
            ProfileSpecificUsername = clickedItem.Username;
            ProfileSpecificUserBio = clickedItem.UserBio;
            ProfileSpecificUserAvatar = clickedItem.UserAvatar;
            ProfileSpecificLastLogon = clickedItem.LastLogon;

            this.Frame.Navigate(typeof(ProfileSpecificPages), null); //Switch to the profile-specific page
        }

        private void myprofile_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();
                    notfound_text.Visibility = Visibility.Collapsed;
                    var sqlsearch = profile_search.Text;

                    cmd.CommandText = "SELECT UserID, Username, UserBio, UserAvatar, LastLogon FROM user_data WHERE UserID=@userid"; //Searches for usernames and returns the needed values
                    cmd.Parameters.AddWithValue("@userid", App.GlobalUserID);
                    cmd.Connection = conn;

                    MySqlDataReader reviewfetch = cmd.ExecuteReader(); //Executes a read command for the table
                    if (reviewfetch.Read())
                    {
                        //Set results as variables
                        ProfileSpecificUserID = Convert.ToInt32(reviewfetch["UserID"]);
                        ProfileSpecificUsername = Convert.ToString(reviewfetch["Username"]);
                        ProfileSpecificUserBio = Convert.ToString(reviewfetch["UserBio"]);
                        ProfileSpecificUserAvatar = Convert.ToString(reviewfetch["UserAvatar"]);
                        ProfileSpecificLastLogon = Convert.ToString(reviewfetch["LastLogon"]);
                        Debug.WriteLine("User ID: " + ProfileSpecificUserID);
                        Debug.WriteLine("Username : " + ProfileSpecificUsername);
                        Debug.WriteLine("User Bio: " + ProfileSpecificUserBio);
                        Debug.WriteLine("User Avatar : " + ProfileSpecificUserAvatar);
                        Debug.WriteLine("Last Logon : " + ProfileSpecificLastLogon);

                        this.Frame.Navigate(typeof(ProfileSpecificPages), null); //Switch to the profile-specific page
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
