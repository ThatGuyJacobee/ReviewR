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

        private static int GlobalUserID = App.GlobalUserID;

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
                conn.Close();
                return true;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string userid = GlobalUserID.ToString();

            bool NoUsername = UsernameValidation(userid); //Run the DataValidation method

            if (NoUsername)
            {
                //When no username is present, open the dialog to get the user to create one
                ContentDialog usernamedialog = new UsernameDialog();
                await usernamedialog.ShowAsync();
            }
        }
    }
}
