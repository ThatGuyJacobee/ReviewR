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
using MySqlConnector;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel; //Used to notify listview values when objects are changed
using System.Diagnostics; //Debug

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RecommendationSystem : Page
    {
        public RecommendationSystem()
        {
            this.InitializeComponent();
            //Waits till page is fully loaded before running the event
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                //Sets variables and SQL command
                cmd.CommandText = "SELECT RecID FROM recommend_data WHERE UserID=@UserID"; //Check whether the user has previously used rec system
                cmd.Parameters.AddWithValue("@UserID", App.GlobalUserID);

                MySqlDataReader details = cmd.ExecuteReader();

                if (!details.Read())
                {
                    //Navigate to the Calibration page if a ReviewID doesn't already exist
                    this.Frame.Navigate(typeof(RecCalibration), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                }
                conn.Close();
            }
        }
    }
}
