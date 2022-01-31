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
    public sealed partial class PasswordChangeDialog : ContentDialog
    {
        public PasswordChangeDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //Closes prompt back to Login Screen
            var CloseDialog = passwordchange_contentdialog;

            CloseDialog.Hide();
        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string password = new_password.Password; //Inputs set as variables
            string passwordcheck = new_password_check.Password;
            var email = PasswordResetDialog.ResetEmail;

            if (password == passwordcheck)
            {
                bool DBUpdateSuccess = UpdatePassDB(password);

                if (DBUpdateSuccess)
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                    mail.From = new MailAddress("reviewrproject@gmail.com");
                    mail.To.Add(email);
                    mail.Subject = "Password successfully changed!";
                    mail.Body = "Hi there,\n\nThis is an automated email letting you know that your account's password has been changed within the ReviewR app.\nThis email has been automatically sent as a confirmation for password change.\nIf this action wasn't you or this email was wrongfully sent, please create a support ticket to report this.\n\nReviewR App";

                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("reviewrproject@gmail.com", "GuyJacobee_1");
                    SmtpServer.EnableSsl = true;

                    SmtpServer.Send(mail);
                    Debug.WriteLine("Password change email has been successfully sent to: " + email);

                    ContentDialog successdialog = new ContentDialog();
                    successdialog.Title = "Success!";
                    successdialog.Content = "Your password has been changed and an email has been sent. You may now return to the login page to login.";
                    successdialog.CloseButtonText = "Next";
                    successdialog.DefaultButton = ContentDialogButton.Close;

                    passwordchange_contentdialog.Hide(); //Close the register dialog (limitation 1 at a time)
                    await successdialog.ShowAsync(); //Displays it until the close button is pressed
                }

                else
                {
                    ContentDialog errordialog = new ContentDialog();
                    errordialog.Title = "Error!";
                    errordialog.Content = "There was an error updating the password. This was most likely an app error, try again. If the issue persists please create a bug report.\n\n[Error Code: DB_NOT_UPDATED]";
                    errordialog.CloseButtonText = "Approve";
                    errordialog.DefaultButton = ContentDialogButton.Close;

                    passwordchange_contentdialog.Hide();
                    await errordialog.ShowAsync();

                    //Ensure the reset dialog reappears after accepting error dialog
                    errordialog.CloseButtonCommandParameter = await passwordchange_contentdialog.ShowAsync();
                }
            }
            
            else
            {
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Validation not passed!\nThe two input passwords aren't correct, try again.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                passwordchange_contentdialog.Hide();
                await errordialog.ShowAsync();

                //Ensure the reset dialog reappears after accepting error dialog
                errordialog.CloseButtonCommandParameter = await passwordchange_contentdialog.ShowAsync();
            }
        }

        private bool UpdatePassDB(string password)
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Selects the user_data table to insert email and password into
                    cmd.CommandText = "UPDATE user_data SET Password=@password WHERE Email=@email";
                    cmd.Parameters.AddWithValue("@password", password); //Sets them as variables
                    cmd.Parameters.AddWithValue("@email", PasswordResetDialog.ResetEmail);

                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
            }
        }

        private void new_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //Password Requirement Checks
            var NumberChar = new[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' }; //Array variable for all the numbers
            var SpecialChar = new[] { '!', '#', '$', '%', '&', '(', ')', '*', '+', '-', '_', '.', '/', ':', ';', '<', '>', '=', '?', '[', ']', '~' }; //Array variable of all the special characters

            string TextBoxText = new_password.Password;

            string PasswordCheck = new_password_check.Password;
            new_password_check_warningimg.Visibility = Visibility.Collapsed;
            new_password_check_tickimg.Visibility = Visibility.Collapsed;
            new_password_check_neutralimg.Visibility = Visibility.Visible;
            new_password_check.Password = "";

            //If the Password Requirements are met, show the tick icon whilst if it's not met, show warning icon
            if (NumberChar.Any(TextBoxText.Contains) && SpecialChar.Any(TextBoxText.Contains) && TextBoxText.Length >= 5)
            { //If password matches all the three criteria, display tick and collapse others
                new_password_neutralimg.Visibility = Visibility.Collapsed;
                new_password_warningimg.Visibility = Visibility.Collapsed;
                new_password_tickimg.Visibility = Visibility.Visible;
            }

            else if (string.IsNullOrEmpty(TextBoxText))
            { //If the input field is empty, display the neutral image and collapse others
                new_password_warningimg.Visibility = Visibility.Collapsed;
                new_password_tickimg.Visibility = Visibility.Collapsed;
                new_password_neutralimg.Visibility = Visibility.Visible;
            }

            else
            { //Otherwise if password field doesn't match either requirment, then display warning image and collapse others
                new_password_neutralimg.Visibility = Visibility.Collapsed;
                new_password_tickimg.Visibility = Visibility.Collapsed;
                new_password_warningimg.Visibility = Visibility.Visible;
            }
        }

        private void new_password_check_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string Password = new_password.Password;
            string PasswordCheck = new_password_check.Password;

            if ((Password == PasswordCheck) && (!string.IsNullOrEmpty(PasswordCheck)))
            {
                new_password_check_neutralimg.Visibility = Visibility.Collapsed;
                new_password_check_warningimg.Visibility = Visibility.Collapsed;
                new_password_check_tickimg.Visibility = Visibility.Visible;
            }

            else if (string.IsNullOrEmpty(PasswordCheck))
            {
                new_password_check_warningimg.Visibility = Visibility.Collapsed;
                new_password_check_tickimg.Visibility = Visibility.Collapsed;
                new_password_check_neutralimg.Visibility = Visibility.Visible;
            }

            else
            {
                new_password_check_neutralimg.Visibility = Visibility.Collapsed;
                new_password_check_tickimg.Visibility = Visibility.Collapsed;
                new_password_check_warningimg.Visibility = Visibility.Visible;
            }
        }
    }
}
