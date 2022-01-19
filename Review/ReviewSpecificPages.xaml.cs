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
        }

        private bool DataDeletion()
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = "DELETE FROM review_data WHERE ReviewID=@reviewid"; //Selects the email and password rows from user_data
                    cmd.Parameters.AddWithValue("@reviewid", ReviewSystem.ReviewSpecificID); //Sets them as variables
                    cmd.Connection = conn;

                    cmd.ExecuteNonQuery();
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
            //checkdialog.PrimaryButtonText = "Cancel";
            checkdialog.CloseButtonText = "Approve";
            checkdialog.DefaultButton = ContentDialogButton.Close;

            await checkdialog.ShowAsync(); //Displays it until the close button is pressed

            //Added a parameter which runs on close button close to restart the app
            bool datadeletion = DataDeletion();
            checkdialog.CloseButtonCommandParameter = datadeletion;

            if (datadeletion)
            {
                checkdialog.Hide();

                ContentDialog successdialog = new ContentDialog();
                successdialog.Title = "Success!";
                successdialog.Content = "The review with the ID " + ReviewSystem.ReviewSpecificID + " has been deleted!";
                checkdialog.PrimaryButtonText = "Cancel";
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
    }
}
