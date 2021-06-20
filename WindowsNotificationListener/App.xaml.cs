using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using System.Diagnostics;
using Windows.System;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.UI.Popups;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Specialized;
using HtmlAgilityPack;
using System.Web;
using Windows.UI.Xaml;
using Octokit;

namespace WindowsNotificationListener
{
    sealed partial class App : Windows.UI.Xaml.Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                }
                Window.Current.Activate();
                //minimizeProgram();

            }
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
            {
                //connectAndPostAsync();
                getNotifications();
            }

            else
            {
                Console.WriteLine("Older version, cannot listen to Notifications, exiting now.");
            }

        }
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }


        private async Task connectAndPostAsync(string v, string v1, string bodyText)
        {
            var gitHubClient = new GitHubClient(new Octokit.ProductHeaderValue("DOTNETAPP"));
            gitHubClient.Credentials = new Credentials("DELETED THIS FOR NOW");

  

            var sb = new StringBuilder("---\n");
            sb.AppendLine("layout: post\n");
            sb.AppendLine("title:  webull post!\n");
            sb.AppendLine("date: " + v + "\n");
            sb.AppendLine("categories: webull\n");
            sb.AppendLine("---\n");
            sb.AppendLine(bodyText);

            var (owner, repoName, filePath, branch) = ("kvnlpz", "RSSFeed", "_posts/" + v + "-webull-" + v1 + ".markdown", "main");
            await gitHubClient.Repository.Content.CreateFile(owner, repoName, filePath, new CreateFileRequest("update from DotNet app", sb.ToString(), branch));

        }



        public async void minimizeProgram()
        {
            IList<AppDiagnosticInfo> infos = await AppDiagnosticInfo.RequestInfoForAppAsync();
            IList<AppResourceGroupInfo> resourceInfos = infos[0].GetResourceGroups();
            await resourceInfos[0].StartSuspendAsync();
        }

        public async void getNotifications()
        {
            UserNotificationListener listener = UserNotificationListener.Current;
            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();
            Debug.WriteLine("Inside the notif function");
            switch (accessStatus)
            {
                case UserNotificationListenerAccessStatus.Allowed:
                    IReadOnlyList<UserNotification> notifs = await listener.GetNotificationsAsync(NotificationKinds.Toast);
                    foreach (UserNotification s in notifs)
                    {
                        string appDisplayName = s.AppInfo.DisplayInfo.DisplayName;
                        Debug.WriteLine(appDisplayName);
                        
                        if (appDisplayName.Contains("webull"))
                        {

                            NotificationBinding toastBinding = s.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
                            if (toastBinding != null)
                            {
                                IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();
                                string titleText = textElements.FirstOrDefault()?.Text;
                                Debug.WriteLine(titleText);
                                if (titleText.Contains("following"))
                                {
                                    string bodyText = string.Join("\n", textElements.Skip(1).Select(t => t.Text));
                                    Debug.WriteLine(bodyText);
                                    await connectAndPostAsync(DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"), appDisplayName);
                                    //await connectAndPostAsync(DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"), bodyText);
                                }

                            }
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    break;
                case UserNotificationListenerAccessStatus.Denied:
                    break;
                case UserNotificationListenerAccessStatus.Unspecified:
                    break;
            }
        }
    }
}
