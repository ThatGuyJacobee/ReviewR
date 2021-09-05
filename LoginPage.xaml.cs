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
//using System.Data.SqlClient;
using MySql.Data.MySqlClient;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        //Create a global method which can be inherited by any class to create a connection to the database
        private string connectionString = "server=127.0.0.1;database=reviewr;uid=root;pwd=;SSL-mode=none;";

        public string ConnectionString { get => connectionString; set => connectionString = value; }

        public LoginPage()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded
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

        private bool DataValidation(string email, string pass)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            using (MySqlCommand cmd = new MySqlCommand("SELECT " +
                "Email, Password " +
                "FROM user_data " +
                "WHERE Email=@email AND Password=@pass;", conn))
            {

                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@pass", pass);
                cmd.Connection = conn;
                cmd.Connection.Open();

                MySqlDataReader login = cmd.ExecuteReader();
                if (login.Read())
                {
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

        private void login_next_Click(object sender, RoutedEventArgs e)
        {
            string email = email_entry.Text;
            string pass = password_entry.Password;

            if (email == "" || pass == "")
            {
                login_status.Text = "Login cannot be empty!";
                return;
            }

            bool loginSuccessful = DataValidation(email, pass);

            if (loginSuccessful)
            {
                this.Frame.Navigate(typeof(MainMenu), null);
            }
            else
            {
                login_status.Text = "Login failed.";
            }
        }

        private void database_status_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
