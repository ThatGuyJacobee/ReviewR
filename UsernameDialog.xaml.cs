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
using MySql.Data.MySqlClient;
using System.Diagnostics; //Debug

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    public sealed partial class UsernameDialog : ContentDialog
    {
        //Uses the static Connection String that was set in the Main App Class (private)
        private static string ConnectionString = App.ConnectionString;

        private static int GlobalUserID = App.GlobalUserID;

        public UsernameDialog()
        {
            this.InitializeComponent();
            Debug.WriteLine(GlobalUserID);
        }

        private bool UsernameInsertion(string userid, string username) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                //Selects the user_data table to insert email and password into
                cmd.CommandText = "INSERT INTO user_data (Username) VALUES (@Username) WHERE UserID=@UserID";
                cmd.Parameters.AddWithValue("@UserID", userid); //Sets them as variables
                cmd.Parameters.AddWithValue("@Username", username);

                //If statement performs a server-side (pre-insert) validation to ensure data matches requirements
                if (username.Length <= 15 && (!string.IsNullOrEmpty(username)))
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
            string usernamecheck = new_username_check.Text;
            string userid = GlobalUserID.ToString();

            bool UsernameSuccess = UsernameInsertion(userid, username); //Run the DataValidation method

            if (UsernameSuccess) {
                ContentDialog successdialog = new ContentDialog();
                successdialog.Title = "Success!";
                successdialog.Content = "Your username has been updated!";
                successdialog.CloseButtonText = "Approve";
                successdialog.DefaultButton = ContentDialogButton.Close;

                username_contentdialog.Hide(); //Close the register dialog (limitation 1 at a time)
                await successdialog.ShowAsync(); //Displays it until the close button is pressed
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

            if (Username.Length <= 15)
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
            string UsernameCheck = new_username_check.Text;

            if (UsernameCheck.Length <= 15)
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
