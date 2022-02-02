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
using Windows.Web.Http; //For POST method
using System.Threading.Tasks; //For POST method
using System.Collections.ObjectModel; //Used to notify listview values when objects are changed
using System.Diagnostics; //Debug
using System.Text.Json; //Used for (de)serizalisation and JSON manipulation
using System.Text.Json.Serialization; //Used as serialization library

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    public sealed partial class AccountEditDialog : ContentDialog
    {
        public AccountEditDialog()
        {
            this.InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT username, userbio FROM user_data WHERE UserID=@UserID"; //Selects the UserID to search and Username to retrieve
                cmd.Parameters.AddWithValue("@UserID", App.GlobalUserID); //Sets them as variables
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();

                MySqlDataReader updateelements = cmd.ExecuteReader();
                if (updateelements.Read())
                {
                    edit_username.Text = Convert.ToString(updateelements["username"]);
                    edit_bio.Text = Convert.ToString(updateelements["userbio"]);
                    conn.Close();
                }
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //Added a parameter which runs on close button close to return to original edit dialog
            ContentDialog returnchoose = new SettingsChooseDialog();
            accountedit_contentdialog.Hide();
            await returnchoose.ShowAsync();
        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string Username = edit_username.Text;
            string Bio = edit_bio.Text;

            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Bio) && Bio.Length <= 2048 && Username.Length <= 15)
            {
                bool accedit = UpdateReview(Username, Bio);

                if (accedit)
                {
                    ContentDialog successdialog = new ContentDialog();
                    successdialog.Title = "Success!";
                    successdialog.Content = "Your account details have been edited successfully!";
                    successdialog.CloseButtonText = "Approve";
                    successdialog.DefaultButton = ContentDialogButton.Close;

                    accountedit_contentdialog.Hide();
                    await successdialog.ShowAsync(); //Displays it until the close button is pressed

                    //Added a parameter which runs on close button close to return to original edit dialog
                    ContentDialog returnchoose = new SettingsChooseDialog();
                    returnchoose.CloseButtonCommandParameter = await returnchoose.ShowAsync();
                }

                else
                {
                    //Display an content dialog which states the error - blackbox testing
                    ContentDialog errordialog = new ContentDialog();
                    errordialog.Title = "Error!";
                    errordialog.Content = "Database update was unsuccessful. This may be an issue on the program side, please try again. If you encounter the error constantly create a bug report.\n\n[Error Code: UPDATE_FAILURE_DB]";
                    errordialog.CloseButtonText = "Approve";
                    errordialog.DefaultButton = ContentDialogButton.Close;

                    accountedit_contentdialog.Hide();
                    await errordialog.ShowAsync(); //Display it until user presses the accept button
                    errordialog.CloseButtonCommandParameter = await accountedit_contentdialog.ShowAsync();
                }
            }

            else
            {
                //Display an content dialog which states the error - blackbox testing
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Validation not passed.\nMake sure your bio is below 2048 characters and username is below 15 characters.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                accountedit_contentdialog.Hide();
                await errordialog.ShowAsync(); //Display it until user presses the accept button
                errordialog.CloseButtonCommandParameter = await accountedit_contentdialog.ShowAsync();
            }
        }

        private bool UpdateReview(string Username, string Bio)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Sets variables and SQL command
                    cmd.CommandText = "UPDATE user_data SET Username=@username, UserBio=@userbio WHERE UserID=@userid";
                    cmd.Parameters.AddWithValue("@username", Username);
                    cmd.Parameters.AddWithValue("@userbio", Bio);
                    cmd.Parameters.AddWithValue("@userid", App.GlobalUserID);

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

        private void edit_username_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void edit_bio_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
