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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        //Uses the static Connection String that was set in the Main App Class (private)
        private static string ConnectionString = App.ConnectionString;

        public LoginPage()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection MySQLCon = new MySqlConnection(ConnectionString))
            {
                try
                {
                   //Opens up the Database Connection
                   MySQLCon.Open();
                }

                catch //Catches the error if it fails to connect.
                {
                    db_status_warning.Visibility = Visibility.Visible;
                    database_status.Text = "Error: DB Connection Closed";
                }

                if (MySQLCon.State.ToString() == "Open") //If the connection status is open, then display success.
                {
                    db_status_tick.Visibility = Visibility.Visible;
                    database_status.Text = "DB Connection Open";
                }
                //Close connection after check is complete.
                MySQLCon.Close();
            }
        }

        private async void register_account_Click(object sender, RoutedEventArgs e)
        {
            //When the Register Account button is clicked, display the Register Content Dialog
            ContentDialog registerdialog = new RegisterDialog();
            await registerdialog.ShowAsync();
        }

        private void email_entry_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void password_entry_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }

        private bool DataValidation(string email, string pass) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT UserID, Username, Email, Password FROM user_data WHERE Email=@email AND Password=@pass"; //Selects the email and password rows from user_data
                cmd.Parameters.AddWithValue("@email", email); //Sets them as variables
                cmd.Parameters.AddWithValue("@pass", pass);
                cmd.Connection = conn;

                MySqlDataReader login = cmd.ExecuteReader(); //Executes a read command for the table
                if (login.Read())
                {
                    Debug.WriteLine("Pre-login (Default) UserID:" + App.GlobalUserID); //Temporarily debugging
                    App.GlobalUserID = Convert.ToInt32(login["UserID"]); //Converts into integer and sets the selected UserID as the global one for the session
                    App.GlobalUsername = Convert.ToString(login["Username"]); //Converts into string and sets the appropirate Username as the global variable for the session
                    conn.Close(); //Close connection
                    Debug.WriteLine("Post-login UserID:" + App.GlobalUserID); //Temporary debugging
                    return true;
                }
                else
                {
                    conn.Close(); //Close connection
                    return false;
                }
            }
        }

        private void login_next_Click(object sender, RoutedEventArgs e) //Method ran on login button press
        {
            string email = email_entry.Text; //Inputs set as variables
            string pass = password_entry.Password;

            if (email == "" || pass == "") //If either are empty display an error
            {
                login_status.Visibility = Visibility.Visible;
                login_status.Text = "Error: Details cannot be empty";
                return;
            }

            bool loginSuccessful = DataValidation(email, pass); //Run the DataValidation method

            if (loginSuccessful)
            {
                this.Frame.Navigate(typeof(NavigationBar), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()); //If input compares identically to database, proceed to main menu
            }
            else
            {
                login_status.Visibility = Visibility.Visible; //Otherwise display an error
                login_status.Text = "Error: Incorrect details";
            }
        }

        private void database_status_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
