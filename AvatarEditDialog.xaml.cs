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
using Windows.Web.Http; //For POST method
using System.Threading.Tasks; //For POST method
using System.Diagnostics; //Debug
using MySqlConnector;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    public sealed partial class AvatarEditDialog : ContentDialog
    {
        public AvatarEditDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //Added a parameter which runs on close button close to return to original edit dialog
            ContentDialog returnchoose = new SettingsChooseDialog();
            avataredit_contentdialog.Hide();
            await returnchoose.ShowAsync();
        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            bool avataredit = DefaultUpdateReview();

            if (avataredit)
            {
                ContentDialog successdialog = new ContentDialog();
                successdialog.Title = "Success!";
                successdialog.Content = "Your account avatar has been changed successfully!\nYou may have to refresh the page to see effect.";
                successdialog.CloseButtonText = "Approve";
                successdialog.DefaultButton = ContentDialogButton.Close;

                avataredit_contentdialog.Hide();
                await successdialog.ShowAsync(); //Displays it until the close button is pressed

                //Added a parameter which runs on close button close to return to original edit dialog
                ContentDialog returnchoose = new SettingsChooseDialog();
                returnchoose.CloseButtonCommandParameter = await returnchoose.ShowAsync();
            }

            else
            {
                //Display an content dialog which states the error - blackbox testing
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Database update was unsuccessful. This may be an issue on the program side, please try again. If you encounter the error constantly create a bug report.\n\n[Error Code: UPDATE_FAILURE_DB]";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                avataredit_contentdialog.Hide();
                await errordialog.ShowAsync(); //Display it until user presses the accept button
                errordialog.CloseButtonCommandParameter = await avataredit_contentdialog.ShowAsync();
            }
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        private bool DefaultUpdateReview()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(App.ConnectionString)) //Uses private connection string
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();

                    //Sets variables and SQL command
                    cmd.CommandText = "UPDATE user_data SET UserAvatar=@avatar WHERE UserID=@userid";
                    cmd.Parameters.AddWithValue("@avatar", "");
                    cmd.Parameters.AddWithValue("@userid", App.GlobalUserID);

                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public async Task ImgurUploadAPI()
        {
            try
            {
                if (imgpath != null)
                {
                    // Construct the HttpClient and Uri
                    HttpClient httpClient = new HttpClient();
                    Uri uri = new Uri("https://api.imgur.com/3/upload");

                    httpClient.DefaultRequestHeaders.Add("Authorization", "Client-ID 08297f306620602");
                    //Debug.WriteLine("Request Headers: ");

                    var file = File.ReadAllBytes(imgpath);
                    var finalimg = Convert.ToBase64String(file);

                    // Construct the JSON to post
                    HttpStringContent content = new HttpStringContent("image=\"{finalimg}\"");
                    Debug.WriteLine("Request Upload: " + content);

                    // Post the JSON and wait for a response
                    HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                        uri,
                        content);

                    // Make sure the post succeeded, and write out the response
                    httpResponseMessage.EnsureSuccessStatusCode();
                    var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                    Debug.WriteLine("Request Response: " + httpResponseBody);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        //Set the GameID as a static long variable which I will use to request further data about the game in the game-specific pages
        public static string imgpath = "";

        private async void FileNameButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                imgpath = file.Path;
                await ImgurUploadAPI();
                Debug.WriteLine("Picked Image: " + file.Name);
                uploadedimage_text.Text = "Picked Image: " + file.Name;
            }
            else
            {
                Debug.WriteLine("Image uploading has been cancelled.");
            }
        }
    }
}
