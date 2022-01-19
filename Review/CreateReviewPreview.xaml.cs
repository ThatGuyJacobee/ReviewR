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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateReviewPreview : Page
    {
        public CreateReviewPreview()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            preview_gamename.Text = "Review Game: " + CreateReviewSearch.GameSpecificGameName + " (" + CreateReviewSearch.GameSpecificGameID + ")";
            preview_username.Text = "User: " + App.GlobalUsername + " (" + App.GlobalUserID + ")";
            preview_title.Text = CreateReviewDetails.ReviewTitle;
            preview_description.Text = CreateReviewDetails.ReviewDescription;
        }

        //Uses the static Connection String that was set in the Main App Class (private)
        private static string ConnectionString = App.ConnectionString;

        private bool DataInsertion(int userid, long revgameid, string revgame, string revtitle, string revdesc) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Selects the review_data table to insert a new review into
                    cmd.CommandText = "INSERT INTO review_data (userid, revgameid, revgame, revtitle, revdesc) VALUES (@userid, @revgameid, @revgame, @revtitle, @revdesc)";
                    cmd.Parameters.AddWithValue("@userid", userid); //Sets them as variables
                    cmd.Parameters.AddWithValue("@revgameid", revgameid);
                    cmd.Parameters.AddWithValue("@revgame", revgame);
                    cmd.Parameters.AddWithValue("@revtitle", revtitle);
                    cmd.Parameters.AddWithValue("@revdesc", revdesc);

                    //If statement performs a server-side (pre-insert) validation to ensure data matches requirements
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                catch
                {
                    conn.Close();
                    return false;
                }
            }
        }

        private async void continue_button_Click(object sender, RoutedEventArgs e)
        {
            int userid = App.GlobalUserID; //Inputs set as variables
            long revgameid = CreateReviewSearch.GameSpecificGameID;
            string revgame = CreateReviewSearch.GameSpecificGameName;
            string revtitle = CreateReviewDetails.ReviewTitle;
            string revdesc = CreateReviewDetails.ReviewDescription;

            bool ReviewCreateSuccess = DataInsertion(userid, revgameid, revgame, revtitle, revdesc); //Run the DataValidation method

            if (ReviewCreateSuccess)
            {
                ContentDialog successdialog = new ContentDialog();
                successdialog.Title = "Success!";
                successdialog.Content = "Your review has been successfully created!\nIt will be immediately available to everyone!";
                successdialog.CloseButtonText = "Approve";
                successdialog.DefaultButton = ContentDialogButton.Close;

                await successdialog.ShowAsync(); //Displays it until the close button is pressed
                this.Frame.Navigate(typeof(ReviewSystem), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()); //If successful navigate to the review landing page
            }

            else
            {
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Something went wrong.\nReport this issue to the developer.\n[ERROR CODE: REVIEW_INSERT_FAILURE]";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                await errordialog.ShowAsync();
            }
        }
    }
}
