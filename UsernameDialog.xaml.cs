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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    public sealed partial class UsernameDialog : ContentDialog
    {
        //Uses the static Connection String that was set in the Main App Class (private)
        private static string ConnectionString = App.ConnectionString;

        public UsernameDialog()
        {
            this.InitializeComponent();
            Debug.WriteLine("Username Dialog UserID:" + App.GlobalUserID);
        }

        private bool UsernameInsertion(string userid, string username) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                //Selects the user_data table to insert email and password into
                cmd.CommandText = "UPDATE user_data SET Username=@Username WHERE UserID=@UserID";
                cmd.Parameters.AddWithValue("@UserID", userid); //Sets them as variables
                cmd.Parameters.AddWithValue("@Username", username);

                string usernamecheck = new_username_check.Text;

                //If statement performs a server-side (pre-insert) validation to ensure data matches requirements
                if (username.Length <= 15 && (!string.IsNullOrEmpty(username)) && (username == usernamecheck))
                {
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                else
                {
                    conn.Close();
                    return false;
                }
            }
        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string username = new_username.Text;
            string userid = App.GlobalUserID.ToString();

            bool UsernameSuccess = UsernameInsertion(userid, username); //Run the UsernameInsertion method

            if (UsernameSuccess) {
                ContentDialog successdialog = new ContentDialog();
                successdialog.Title = "Success!";
                successdialog.Content = "Your username has been updated! The app will restart and you will have to relog after approving this message for security purposes.";
                successdialog.CloseButtonText = "Approve";
                successdialog.DefaultButton = ContentDialogButton.Close;

                username_contentdialog.Hide(); //Close the register dialog (limitation 1 at a time)
                await successdialog.ShowAsync(); //Displays it until the close button is pressed

                //Added a parameter which runs on close button close to restart the app
                successdialog.CloseButtonCommandParameter = await CoreApplication.RequestRestartAsync("App restarted to update username.");

            }
            else {
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Validation not passed.\nMake both inputs are the same and smaller than 15 characters.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                username_contentdialog.Hide();
                await errordialog.ShowAsync();
            }
        }

        private void new_username_TextChanged(object sender, TextChangedEventArgs e)
        {
            string Username = new_username.Text;

            if (Username.Length <= 15 && (!string.IsNullOrEmpty(Username)))
            {
                username_neutralimg.Visibility = Visibility.Collapsed;
                username_warningimg.Visibility = Visibility.Collapsed;
                username_tickimg.Visibility = Visibility.Visible;
            }

            else if (string.IsNullOrEmpty(Username))
            {
                username_warningimg.Visibility = Visibility.Collapsed;
                username_tickimg.Visibility = Visibility.Collapsed;
                username_neutralimg.Visibility = Visibility.Visible;
            }

            else
            {
                username_neutralimg.Visibility = Visibility.Collapsed;
                username_tickimg.Visibility = Visibility.Collapsed;
                username_warningimg.Visibility = Visibility.Visible;
            }
        }

        private void new_username_check_TextChanged(object sender, TextChangedEventArgs e)
        {
            string Username = new_username.Text;
            string UsernameCheck = new_username_check.Text;

            if ((Username == UsernameCheck) && (!string.IsNullOrEmpty(UsernameCheck)) && UsernameCheck.Length <= 15)
            {
                new_username_neutralimg.Visibility = Visibility.Collapsed;
                new_username_warningimg.Visibility = Visibility.Collapsed;
                new_username_tickimg.Visibility = Visibility.Visible;
            }

            else if (string.IsNullOrEmpty(UsernameCheck))
            {
                new_username_warningimg.Visibility = Visibility.Collapsed;
                new_username_tickimg.Visibility = Visibility.Collapsed;
                new_username_neutralimg.Visibility = Visibility.Visible;
            }

            else
            {
                new_username_neutralimg.Visibility = Visibility.Collapsed;
                new_username_tickimg.Visibility = Visibility.Collapsed;
                new_username_warningimg.Visibility = Visibility.Visible;
            }
        }
    }
}
