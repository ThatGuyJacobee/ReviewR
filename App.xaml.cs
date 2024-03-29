﻿using System;
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
using MySqlConnector;
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
        private static string connectionString = "ADD_DB_CONNECTION_STRING"; //Add connection string in the format of "server=[];database=[],uid=[],pwd=[];";

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
        public static string GlobalClientidIGDB = "ADD_IGDB_API_KEY"; //Add a IGDB API Key - Grab the key from https://api-docs.igdb.com/#account-creation

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
                Uri uri = new Uri("ADD URL");
                //Add oauth2 url following: https://id.twitch.tv/oauth2/token?client_id=ID_HERE&client_secret=SECRET_HERE&grant_type=client_credentials
				//Get ClientID and Client Secret from account management on IGDB: https://api-docs.igdb.com/#account-creation

                // Construct the JSON to post
                HttpStringContent content = new HttpStringContent("ADD URL");
                //Add oauth2 url following: https://id.twitch.tv/oauth2/token?client_id=ID_HERE&client_secret=SECRET_HERE&grant_type=client_credentials
				//Get ClientID and Client Secret from account management on IGDB: https://api-docs.igdb.com/#account-creation

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
        /// Invoked when the application is launched through a custom URI scheme, such as
        /// is the case in an OAuth 2.0 authorization flow.
        /// </summary>
        /// <param name="args">Details about the URI that activated the app.</param>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            // When the app was activated by a Protocol (custom URI scheme), forwards
            // the URI to the MainPage through a Navigate event.
            if (args.Kind == ActivationKind.Protocol)
            {
                // Extracts the authorization response URI from the arguments.
                ProtocolActivatedEventArgs protocolArgs = (ProtocolActivatedEventArgs)args;
                Uri uri = protocolArgs.Uri;
                Debug.WriteLine("Authorization Response: " + uri.AbsoluteUri);

                // Gets the current frame, making one if needed.
                var frame = Window.Current.Content as Frame;
                if (frame == null)
                    frame = new Frame();

                // Opens the URI for "navigation" (handling) on the MainPage.
                frame.Navigate(typeof(LoginPage), uri); //Go to loginpage to perform oauth database checks
                Window.Current.Content = frame;
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
