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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReviewEdit : Page
    {
        public ReviewEdit()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            usernamereview_text.Text = "Review ID: " + ReviewSystem.ReviewSpecificID;
            reviewedgame_text.Text = ReviewSystem.ReviewSpecificGameName;

            reviewedit_title.Text = ReviewSystem.ReviewSpecificGameTitle;
            reviewedit_description.Text = ReviewSystem.ReviewSpecificDescription;
        }

        private bool UpdateReview(string Title, string Description)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Sets variables and SQL command
                    cmd.CommandText = "UPDATE review_data SET RevTitle=@revtitle, RevDesc=@revdesc WHERE ReviewID=@reviewid";
                    cmd.Parameters.AddWithValue("@revtitle", Title);
                    cmd.Parameters.AddWithValue("@revdesc", Description);
                    cmd.Parameters.AddWithValue("@reviewid", ReviewSystem.ReviewSpecificID);

                    cmd.ExecuteNonQuery();
                    conn.Close();

                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        private async void continue_button_Click(object sender, RoutedEventArgs e)
        {
            string Title = reviewedit_title.Text;
            string Description = reviewedit_description.Text;

            if (!string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Description) && Description.Length <= 2048)
            {
                bool reviewedit = UpdateReview(Title, Description);

                if (reviewedit)
                {
                    ContentDialog successdialog = new ContentDialog();
                    successdialog.Title = "Success!";
                    successdialog.Content = "The Review has been edited! The app will return you to the review landing page on approval.";
                    successdialog.CloseButtonText = "Approve";
                    successdialog.DefaultButton = ContentDialogButton.Close;

                    await successdialog.ShowAsync(); //Displays it until the close button is pressed

                    //Added a parameter which runs on close button close to restart the app
                    successdialog.CloseButtonCommandParameter = this.Frame.Navigate(typeof(ReviewSystem), null);
                }

                else
                {
                    //Display an content dialog which states the error - blackbox testing
                    ContentDialog errordialog = new ContentDialog();
                    errordialog.Title = "Error!";
                    errordialog.Content = "Database update was unsuccessful. This may be an issue on the program side, please try again. If you encounter the error constantly create a bug report.\n\n[Error Code: UPDATE_FAILURE_DB]";
                    errordialog.CloseButtonText = "Approve";
                    errordialog.DefaultButton = ContentDialogButton.Close;

                    await errordialog.ShowAsync(); //Display it until user presses the accept button
                }
            }

            else
            {
                //Display an content dialog which states the error - blackbox testing
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Validation not passed.\nMake sure NOT to include any inapprporiate words and keep the description below 2048 characters.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                await errordialog.ShowAsync(); //Display it until user presses the accept button
            }
        }
    }
}
