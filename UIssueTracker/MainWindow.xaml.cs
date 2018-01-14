using CodeHollow.FeedReader;
using System.Windows;
using UIssueTracker.Classes;
using UIssueTracker.UserControls;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;

namespace UIssueTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            HistoryCategory.Visibility = Visibility.Collapsed;
            MyTransitioner.SelectedIndex = 0;
            Title = Constants.PRODUCT_NAME;

            Dispatcher.BeginInvoke((Action)LoadSubscribedIssuesAsync);
            LoadBugsAsync();
            LoadFixesAsync();
            LoadHistoryCards();
        }

        public void LoadHistoryCards()
        {
            HistoryContainer.Children.Clear();
            HistoryCategory.Visibility = Visibility.Collapsed;
            if (File.Exists(Constants.GetIssuesHistoryFile()))
            {
                HistoryIssue historyIssue = Newtonsoft.Json.JsonConvert.DeserializeObject<HistoryIssue>(File.ReadAllText(Constants.GetIssuesHistoryFile()));
                foreach (var s in historyIssue.IssueHistory)
                {
                    IssueHistoryCard issueHistoryCard = new IssueHistoryCard(s.Key, s.Value, this);
                    HistoryContainer.Children.Add(issueHistoryCard);
                    HistoryCategory.Visibility = Visibility.Visible;
                }
            }
        }

        public async void LoadSubscribedIssuesAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Directory.Exists(Constants.GetIssueFilesDirectory()))
                    {
                        string[] SubscribedIssues = Directory.GetFiles(Constants.GetIssueFilesDirectory());
                        SubscribedIssuesContainer.Children.Clear();
                        int FixedIssues = 0;
                        int TotalIssuesSubscribed = 0;
                        foreach (string s in SubscribedIssues)
                        {
                            TotalIssuesSubscribed++;
                            SubscribeIssue subscribeIssue = Constants.GetSubscribedIssue(s);
                            IssueCard issueCard = new IssueCard(subscribeIssue.SubscribedIssueTitle, subscribeIssue.SubscribedIssueDescription, subscribeIssue.SubscribedIssueLink, this, null, true);
                            if (issueCard.HasIssueResolutionChanged(s, out Constants.IssueResolution OutChangedResolution) == false)
                            {
                                if (OutChangedResolution != Constants.IssueResolution.Fixed)
                                {
                                    SubscribedIssuesContainer.Children.Add(issueCard);
                                }
                                else
                                {
                                    FixedIssues++;
                                }
                            }
                        }
                        if (FixedIssues > 0)
                        {
                            NumberOfIssuesFixed.Text = string.Format("{0}/{1} ISSUE(S) HAS BEEN FIXED!", FixedIssues, TotalIssuesSubscribed);
                            NumberOfIssuesFixedDialogHost.IsOpen = true;
                        }
                    }
                }));
            });
        }

        private async void LoadBugsAsync()
        {
            Feed feed = await FeedReader.ReadAsync(Constants.BUG_URL);
            foreach (var item in feed.Items)
            {
                BugsContainer.Children.Add(new IssueCard(item.Title, item.Description, item.Link, this, true));                
            }
            BugsLoadingBar.Visibility = Visibility.Collapsed;
        }

        private async void LoadFixesAsync()
        {
            Feed feed = await FeedReader.ReadAsync(Constants.FIX_URL);
            foreach (var item in feed.Items)
            {                
                FixesContainer.Children.Add(new IssueCard(item.Title, item.Description, item.Link, this, false));
            }
            FixesLoadingBar.Visibility = Visibility.Collapsed;
        }

        private void SearchIssue_Btn_Click(object sender, RoutedEventArgs e)
        {
            ShowTransitioner(@"https://issues.unrealengine.com/issue/" + IssueSearchText.Text, null, false);
        }

        public void ShowTransitioner(string Link, string Title, bool bIsSubscribedCard)
        {
            MyDialogHost.IsOpen = true;
            MyTransitioner.SelectedIndex = 1;
            IssueDetail.LoadIssue(Link, Title, bIsSubscribedCard, this);
            MyDialogHost.IsOpen = false;
        }

        private void UE4_Twitter_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://twitter.com/UnrealEngine");
        }

        private void UE4_FB_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://www.facebook.com/UnrealEngine/");
        }

        private void UE4_Git_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/EpicGames/UnrealEngine");
        }

        private void Satheesh_Twitter_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://twitter.com/ryanjon2040");
        }

        private void Satheesh_FB_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://www.facebook.com/satheeshpv");
        }

        private void Satheesh_Git_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/ryanjon2040");
        }
    }
}
