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
            public string GameName { get; set; }
            public string GameTitle { get; set; }
        }

            private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool reviewFetchSuccessful = DataValidation(); //Run the DataValidation method

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

        private bool DataValidation() //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT RevGame, RevTitle FROM review_data ORDER BY ReviewID DESC LIMIT 10"; //Selects the email and password rows from user_data
                cmd.Connection = conn;

                MySqlDataReader reviewfetch = cmd.ExecuteReader(); //Executes a read command for the table
                if (reviewfetch.Read())
                {
                    ObservableCollection<ReviewObject> ReviewList = new ObservableCollection<ReviewObject>();

                    do
                    {
                        var ReviewGame = Convert.ToString(reviewfetch["RevGame"]);
                        var ReviewTitle = Convert.ToString(reviewfetch["RevTitle"]);
                        Debug.WriteLine("Game Reviewed: " + ReviewGame);
                        Debug.WriteLine("Game Title: " + ReviewTitle);

                        ReviewObject add = new ReviewObject() { GameName = ReviewGame, GameTitle = ReviewTitle };
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
            
        }
    }
}
