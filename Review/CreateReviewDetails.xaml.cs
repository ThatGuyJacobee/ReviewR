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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateReviewDetails : Page
    {
        public CreateReviewDetails()
        {
            this.InitializeComponent();
        }

        public static string ReviewTitle = "";
        public static string ReviewDescription = "";

        private async void continue_button_Click(object sender, RoutedEventArgs e)
        {
            string Title = create_title.Text;
            string Description = create_description.Text;

            if (!string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Description) && Description.Length <= 2048)
            {
                this.Frame.Navigate(typeof(CreateReviewPreview), null); //Switch to the review submit preview page
                ReviewTitle = Title;
                ReviewDescription = Description;
            }

            else
            {
                //Display an content dialog which states the error - blackbox testing
                ContentDialog errordialog = new ContentDialog();
                errordialog.Title = "Error!";
                errordialog.Content = "Validation not passed.\nMake sure NOT to include any inapprporiate words and keep the description below 2048 characters.";
                errordialog.CloseButtonText = "Approve";
                errordialog.DefaultButton = ContentDialogButton.Close;

                await errordialog.ShowAsync(); //Display it until user presses the accept button
            }
        }
    }
}
