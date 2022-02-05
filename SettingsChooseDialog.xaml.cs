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
using System.Net.Mail; //Used to send emails
using System.ComponentModel.DataAnnotations; //Used to check that email is real
using System.Diagnostics; //Debug

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    public sealed partial class SettingsChooseDialog : ContentDialog
    {
        public SettingsChooseDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void password_change_Click(object sender, RoutedEventArgs e)
        {
            bool EmailExists = CheckEmail();

            if (EmailExists)
            {
                ContentDialog passwordtokenreset = new PasswordTokenDialog();

                settingschoose_contentdialog.Hide(); //Hide the current choice dialog
                await passwordtokenreset.ShowAsync(); //Re-use the password reset class previously made for the login screen
            }

            else
            {
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Validation not passed.\nAn account with the supplied email cannot be found or has been used via Google Account Log-in!";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                settingschoose_contentdialog.Hide();
                await errordialog.ShowAsync();

                //Ensure the reset dialog reappears after accepting error dialog
                ContentDialog returndialog = new SettingsChooseDialog();
                errordialog.CloseButtonCommandParameter = await returndialog.ShowAsync();
            }
        }

        private async void set_avatar_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog setavatar = new AvatarEditDialog();

            settingschoose_contentdialog.Hide(); //Hide the current choice dialog
            await setavatar.ShowAsync(); //Re-use the password reset class previously made for the login screen
        }

        private async void edit_profilebio_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog editbio = new AccountEditDialog();

            settingschoose_contentdialog.Hide(); //Hide the current choice dialog
            await editbio.ShowAsync(); //Re-use the password reset class previously made for the login screen
        }

        private bool CheckEmail()
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT UserID, AuthSub, Email FROM user_data WHERE UserID=@userid"; //Selects the email and password rows from user_data
                cmd.Parameters.AddWithValue("@userid", App.GlobalUserID); //Sets them as variables
                cmd.Connection = conn;

                MySqlDataReader emailread = cmd.ExecuteReader(); //Executes a read command for the table
                if (emailread.Read())
                {
                    var authsub = Convert.ToString(emailread["AuthSub"]); //Checks if the account was made via google account signin

                    if (string.IsNullOrEmpty(authsub)) //If it's not via google account, then class may continue
                    {
                        conn.Close(); //Close connection
                        return true;
                    }

                    else //If it is via google account, then reset password won't apply hence return false for error
                    {
                        conn.Close(); //Close connection
                        return false;
                    }
                }
                else
                {
                    conn.Close(); //Close connection
                    return false;
                }
            }
        }
    }
}
