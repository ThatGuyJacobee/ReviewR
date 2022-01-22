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
    public sealed partial class PasswordTokenDialog : ContentDialog
    {
        public PasswordTokenDialog()
        {
            this.InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var email = PasswordResetDialog.ResetEmail;

            //Generate a random 8-digit number as the token
            Random rnd = new Random();
            int Token = rnd.Next(10000000, 99999999);
            Debug.WriteLine("Generated reset token: " + Token);

            bool EmailSuccess = SendEmailToken(Token);

            if (EmailSuccess)
            {
                Debug.WriteLine("Email sent successfully.");
                bool DBSuccess = InsertTokenDB(Token);

                if (DBSuccess)
                {
                    Debug.WriteLine("Successfully inserted token into database.");
                }

                else
                {
                    Debug.WriteLine("Error inserting token into database.");
                }
            }

            else
            {
                Debug.WriteLine("Email wasn't sent successfully.");
            }
        }

        private bool SendEmailToken(int Token)
        {
            var email = PasswordResetDialog.ResetEmail;
            //Attempt to send an email with the reset token
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("reviewrproject@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Reset password request!";
                mail.Body = "Hi there,\n\nYou have requested to reset your password through the ReviewR app.\nThis email has been automatically sent as confirmation and contains your password reset token.\n\nYour reset password token is: " + Token + "\n\nPlease enter this 8-digit number on the reset password dialog page. If this action wasn't you or this email was wrongfully sent, please ignore this message.\n\nReviewR App";

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("reviewrproject@gmail.com", "GuyJacobee_1");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                Debug.WriteLine("Reset password email has been sent to: " + email);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private bool InsertTokenDB(int Token)
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Selects the user_data table to insert email and password into
                    cmd.CommandText = "UPDATE user_data SET PassReset=@passreset WHERE Email=@email";
                    cmd.Parameters.AddWithValue("@passreset", Token); //Sets them as variables
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

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //Closes prompt back to Login Screen
            var CloseDialog = passwordtoken_contentdialog;

            CloseDialog.Hide();
        }

        private static string ResetToken = "";

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var InputToken = enter_token.Text;

            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Selects the user_data table to insert email and password into
                    cmd.CommandText = "SELECT PassReset FROM user_data WHERE Email=@email";
                    cmd.Parameters.AddWithValue("@email", PasswordResetDialog.ResetEmail);
                    cmd.Connection = conn;

                    MySqlDataReader tokenread = cmd.ExecuteReader(); //Executes a read command for the table
                    if (tokenread.Read())
                    {
                        ResetToken = Convert.ToString(tokenread["PassReset"]);
                        conn.Close(); //Close connection
                    }
                    else
                    {
                        conn.Close(); //Close connection
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

            if (InputToken == ResetToken)
            {
                ContentDialog successdialog = new ContentDialog();
                successdialog.Title = "Success!";
                successdialog.Content = "The token is correct. You will now be prompted to change your password.";
                successdialog.CloseButtonText = "Next";
                successdialog.DefaultButton = ContentDialogButton.Close;

                passwordtoken_contentdialog.Hide(); //Close the register dialog (limitation 1 at a time)
                await successdialog.ShowAsync(); //Displays it until the close button is pressed

                ContentDialog passwordreset = new PasswordChangeDialog();
                successdialog.CloseButtonCommandParameter = await passwordreset.ShowAsync();
            }

            else
            {
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Token isn't correct!\nPlease double check that the token is correct, and use the latest token received to your email.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                passwordtoken_contentdialog.Hide();
                await errordialog.ShowAsync();

                //Ensure the reset dialog reappears after accepting error dialog
                ContentDialog passwordtokendialog = new PasswordTokenDialog();
                errordialog.CloseButtonCommandParameter = await passwordtokendialog.ShowAsync();
            }
        }
    }
}
