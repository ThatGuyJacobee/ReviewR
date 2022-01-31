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
using Windows.Storage; //Storage and below required for Google OAuth 2.0 Login
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using System.Net.Http;
using System.Text;
using Windows.Data.Json;
using Windows.Storage.Streams;
using System.Text.Json; //Used for (de)serizalisation and JSON manipulation
using System.Text.Json.Serialization; //Used as serialization library
using System.Net.Mail; //Used to send emails

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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
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
                    //When the Register Account button is clicked, display the Register Content Dialog
                    ContentDialog nocondialog = new NoConnection();
                    await nocondialog.ShowAsync();
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

        private async void forgot_password_Click(object sender, RoutedEventArgs e)
        {
            //When the Forgot button is clicked, display the Reset Password Content Dialog
            ContentDialog passwordreset = new PasswordResetDialog();
            await passwordreset.ShowAsync();
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

        private bool DatabaseAccountWrongSignin(string email) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT UserID, Username, Email, Password FROM user_data WHERE Email=@email AND AuthSub IS NOT NULL"; //Selects the email and password rows from user_data
                cmd.Parameters.AddWithValue("@email", email); //Sets them as variables
                cmd.Connection = conn;

                MySqlDataReader login = cmd.ExecuteReader(); //Executes a read command for the table
                if (login.Read())
                {
                    conn.Close(); //Close connection
                    return true;
                }
                else
                {
                    conn.Close(); //Close connection
                    return false;
                }
            }
        }

        private async void login_next_Click(object sender, RoutedEventArgs e) //Method ran on login button press
        {
            string email = email_entry.Text; //Inputs set as variables
            string pass = password_entry.Password;

            if (email == "" || pass == "") //If either are empty display an error
            {
                login_status.Visibility = Visibility.Visible;
                login_status.Text = "Error: Details cannot be empty";
                return;
            }

            bool databaseWrongSignIn = DatabaseAccountWrongSignin(email);

            if (!databaseWrongSignIn)
            {
                try
                {
                    bool loginSuccessful = DataValidation(email, pass); //Run the DataValidation method

                    if (loginSuccessful)
                    {
                        //Each time on logon, update the last logon date and time for the user
                        using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
                        {
                            conn.Open();
                            MySqlCommand cmd = conn.CreateCommand();

                            //Sets variables and SQL command
                            cmd.CommandText = "UPDATE user_data SET lastlogon=@lastlogon WHERE UserID=@UserID";
                            cmd.Parameters.AddWithValue("@UserID", App.GlobalUserID);
                            cmd.Parameters.AddWithValue("@lastlogon", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }

                        //Navitage to the Main Menu class
                        this.Frame.Navigate(typeof(NavigationBar), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()); //If input compares identically to database, proceed to main menu
                    }

                    else
                    {
                        login_status.Visibility = Visibility.Visible; //Otherwise display an error
                        login_status.Text = "Error: Incorrect details";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Data check was unsuccessful.");
                    Debug.WriteLine(ex.ToString());
                }

            }
            else
            {
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Account already exists.\nAn account with the supplied email address exists as a Google Account log-in. Please use this method to login.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                await errordialog.ShowAsync();
            }
        }

        private void database_status_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        const string clientID = "139950512543-hh4oeaddj7borputdqi9jt3ju3o9322s.apps.googleusercontent.com";
        const string redirectURI = "reviewr.oauth2:/oauth2redirect";
        const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

        private void google_signin_Click(object sender, RoutedEventArgs e)
        {
            // Generates state and PKCE values.
            string state = randomDataBase64url(32);
            string code_verifier = randomDataBase64url(32);
            string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Stores the state and code_verifier values into local settings.
            // Member variables of this class may not be present when the app is resumed with the
            // authorization response, so LocalSettings can be used to persist any needed values.
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["state"] = state;
            localSettings.Values["code_verifier"] = code_verifier;

            // Creates the OAuth 2.0 authorization request. Amended by adding %20email within the request to return email value.
            string authorizationRequest = string.Format("{0}?response_type=code&scope=openid%20profile%20email&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);

            output("Opening authorization request URI: " + authorizationRequest);

            // Opens the Authorization URI in the browser.
            var success = Windows.System.Launcher.LaunchUriAsync(new Uri(authorizationRequest));
        }

        /// <summary>
        /// Processes the OAuth 2.0 Authorization Response
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Uri)
            {
                // Gets URI from navigation parameters.
                Uri authorizationResponse = (Uri)e.Parameter;
                string queryString = authorizationResponse.Query;
                output("MainPage received authorizationResponse: " + authorizationResponse);

                // Parses URI params into a dictionary
                // ref: http://stackoverflow.com/a/11957114/72176
                Dictionary<string, string> queryStringParams =
                        queryString.Substring(1).Split('&')
                             .ToDictionary(c => c.Split('=')[0],
                                           c => Uri.UnescapeDataString(c.Split('=')[1]));

                if (queryStringParams.ContainsKey("error"))
                {
                    output(String.Format("OAuth authorization error: {0}.", queryStringParams["error"]));
                    return;
                }

                if (!queryStringParams.ContainsKey("code")
                    || !queryStringParams.ContainsKey("state"))
                {
                    output("Malformed authorization response. " + queryString);
                    return;
                }

                // Gets the Authorization code & state
                string code = queryStringParams["code"];
                string incoming_state = queryStringParams["state"];

                // Retrieves the expected 'state' value from local settings (saved when the request was made).
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                string expected_state = (String)localSettings.Values["state"];

                // Compares the receieved state to the expected value, to ensure that
                // this app made the request which resulted in authorization
                if (incoming_state != expected_state)
                {
                    output(String.Format("Received request with invalid state ({0})", incoming_state));
                    return;
                }

                // Resets expected state value to avoid a replay attack.
                localSettings.Values["state"] = null;

                // Authorization Code is now ready to use!
                output(Environment.NewLine + "Authorization code: " + code);

                string code_verifier = (String)localSettings.Values["code_verifier"];
                performCodeExchangeAsync(code, code_verifier);
            }
            else
            {
                Debug.WriteLine(e.Parameter);
            }
        }

        async void performCodeExchangeAsync(string code, string code_verifier)
        {
            // Builds the Token request
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&scope=&grant_type=authorization_code",
                code,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                code_verifier
                );
            StringContent content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            HttpClient client = new HttpClient(handler);

            output(Environment.NewLine + "Exchanging code for tokens...");
            HttpResponseMessage response = await client.PostAsync(tokenEndpoint, content);
            string responseString = await response.Content.ReadAsStringAsync();
            output(responseString);

            if (!response.IsSuccessStatusCode)
            {
                output("Authorization code exchange failed.");
                return;
            }

            // Sets the Authentication header of our HTTP client using the acquired access token.
            JsonObject tokens = JsonObject.Parse(responseString);
            string accessToken = tokens.GetNamedString("access_token");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Makes a call to the Userinfo endpoint, and prints the results.
            output("Making API Call to Userinfo...");
            HttpResponseMessage userinfoResponse = client.GetAsync(userInfoEndpoint).Result;
            string userinfoResponseContent = await userinfoResponse.Content.ReadAsStringAsync();
            output(userinfoResponseContent);

            //After the API callback is returned, run the check event
            OAuthSuccessCheck(userinfoResponseContent);
        }

        public partial class UserInfoObject
        {
            [JsonPropertyName("sub")]
            public string Sub { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }
        }

        //Method checks whether the given google account's email and Sub
        //(Google unique number) are already in the database, if not then
        //a new userID is created with the given Sub and email.
        public async void OAuthSuccessCheck(string userinfoResponseContent)
        {
            UserInfoObject Userinfodata = JsonSerializer.Deserialize<UserInfoObject>(userinfoResponseContent);

            Debug.WriteLine("UserInfo Object returned and deserialized");

            var SubUser = Userinfodata.Sub;
            var EmailUser = Userinfodata.Email;
            Debug.WriteLine("Sub: " + Userinfodata.Sub);
            Debug.WriteLine("Email: " + Userinfodata.Email);

            bool foundMatch = DatabaseAccountValidation(SubUser, EmailUser); //Run the Database validation method to check if user already exists
            bool foundDuplicate = DatabaseAccountDuplication(EmailUser); //Run the Database duplication method to check whether the user already has an normal account

            if (foundMatch && !foundDuplicate)
            {
                Debug.WriteLine("OAuth current login user: Match Found to database!");

                //Each time on logon, update the last logon date and time for the user
                using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Sets variables and SQL command
                    cmd.CommandText = "UPDATE user_data SET lastlogon=@lastlogon WHERE UserID=@UserID";
                    cmd.Parameters.AddWithValue("@UserID", App.GlobalUserID);
                    cmd.Parameters.AddWithValue("@lastlogon", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                //Navitage to the Main Menu class
                this.Frame.Navigate(typeof(NavigationBar), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
            }
            else if (foundDuplicate)
            {
                Debug.WriteLine("OAuth current login user: A regular account with the associated email address already exists!");

                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "The email with the associated Google Account already exists as a regular account, hence it can't be used again! Please login through the regular account or if your forgot your credentials, reset your password.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                await errordialog.ShowAsync();
            }
            else if (!foundMatch && !foundDuplicate)
            {
                Debug.WriteLine("OAuth current login user: No matches to the database or duplicate accounts!\nCreating a new UserID!");

                using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
                {
                    try
                    {
                        conn.Open();
                        MySqlCommand cmd = conn.CreateCommand();

                        //Selects the user_data table to insert email and password into
                        cmd.CommandText = "INSERT INTO user_data (email, authsub, creationdate) VALUES (@email, @sub, @creationdate)";
                        cmd.Parameters.AddWithValue("@email", EmailUser); //Sets them as variables
                        cmd.Parameters.AddWithValue("@sub", SubUser);
                        cmd.Parameters.AddWithValue("@creationdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); //Variable to set the creation date alongside the account.

                        //If statement performs a server-side (pre-insert) validation to ensure data matches requirements
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        Debug.WriteLine("OAuth current login user: New user has been successfully created!");

                        MailMessage mail = new MailMessage();
                        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                        mail.From = new MailAddress("reviewrproject@gmail.com");
                        mail.To.Add(EmailUser);
                        mail.Subject = "Account has been successfully registered!";
                        mail.Body = "Hi there,\n\nThank you for registering an account on the ReviewR app via Google Account method.\nThis email has been automatically sent as a confirmation for account creation.\nIf this action wasn't you or this email was wrongfully sent, please ignore this message.\n\nReviewR App";

                        SmtpServer.Port = 587;
                        SmtpServer.Credentials = new System.Net.NetworkCredential("reviewrproject@gmail.com", "GuyJacobee_1");
                        SmtpServer.EnableSsl = true;

                        SmtpServer.Send(mail);
                        Debug.WriteLine("Account registration Email has been successfully sent to: " + EmailUser);

                        //Now check again if the user is in the database by re-running the database account validation method
                        OAuthSuccessCheck(userinfoResponseContent);
                    }
                    catch
                    {
                        Debug.WriteLine("OAuth current login user: Error! New User not created!");
                        ContentDialog errordialog = new ContentDialog();
                        errordialog.Title = "Error!";
                        errordialog.Content = "Something went wrong.\nReport this issue to the developer.\n[ERROR CODE: OAUTH_INSERT]";
                        errordialog.CloseButtonText = "Approve";
                        errordialog.DefaultButton = ContentDialogButton.Close;

                        await errordialog.ShowAsync();
                        conn.Close();
                    }
                }
            }
        }

        private bool DatabaseAccountValidation(string SubUser, string EmailUser) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT UserID, Username, Email, Password FROM user_data WHERE Email=@email AND AuthSub=@sub"; //Selects the email and password rows from user_data
                cmd.Parameters.AddWithValue("@email", EmailUser); //Sets them as variables
                cmd.Parameters.AddWithValue("@sub", SubUser);
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

        private bool DatabaseAccountDuplication(string EmailUser) //Method for database validation
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString)) //Uses private connection string
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT UserID, Username, Email, Password FROM user_data WHERE Email=@email AND AuthSub IS NULL"; //Selects the email and password rows from user_data
                cmd.Parameters.AddWithValue("@email", EmailUser); //Sets them as variables
                cmd.Connection = conn;

                MySqlDataReader login = cmd.ExecuteReader(); //Executes a read command for the table
                if (login.Read())
                {
                    conn.Close(); //Close connection
                    return true;
                }
                else
                {
                    conn.Close(); //Close connection
                    return false;
                }
            }
        }

        /// <summary>
        /// Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        public void output(string output)
        {
            Debug.WriteLine(output);
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string randomDataBase64url(uint length)
        {
            IBuffer buffer = CryptographicBuffer.GenerateRandom(length);
            return base64urlencodeNoPadding(buffer);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static IBuffer sha256(string inputString)
        {
            HashAlgorithmProvider sha = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(inputString, BinaryStringEncoding.Utf8);
            return sha.HashData(buff);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string base64urlencodeNoPadding(IBuffer buffer)
        {
            string base64 = CryptographicBuffer.EncodeToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }
    }
}
