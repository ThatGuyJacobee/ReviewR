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
using System.Diagnostics; //Debug
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminPanel : Page
    {
        public AdminPanel()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            username_text.Text = "Welcome to the admin panel " + App.GlobalUsername + "!";

            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = "SELECT COUNT(UserID) FROM user_data"; //Counts amount of occurances
                    cmd.Connection = conn;

                    Int32 count = Convert.ToInt32(cmd.ExecuteScalar());
                    user_count.Text = count.ToString();
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = "SELECT COUNT(ReviewID) FROM review_data"; //Counts amount of occurances
                    cmd.Connection = conn;

                    Int32 count = Convert.ToInt32(cmd.ExecuteScalar());
                    review_count.Text = count.ToString();
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }
}
