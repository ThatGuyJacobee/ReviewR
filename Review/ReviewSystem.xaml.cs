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
    public sealed partial class ReviewSystem : Page
    {
        public ReviewSystem()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }

        public partial class ReviewObject
        {
            public int ReviewID { get; set; }
            public int UserID { get; set; }
            public string GameName { get; set; }
            public string GameTitle { get; set; }
            public string RevDesc { get; set; }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool reviewFetchSuccessful = DataFetch(); //Run the DataValidation method

            if (reviewFetchSuccessful)
            {
                Debug.WriteLine("Review Fetching Successful!");
            }

            else
            {
                Debug.WriteLine("Review Fetching unsuccessful!");
            }
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void createnew_next_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CreateReview), null); //When button pressed, move to create review page
        }

        private bool DataFetch() //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT ReviewID, UserID, RevGame, RevTitle, RevDesc FROM review_data ORDER BY ReviewID DESC LIMIT 10"; //Selects the email and password rows from user_data
                cmd.Connection = conn;

                MySqlDataReader reviewfetch = cmd.ExecuteReader(); //Executes a read command for the table
                if (reviewfetch.Read())
                {
                    ObservableCollection<ReviewObject> ReviewList = new ObservableCollection<ReviewObject>();

                    do
                    {
                        //Set results as variables
                        var ReviewID = Convert.ToInt32(reviewfetch["ReviewID"]);
                        var UserID = Convert.ToInt32(reviewfetch["UserID"]);
                        var ReviewGame = Convert.ToString(reviewfetch["RevGame"]);
                        var ReviewTitle = Convert.ToString(reviewfetch["RevTitle"]);
                        var ReviewDesc = Convert.ToString(reviewfetch["RevDesc"]);
                        Debug.WriteLine("Review ID: " + ReviewID);
                        Debug.WriteLine("User ID: " + UserID);
                        Debug.WriteLine("Game Reviewed: " + ReviewGame);
                        Debug.WriteLine("Game Title: " + ReviewTitle);
                        Debug.WriteLine("Review Description: " + ReviewDesc);

                        ReviewObject add = new ReviewObject() { ReviewID = ReviewID, UserID = UserID, GameName = ReviewGame, GameTitle = ReviewTitle, RevDesc = ReviewDesc };
                        ReviewList.Add(add); //Adds item to the temporary list
                    }
                    
                    while (reviewfetch.Read());

                    reviewsearch_list.ItemsSource = ReviewList; //Inserts all items at once into the listview
                    conn.Close(); //Close connection
                    return true;
                }
                else
                {
                    conn.Close(); //Close connection
                    return false;
                }
            }
        }

        private void rev_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            //If the search box is empty, then run the latest reviews method
            if (string.IsNullOrEmpty(rev_search.Text))
            {
                DataFetch();
            }

            //Otherwise search for whatever the input is within the database
            else
            {
                using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();
                    notfound_text.Visibility = Visibility.Collapsed;
                    var sqlsearch = rev_search.Text;

                    cmd.CommandText = "SELECT ReviewID, UserID, RevGame, RevTitle, RevDesc FROM review_data WHERE RevGame LIKE '%"+ sqlsearch +"%' OR RevTitle LIKE '%"+ sqlsearch +"%' ORDER BY ReviewID DESC LIMIT 10"; //Selects the email and password rows from user_data
                    cmd.Connection = conn;

                    MySqlDataReader reviewfetch = cmd.ExecuteReader(); //Executes a read command for the table
                    if (reviewfetch.Read())
                    {
                        ObservableCollection<ReviewObject> ReviewList = new ObservableCollection<ReviewObject>();

                        do
                        {
                            //Set results as variables
                            var ReviewID = Convert.ToInt32(reviewfetch["ReviewID"]);
                            var UserID = Convert.ToInt32(reviewfetch["UserID"]);
                            var ReviewGame = Convert.ToString(reviewfetch["RevGame"]);
                            var ReviewTitle = Convert.ToString(reviewfetch["RevTitle"]);
                            var ReviewDesc = Convert.ToString(reviewfetch["RevDesc"]);
                            Debug.WriteLine("Review ID: " + ReviewID);
                            Debug.WriteLine("User ID: " + UserID);
                            Debug.WriteLine("Game Reviewed: " + ReviewGame);
                            Debug.WriteLine("Game Title: " + ReviewTitle);
                            Debug.WriteLine("Review Description: " + ReviewDesc);

                            ReviewObject add = new ReviewObject() { ReviewID = ReviewID, UserID = UserID, GameName = ReviewGame, GameTitle = ReviewTitle, RevDesc = ReviewDesc };
                            ReviewList.Add(add); //Adds item to the temporary list
                        }

                        while (reviewfetch.Read());

                        reviewsearch_list.ItemsSource = ReviewList; //Inserts all items at once into the listview
                        conn.Close(); //Close connection
                    }

                    else
                    {
                        notfound_text.Visibility = Visibility.Visible;
                        reviewsearch_list.ItemsSource = null;
                    }
                }
            }
        }

        //Set the GameID as a static long variable which I will use to request further data about the game in the game-specific pages
        public static int ReviewSpecificID = 0;

        //Set the Game Name as above
        public static int ReviewSpecificUserID = 0;

        //Set the Game Name as above
        public static string ReviewSpecificGameName = "";

        //Set the Game Name as above
        public static string ReviewSpecificGameTitle = "";

        //Set the Game Name as above
        public static string ReviewSpecificDescription = "";

        private void reviewsearch_list_ItemClick(object sender, ItemClickEventArgs e) //When an item in List View is pressed
        {
            var clickedItem = e.ClickedItem as ReviewObject;
            Debug.WriteLine("Click Item GameID: " + clickedItem.GameName);
            Debug.WriteLine("Click Item Game Name: " + clickedItem.GameTitle);

            //Set the GameID as a static long variable
            ReviewSpecificID = clickedItem.ReviewID;
            ReviewSpecificUserID = clickedItem.UserID;
            ReviewSpecificGameName = clickedItem.GameName;
            ReviewSpecificGameTitle = clickedItem.GameTitle;
            ReviewSpecificDescription = clickedItem.RevDesc;

            this.Frame.Navigate(typeof(ReviewSpecificPages), null); //Switch to the game-specific page
        }
    }
}
