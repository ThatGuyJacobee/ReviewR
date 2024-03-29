﻿using System;
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
    public sealed partial class RegisterDialog : ContentDialog
    {
        //Uses the static Connection String that was set in the Main App Class (private)
        private static string ConnectionString = App.ConnectionString;

        public RegisterDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //Closes prompt back to Login Screen
            var CloseDialog = register_contentdialog;

            CloseDialog.Hide();
        }

        private bool DataInsertion(string email, string passwordcheck) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                //Selects the user_data table to insert email and password into
                cmd.CommandText = "INSERT INTO user_data (email, password, creationdate) VALUES (@email, @password, @creationdate)";
                cmd.Parameters.AddWithValue("@email", email); //Sets them as variables
                cmd.Parameters.AddWithValue("@password", passwordcheck);
                cmd.Parameters.AddWithValue("@creationdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); //Variable to set the creation date alongside the account.

                var NumberChar = new[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' }; //Array variable for all the numbers
                var SpecialChar = new[] { '!', '#', '$', '%', '&', '(', ')', '*', '+', '-', '_', '.', '/', ':', ';', '<', '>', '=', '?', '[', ']', '~' }; //Array variable of all the special characters
                string password = new_password.Password; //Variable set here as it's not used in the button method

                //If statement performs a server-side (pre-insert) validation to ensure data matches requirements
                if (new EmailAddressAttribute().IsValid(email) && email.Contains("@") && NumberChar.Any(password.Contains) && SpecialChar.Any(password.Contains) && password.Length >= 5 && (password == passwordcheck) && !string.IsNullOrEmpty(passwordcheck))
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
            string email = new_email.Text; //Inputs set as variables
            string passwordcheck = new_password_check.Password;

            bool databaseDuplicate = DatabaseAccountDuplication(email);

            if (!databaseDuplicate) { //If it was true, then a success content dialog is displayed after the register is closed

                try
                {
                    bool dataInserted = DataInsertion(email, passwordcheck);

                    if (dataInserted)
                    {
                        MailMessage mail = new MailMessage();
                        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                        mail.From = new MailAddress("reviewrproject@gmail.com");
                        mail.To.Add(email);
                        mail.Subject = "Account has been successfully registered!";
                        mail.Body = "Hi there,\n\nThank you for registering an account on the ReviewR app.\nThis email has been automatically sent as a confirmation for account creation.\nIf this action wasn't you or this email was wrongfully sent, please ignore this message.\n\nReviewR App";

                        SmtpServer.Port = 587;
                        SmtpServer.Credentials = new System.Net.NetworkCredential("reviewrproject@gmail.com", "GuyJacobee_1");
                        SmtpServer.EnableSsl = true;

                        SmtpServer.Send(mail);
                        Debug.WriteLine("Account registration Email has been successfully sent to: " + email);

                        ContentDialog successdialog = new ContentDialog();
                        successdialog.Title = "Success!";
                        successdialog.Content = "Your account has been successfully created!\nAn email has been sent to the registered email confirming signup.\nPlease login with these credentials.";
                        successdialog.CloseButtonText = "Approve";
                        successdialog.DefaultButton = ContentDialogButton.Close;

                        register_contentdialog.Hide(); //Close the register dialog (limitation 1 at a time)
                        await successdialog.ShowAsync(); //Displays it until the close button is pressed
                    }

                    else
                    {
                        Debug.WriteLine("Data insertion was unsuccessful. Validation failed");
                        ContentDialog errordialog = new ContentDialog();
                        errordialog.Title = "Error!";
                        errordialog.Content = "Validation not passed.\nMake sure all inputs are approved.";
                        errordialog.CloseButtonText = "Approve";
                        errordialog.DefaultButton = ContentDialogButton.Close;

                        register_contentdialog.Hide();
                        await errordialog.ShowAsync();
                    }
                   
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Data insertion was unsuccessful.");
                    Debug.WriteLine(ex.ToString());
                }
            }

            else { //Otherwise the error content dialog is displayed after register is forcefully closed
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Account already exists.\nAn account with the supplied email address either exists as a regular account, or has been used via Google Sign-in.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                register_contentdialog.Hide();
                await errordialog.ShowAsync();
            }
        }

        private bool DatabaseAccountDuplication(string email) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT UserID, Username, Email, Password FROM user_data WHERE Email=@email"; //Selects the email and password rows from user_data
                cmd.Parameters.AddWithValue("@email", email); //Sets them as variables
                cmd.Connection = conn;

                MySqlDataReader login = cmd.ExecuteReader(); //Executes a read command for the table
                if (login.Read())
                {
                    conn.Close(); //Close connection
                    return true;
                }
                else
                {
                    conn.Close(); //Close connection
                    return false;
                }
            }
        }

        private void new_email_TextChanged(object sender, TextChangedEventArgs e) //Method which runs every time the new_email input text changes
        {
            string EmailBoxText = new_email.Text;
            string TextBoxText = new_password.Password;

            if (new EmailAddressAttribute().IsValid(EmailBoxText) && EmailBoxText.Contains("@")) { //Checks if email input contains an @ and if it does, display tick image and collapse others
                new_email_neutralimg.Visibility = Visibility.Collapsed;
                new_email_warningimg.Visibility = Visibility.Collapsed;
                new_email_tickimg.Visibility = Visibility.Visible;
            }

            else if (string.IsNullOrEmpty(EmailBoxText)) { //If the input field is empty, display the neutral image and collapse others
                new_email_warningimg.Visibility = Visibility.Collapsed;
                new_email_tickimg.Visibility = Visibility.Collapsed;
                new_email_neutralimg.Visibility = Visibility.Visible;
            }

            else { //Otherwise if input field doesn't match, display warning image and collapse others
                new_email_neutralimg.Visibility = Visibility.Collapsed;
                new_email_tickimg.Visibility = Visibility.Collapsed;
                new_email_warningimg.Visibility = Visibility.Visible;
            }
        }

        private void new_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //Password Requirement Checks
            var NumberChar = new[] {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0'}; //Array variable for all the numbers
            var SpecialChar = new[] { '!', '#', '$', '%', '&', '(', ')', '*', '+', '-', '_', '.', '/', ':', ';', '<', '>', '=', '?', '[', ']', '~' }; //Array variable of all the special characters

            string TextBoxText = new_password.Password;

            string PasswordCheck = new_password_check.Password;
            new_password_check_warningimg.Visibility = Visibility.Collapsed;
            new_password_check_tickimg.Visibility = Visibility.Collapsed;
            new_password_check_neutralimg.Visibility = Visibility.Visible;
            new_password_check.Password = "";

            //If the Password Requirements are met, show the tick icon whilst if it's not met, show warning icon
            if (NumberChar.Any(TextBoxText.Contains) && SpecialChar.Any(TextBoxText.Contains) && TextBoxText.Length >= 5) { //If password matches all the three criteria, display tick and collapse others
                new_password_neutralimg.Visibility = Visibility.Collapsed;
                new_password_warningimg.Visibility = Visibility.Collapsed;
                new_password_tickimg.Visibility = Visibility.Visible;
            }

            else if (string.IsNullOrEmpty(TextBoxText)) { //If the input field is empty, display the neutral image and collapse others
                new_password_warningimg.Visibility = Visibility.Collapsed;
                new_password_tickimg.Visibility = Visibility.Collapsed;
                new_password_neutralimg.Visibility = Visibility.Visible;
            }

            else { //Otherwise if password field doesn't match either requirment, then display warning image and collapse others
                new_password_neutralimg.Visibility = Visibility.Collapsed;
                new_password_tickimg.Visibility = Visibility.Collapsed;
                new_password_warningimg.Visibility = Visibility.Visible;
            }
        }

        private void new_password_check_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string Password = new_password.Password;
            string PasswordCheck = new_password_check.Password;

            if ((Password == PasswordCheck) && (!string.IsNullOrEmpty(PasswordCheck))) {
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

            else {
                new_password_check_neutralimg.Visibility = Visibility.Collapsed;
                new_password_check_tickimg.Visibility = Visibility.Collapsed;
                new_password_check_warningimg.Visibility = Visibility.Visible;
            }
        }
    }
}
