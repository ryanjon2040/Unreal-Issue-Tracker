using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using UIssueTracker.Classes;

namespace UIssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for IssueCard.xaml
    /// </summary>
    public partial class IssueCard : UserControl
    {
        private MainWindow mainWindow = null;
        private bool _bIsSubscribedCard = false;
        private string LoadURL = null;

        public IssueCard(string Title, string Description, string Link, MainWindow NewMainWindow, bool? bIsBug = null, bool bIsSubscribedCard = false)
        {
            InitializeComponent();
            mainWindow = NewMainWindow;
            CardTitle.Header = Title;
            CardDescription.Text = Constants.RemoveHtmlTags(Description);
            CardLink.Text = Link;
            LoadURL = Link;
            _bIsSubscribedCard = bIsSubscribedCard;
            if (bIsBug != null)
            {                
                CardTitle.Foreground = (bool)bIsBug ? Constants.ToSolidColorBrush("#DDE21717") : Constants.ToSolidColorBrush("#DD4EE217");
            }
        }

        public bool HasIssueResolutionChanged(string FilePath, out Constants.IssueResolution ChangedResolution)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(LoadURL);

            string CurrentIssueStatus = htmlDoc.DocumentNode.SelectNodes("//*[@class='resolution']").Select(n => n.InnerText.Trim()).ElementAt(0);
            CardTitle.Foreground = Constants.GetResolutionColor(CurrentIssueStatus);
            if (Constants.GetSubscribedIssue(FilePath).SubscribedIssueResolution.ToLower() != CurrentIssueStatus.ToLower())
            {
                ChangedResolution = Constants.GetResolution(CurrentIssueStatus);
                return true;
            }
            
            ChangedResolution = Constants.IssueResolution.None;
            return false;
        }

        private void ShowDetails_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mainWindow.ShowTransitioner(CardLink.Text, CardTitle.Header.ToString(), _bIsSubscribedCard);
        }

        private void IssueLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(LoadURL);
        }
    }
}
