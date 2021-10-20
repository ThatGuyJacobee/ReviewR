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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenu : Page
    {
        //Uses the static Connection String that was set in the Main App Class (private)
        private static string ConnectionString = App.ConnectionString;

        public MainMenu()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }

        private bool UsernameValidation(string userid) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT UserID, Username FROM user_data WHERE UserID=@UserID AND Username IS NULL"; //Selects the UserID to search and Username to retrieve
                cmd.Parameters.AddWithValue("@UserID", userid); //Sets them as variables
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();

                MySqlDataReader usernameval = cmd.ExecuteReader();
                if (usernameval.Read())
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            welcomeback_text.Text = "Welcome back, " + App.GlobalUsername + "!";

            Debug.WriteLine("Main Menu UserID:0"); //Temporary debugging

            //Checks if Username Dialog should occur
            string userid = App.GlobalUserID.ToString(); //Converts int to a string

            bool NoUsername = UsernameValidation(userid); //Run the UsernameValidation method

            if (NoUsername)
            {
                //When no username is present, open the dialog to get the user to create one
                ContentDialog usernamedialog = new UsernameDialog();
                await usernamedialog.ShowAsync();
            }
            else
            {
                return;
            }
        }

        private void ProgressBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }
    }
}
