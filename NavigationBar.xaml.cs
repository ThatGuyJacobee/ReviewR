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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReviewR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationBar : Page
    {
        public NavigationBar()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(MainMenu)); //Loads MainMenu Page as default
        }
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // you can also add items in code behind
            //NavView.MenuItems.Add(new NavigationViewItemSeparator());
            //NavView.MenuItems.Add(new NavigationViewItem()
            //{ Content = "My content", Icon = new SymbolIcon(Symbol.Folder), Tag = "content" });

            // set the initial SelectedItem 
            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "home")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                // find NavigationViewItem with Content that equals InvokedItem
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
                NavView_Navigate(item as NavigationViewItem);
            }
        }

        private void NavView_Navigate(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "home":
                    ContentFrame.Navigate(typeof(MainMenu), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                    break;

                case "revsys":
                    ContentFrame.Navigate(typeof(ReviewSystem), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                    break;

                case "recsys":
                    ContentFrame.Navigate(typeof(RecommendationSystem), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                    break;

                case "profile":
                    ContentFrame.Navigate(typeof(ProfilePages), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                    break;

                case "ghubs":
                    ContentFrame.Navigate(typeof(GameHubs), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                    break;

                case "gethelp":
                    ContentFrame.Navigate(typeof(GetHelp), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
                    break;

                case "logout":
                    this.Frame.Navigate(typeof(LoginPage), null); //Navigates the entire page rather than just the Content Frame of the navbar
                    App.GlobalUserID = 0; //Sets the GlobalUserID to default 0
                    App.GlobalUsername = ""; //Sets the GlobalUsername to default empty
                    break;
            }
        }
    }
}
