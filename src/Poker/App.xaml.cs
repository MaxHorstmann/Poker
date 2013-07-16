using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using PokerObjects;
using Poker.Common;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

// source of poker deck images:
// http://commons.wikimedia.org/wiki/Poker_(cards_deck)


namespace Poker
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public Table CurrentTable { get; set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Common.SuspensionManager.KnownTypes.Add(typeof(Table));
            Common.SuspensionManager.KnownTypes.Add(typeof(HandHistory));
            Common.SuspensionManager.KnownTypes.Add(typeof(HandHistoryWindowsStoreApp));
            Common.SuspensionManager.KnownTypes.Add(typeof(HumanPlayer));
            Common.SuspensionManager.KnownTypes.Add(typeof(ComputerPlayer));
            Common.SuspensionManager.KnownTypes.Add(typeof(Pot));
            Common.SuspensionManager.KnownTypes.Add(typeof(Deck));


            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Add frame to suspension manager
                Common.SuspensionManager.RegisterFrame(rootFrame, "appFrame");
                if ((args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    || (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser))
                {
                    try
                    {
                        await Common.SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(TitlePage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }


            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            
            // save current poker game...
            await Common.SuspensionManager.SaveAsync();


            deferral.Complete();
        }
    }
}
