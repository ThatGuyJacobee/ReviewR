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
using System.Net.Mail; //Used to send emails
using System.ComponentModel.DataAnnotations; //Used to check that email is real
using System.Diagnostics; //Debug

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    public sealed partial class PasswordResetDialog : ContentDialog
    {
        public PasswordResetDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //Closes prompt back to Login Screen
            var CloseDialog = passwordreset_contentdialog;

            CloseDialog.Hide();
        }

        public static string ResetEmail = "";

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(reset_email.Text) || !new EmailAddressAttribute().IsValid(reset_email.Text) || !reset_email.Text.Contains("@"))
            {
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Validation not passed.\nMake sure the email is valid.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                passwordreset_contentdialog.Hide();
                await errordialog.ShowAsync();

                //Ensure the reset dialog reappears after accepting error dialog
                ContentDialog passwordresetdialog = new PasswordResetDialog();
                errordialog.CloseButtonCommandParameter = await passwordresetdialog.ShowAsync();
            }

            else
            {
                bool EmailExists = CheckEmail();

                if (EmailExists)
                {
                    ResetEmail = reset_email.Text;

                    ContentDialog successdialog = new ContentDialog();
                    successdialog.Title = "Success!";
                    successdialog.Content = "A password reset token will be sent to the supplied email! You will now be prompted to enter the token to ensure identity.";
                    successdialog.CloseButtonText = "Next";
                    successdialog.DefaultButton = ContentDialogButton.Close;

                    passwordreset_contentdialog.Hide(); //Close the register dialog (limitation 1 at a time)
                    await successdialog.ShowAsync(); //Displays it until the close button is pressed

                    ContentDialog passwordtokenreset = new PasswordTokenDialog();
                    successdialog.CloseButtonCommandParameter = await passwordtokenreset.ShowAsync();
                }

                else
                {
                    ContentDialog errordialog = new ContentDialog();
                    errordialog.Title = "Error!";
                    errordialog.Content = "Validation not passed.\nAn account with the supplied email cannot be found or has been used via Google Account Log-in!";
                    errordialog.CloseButtonText = "Approve";
                    errordialog.DefaultButton = ContentDialogButton.Close;

                    passwordreset_contentdialog.Hide();
                    await errordialog.ShowAsync();

                    //Ensure the reset dialog reappears after accepting error dialog
                    ContentDialog passwordresetdialog = new UsernameDialog();
                    errordialog.CloseButtonCommandParameter = await passwordresetdialog.ShowAsync();
                }
            }
        }

        private bool CheckEmail()
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT UserID, AuthSub, Email FROM user_data WHERE Email=@email"; //Selects the email and password rows from user_data
                cmd.Parameters.AddWithValue("@email", reset_email.Text); //Sets them as variables
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
