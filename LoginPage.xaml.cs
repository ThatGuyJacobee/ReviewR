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
using System.Data.SqlClient;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        //Create a global method which can be inherited by any class to create a connection to the database
        private string connectionString = "Data Source=LAPTOP-E5I3QMPO;Initial Catalog=ReviewR;Integrated Security=True";

        public string ConnectionString { get => connectionString; set => connectionString = value; }

        public LoginPage()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (SqlConnection SQLCon = new SqlConnection(ConnectionString))
            {
                //Opens up the Database Connection
                SQLCon.Open();

                if (SQLCon.State.ToString() == "Closed") 
                {
                    db_status_warning.Visibility = Visibility.Visible;
                    database_status.Text = "Error: DB Connection Closed";
                }

                else if (SQLCon.State.ToString() == "Open")
                {
                    db_status_tick.Visibility = Visibility.Visible;
                    database_status.Text = "DB Connection Open";
                }
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

        private void login_next_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void database_status_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
