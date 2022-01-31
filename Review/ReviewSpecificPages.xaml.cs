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
using System.Diagnostics; //Debug
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReviewSpecificPages : Page
    {
        public ReviewSpecificPages()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }

        //Set the ReviewID as above
        public static string VoteType = "";

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            usernamereview_text.Text = "Review ID: " + ReviewSystem.ReviewSpecificID;
            reviewedgame_text.Text = ReviewSystem.ReviewSpecificGameName;
            reviewtitle_text.Text = ReviewSystem.ReviewSpecificGameTitle;
            reviewdescription_text.Text = ReviewSystem.ReviewSpecificDescription;

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

                        if (AdminCheck == "Administrator" || ReviewSystem.ReviewSpecificUserID == App.GlobalUserID)
                        {
                            delete_review.Visibility = Visibility.Visible;
                            edit_review.Visibility = Visibility.Visible;
                        }
                    }
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            //Checking if the UserID has already voted on this ReviewID
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = "SELECT VoteID, VoteType FROM review_votes WHERE UserID=@userid AND ReviewID=@reviewid"; //Selects the voteid and type where the current user's userid and reviewid matches
                    cmd.Parameters.AddWithValue("@userid", App.GlobalUserID); //Sets them as variables
                    cmd.Parameters.AddWithValue("@reviewid", ReviewSystem.ReviewSpecificID); //Sets them as variables
                    cmd.Connection = conn;

                    MySqlDataReader votingcheck = cmd.ExecuteReader();

                    if (votingcheck.Read())
                    {
                        //If it finds then a vote must already exist
                        VoteType = Convert.ToString(votingcheck["VoteType"]);

                        if (VoteType == "Upvote")
                        {
                            no_upvote.Visibility = Visibility.Collapsed;
                            upvote.Visibility = Visibility.Visible;
                        }

                        else if (VoteType == "Downvote")
                        {
                            no_downvote.Visibility = Visibility.Collapsed;
                            downvote.Visibility = Visibility.Visible;
                        }
                    }

                    else
                    {
                        //If it can't find a match, then a vote for this review id doesn't exist, so a new voteid is made which is set to default neutral vote
                        try
                        {
                            DataInsertion();
                        }

                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            UpdateVoteCount();
        }

        private void DataInsertion() //Method for inserting a new voteid
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                //Selects the review_votes table to insert the values into and create a new voteid
                cmd.CommandText = "INSERT INTO review_votes (ReviewID, UserID, VoteType) VALUES (@reviewid, @userid, @votetype)";
                cmd.Parameters.AddWithValue("@reviewid", ReviewSystem.ReviewSpecificID); //Sets them as variables
                cmd.Parameters.AddWithValue("@userid", App.GlobalUserID);
                cmd.Parameters.AddWithValue("@votetype", "Neutral");

                //Submits the insertiton command and closes connection
                cmd.ExecuteNonQuery();
                conn.Close();

                //Set the local VoteType to neutral since it's a new voteid
                VoteType = "Neutral";
            }
        }

        private bool DataDeletion()
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = "DELETE FROM review_votes WHERE ReviewID=@reviewid"; //Deletes the VoteID in the review_votes table
                    cmd.Parameters.AddWithValue("@reviewid", ReviewSystem.ReviewSpecificID); //Sets them as variables
                    cmd.ExecuteScalar();

                    cmd.CommandText = "DELETE FROM review_data WHERE ReviewID=@reviewid"; //Deletes the ReviewID in the review_data table

                    cmd.ExecuteScalar();
                    conn.Close();
                    return true;
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return false;
                }
            }
        }

        private void edit_review_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ReviewEdit), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private async void delete_review_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog checkdialog = new ContentDialog();
            checkdialog.Title = "Delete Review with ID: " + ReviewSystem.ReviewSpecificID;
            checkdialog.Content = "Are you sure that you wish to delete this review?\n\nWarning: This action is irreversible!";
            checkdialog.PrimaryButtonText = "Cancel";
            checkdialog.CloseButtonText = "Approve";
            checkdialog.DefaultButton = ContentDialogButton.Close;

            await checkdialog.ShowAsync(); //Displays it until the close button is pressed

            //Added a parameter runs to delete the review if approve button is pressed
            bool datadeletion = DataDeletion();
            checkdialog.CloseButtonCommandParameter = datadeletion;

            if (datadeletion)
            {
                checkdialog.Hide();

                ContentDialog successdialog = new ContentDialog();
                successdialog.Title = "Success!";
                successdialog.Content = "The review with the ID " + ReviewSystem.ReviewSpecificID + " has been deleted!";
                //successdialog.PrimaryButtonText = "Cancel";
                successdialog.CloseButtonText = "Approve";
                successdialog.DefaultButton = ContentDialogButton.Close;

                await successdialog.ShowAsync(); //Displays it until the close button is pressed
            }

            else
            {
                checkdialog.Hide();

                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Review was unsuccessfully deleted.";
                errordialog.Content = "This may be an issue on the server end, please try again. If it persists submit a bug report.\n\n[Error Code: DEL_FAILURE_DB]";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                await errordialog.ShowAsync(); //Displays it until the close button is pressed
            }
        }

        private void UpdateVoteCount()
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = "SELECT COUNT(VoteType) FROM review_votes WHERE ReviewID=@reviewid AND VoteType=@upvote";

                    cmd.Parameters.AddWithValue("@upvote", "Upvote");
                    cmd.Parameters.AddWithValue("@downvote", "Downvote");
                    cmd.Parameters.AddWithValue("@reviewid", ReviewSystem.ReviewSpecificID); //Sets them as variables
                    cmd.Connection = conn;

                    //Get a total count of upvotes
                    var UpvoteCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Debug.WriteLine("Total upvotes: " + UpvoteCount);

                    cmd.CommandText = "SELECT COUNT(VoteType) FROM review_votes WHERE ReviewID=@reviewid AND VoteType=@downvote";

                    //Get a total count of downvotes
                    var DownvoteCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Debug.WriteLine("Total downvotes: " + DownvoteCount);

                    var TotalCount = UpvoteCount - DownvoteCount;
                    Debug.WriteLine("Final overall vote count: " + TotalCount);

                    voting_total.Text = TotalCount.ToString();
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private void UpdateVoteType()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Sets variables and SQL command
                    cmd.CommandText = "UPDATE review_votes SET VoteType=@votetype WHERE UserID=@userid AND ReviewID=@reviewid";
                    cmd.Parameters.AddWithValue("@votetype", VoteType);
                    cmd.Parameters.AddWithValue("@userid", App.GlobalUserID);
                    cmd.Parameters.AddWithValue("@reviewid", ReviewSystem.ReviewSpecificID); //Sets them as variables

                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void no_upvote_Click(object sender, RoutedEventArgs e)
        {
            if (VoteType == "Neutral")
            {
                no_upvote.Visibility = Visibility.Collapsed;
                upvote.Visibility = Visibility.Visible;

                VoteType = "Upvote";
                UpdateVoteType();
                UpdateVoteCount();
            }

            else if (VoteType == "Downvote")
            {
                downvote.Visibility = Visibility.Collapsed;
                no_downvote.Visibility = Visibility.Visible;
                upvote.Visibility = Visibility.Visible;

                VoteType = "Upvote";
                UpdateVoteType();
                UpdateVoteCount();
            }
        }

        private void no_downvote_Click(object sender, RoutedEventArgs e)
        {
            if (VoteType == "Neutral")
            {
                no_downvote.Visibility = Visibility.Collapsed;
                downvote.Visibility = Visibility.Visible;

                VoteType = "Downvote";
                UpdateVoteType();
                UpdateVoteCount();
            }

            else if (VoteType == "Upvote")
            {
                upvote.Visibility = Visibility.Collapsed;
                no_upvote.Visibility = Visibility.Visible;
                downvote.Visibility = Visibility.Visible;

                VoteType = "Downvote";
                UpdateVoteType();
                UpdateVoteCount();
            }
        }

        private void upvote_Click(object sender, RoutedEventArgs e)
        {
            if (VoteType == "Upvote")
            {
                upvote.Visibility = Visibility.Collapsed;
                no_upvote.Visibility = Visibility.Visible;

                VoteType = "Neutral";
                UpdateVoteType();
                UpdateVoteCount();
            }
        }

        private void downvote_Click(object sender, RoutedEventArgs e)
        {
            if (VoteType == "Downvote")
            {
                downvote.Visibility = Visibility.Collapsed;
                no_downvote.Visibility = Visibility.Visible;

                VoteType = "Neutral";
                UpdateVoteType();
                UpdateVoteCount();
            }
        }
    }
}
