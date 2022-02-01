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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    public sealed partial class SettingsChooseDialog : ContentDialog
    {
        public SettingsChooseDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void password_change_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog passwordtokenreset = new PasswordTokenDialog();

            settingschoose_contentdialog.Hide(); //Hide the current choice dialog
            await passwordtokenreset.ShowAsync(); //Re-use the password reset class previously made for the login screen
        }

        private async void set_avatar_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog setavatar = new AvatarEditDialog();

            settingschoose_contentdialog.Hide(); //Hide the current choice dialog
            await setavatar.ShowAsync(); //Re-use the password reset class previously made for the login screen
        }

        private async void edit_profilebio_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog editbio = new AccountEditDialog();

            settingschoose_contentdialog.Hide(); //Hide the current choice dialog
            await editbio.ShowAsync(); //Re-use the password reset class previously made for the login screen
        }
    }
}
