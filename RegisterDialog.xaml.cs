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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    public sealed partial class RegisterDialog : ContentDialog
    {
        //Create a global method which can be inherited by any class to create a connection to the database
        private string connectionString = "server=127.0.0.1;database=reviewr;uid=root;pwd=;SSL-mode=none;";

        public string ConnectionString { get => connectionString; set => connectionString = value; }

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

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            using (MySqlConnection MySQLCon = new MySqlConnection(ConnectionString))
            using (MySqlCommand cmd = new MySqlCommand("INSERT INTO user_data (email, password)" +
                "WHERE Email=@email AND Password=@pass;", MySQLCon))
            {
                //Submits the given Register details into the database for storage
                string email = new_email.Text;
                string pass = new_password.Password;
                //Closes Register Dialog and displays an Successfully Registered Dialog
            }
        }

        private void new_email_TextChanged(object sender, TextChangedEventArgs e)
        {
            string EmailBoxText = new_email.Text;
            
            if (EmailBoxText.Contains("@")) {
                new_email_neutralimg.Visibility = Visibility.Collapsed;
                new_email_warningimg.Visibility = Visibility.Collapsed;
                new_email_tickimg.Visibility = Visibility.Visible;
            }

            else if (string.IsNullOrEmpty(EmailBoxText)) {
                new_email_warningimg.Visibility = Visibility.Collapsed;
                new_email_tickimg.Visibility = Visibility.Collapsed;
                new_email_neutralimg.Visibility = Visibility.Visible;
            }

            else {
                new_email_neutralimg.Visibility = Visibility.Collapsed;
                new_email_tickimg.Visibility = Visibility.Collapsed;
                new_email_warningimg.Visibility = Visibility.Visible;
            }
        }

        private void new_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //Password Requirement Checks
            var NumberChar = new[] {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0'};
            var SpecialChar = new[] { '!', '#', '$', '%', '&', '(', ')', '*', '+', '-', '_', '.', '/', ':', ';', '<', '>', '=', '?', '[', ']', '~' };

            string TextBoxText = new_password.Password;

            //If the Password Requirements are met, show the tick icon whilst if it's not met, show warning icon
            if (NumberChar.Any(TextBoxText.Contains) && (SpecialChar.Any(TextBoxText.Contains) && (TextBoxText.Length >= 5))) {
                new_password_neutralimg.Visibility = Visibility.Collapsed;
                new_password_warningimg.Visibility = Visibility.Collapsed;
                new_password_tickimg.Visibility = Visibility.Visible;
            }

            else if (string.IsNullOrEmpty(TextBoxText)) {
                new_password_warningimg.Visibility = Visibility.Collapsed;
                new_password_tickimg.Visibility = Visibility.Collapsed;
                new_password_neutralimg.Visibility = Visibility.Visible;
            }

            else {
                new_password_neutralimg.Visibility = Visibility.Collapsed;
                new_password_tickimg.Visibility = Visibility.Collapsed;
                new_password_warningimg.Visibility = Visibility.Visible;
            }
        }

        private void new_password_check_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var Password = new_password.Password;
            var PasswordCheck = new_password_check.Password;

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
