using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
using Windows.Web.Http; //For POST method
using System.Threading.Tasks; //For POST method
using Windows.Storage.Streams;
using System.Diagnostics; //Debug
using System.Text.Json; //Used for (de)serizalisation and JSON manipulation

namespace ReviewR
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        //Create a global method which can be inherited by any class to create a connection to the database
        private static string connectionString = "server=127.0.0.1;database=reviewr;uid=root;pwd=;SSL-mode=none;";

        public static string ConnectionString { get => connectionString; set => connectionString = value; }

        //Create a global method which can be inherited for all functions the user performs after being logged in successfully (default will be 0 / null)
        private static int GUserID = 0;

        //Value can be set after successful login to match who just logged in
        public static int GlobalUserID { get => GUserID; set => GUserID = value; }

        //Create a global method which can be inherited for all functions the user performs after being logged in successfully (default will be 0 / null)
        private static string GUsername = "";

        //Value can be set after successful login to match who just logged in
        public static string GlobalUsername { get => GUsername; set => GUsername = value; }

        //Set the value for IGDB access token received from the POST return output
        private static string GAccessIGDB = "";
        public static string GlobalAccessIGDB { get => GAccessIGDB; set => GAccessIGDB = value; }

        //Sets value for the Client ID when making an API request
        public static string GlobalClientidIGDB = "lcygp51ma1kkvf7ix71xlct0szpuqj";

        public class IGDBcredentials
        {
            public string access_token { get; set; }
        }
        
        private async Task TryPostJsonAsync() //Used MS docs as reference and adjusted for my use - https://docs.microsoft.com/en-us/windows/uwp/networking/httpclient#post-json-data-over-http
        {
            try
            {
                // Construct the HttpClient and Uri
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri("https://id.twitch.tv/oauth2/token?client_id=lcygp51ma1kkvf7ix71xlct0szpuqj&client_secret=m49i9n3p9roas71ij6zpps0ac50jnr&grant_type=client_credentials");

                // Construct the JSON to post
                HttpStringContent content = new HttpStringContent(
                    "https://id.twitch.tv/oauth2/token?client_id=lcygp51ma1kkvf7ix71xlct0szpuqj&client_secret=m49i9n3p9roas71ij6zpps0ac50jnr&grant_type=client_credentials");

                // Post the JSON and wait for a response
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                    uri,
                    content);

                // Make sure the post succeeded, and write out the response
                httpResponseMessage.EnsureSuccessStatusCode();
                var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();

                //Deserialize JSON into an c# object
                IGDBcredentials igdbcredentials = JsonSerializer.Deserialize<IGDBcredentials>(httpResponseBody);
                
                //Debug to ensure IGDB was successful
                Debug.WriteLine("IGDB Verification: " + httpResponseBody);
                Debug.WriteLine($"access_token: {igdbcredentials.access_token}");

                //Set the access_token as a static string to be invoked and used across the application
                GlobalAccessIGDB = igdbcredentials.access_token;
                Debug.WriteLine("Global Access IGDB API Token:" + GlobalAccessIGDB);
            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            TryPostJsonAsync();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(LoginPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
