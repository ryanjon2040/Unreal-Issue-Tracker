using HtmlAgilityPack;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UIssueTracker.Classes;

namespace UIssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for IssueDetails.xaml
    /// </summary>
    public partial class IssueDetails : UserControl
    {
        public string IssueURL { get; private set; }
        public string CurrentIssueTitle { get; private set; }
        public string CurrentIssueStatus { get; private set; }
        public string CurrentIssueDescription { get; private set; }
        public string CurrentIssueRepro { get; private set; }
        public string CurrentIssueAnswerHubLink { get; private set; }

        private bool bDescriptionSet = false;
        private bool bStepsToReproduceSet = false;
        private string CurrentIssueNumber = null;
        private MainWindow OwningWindow = null;

        public IssueDetails()
        {
            InitializeComponent();
        }

        public void LoadIssue(string URL, string Title, bool bIsSubscribedCard, MainWindow mainWindow)
        {
            OwningWindow = mainWindow;
            CurrentIssueAnswerHubLink = null;

            bDescriptionSet = false;
            bStepsToReproduceSet = false;
            IssueURL = URL;

            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(IssueURL);

            CurrentIssueNumber = Constants.GetIssueNumber(URL);            

            if (Title == null)
            {
                Title = htmlDoc.DocumentNode.SelectNodes("//*[@class='visible-xs']").Select(n => n.InnerText.Trim()).ElementAt(0);
            }
            CurrentIssueTitle = Title;
            IssueTitle.Text = CurrentIssueTitle;
            CurrentIssueStatus = htmlDoc.DocumentNode.SelectNodes("//*[@class='resolution']").Select(n => n.InnerText.Trim()).ElementAt(0);
            IssueStatus.Text = CurrentIssueStatus;

            Subscribe_Btn.IsEnabled = true;
            if (bIsSubscribedCard == false)
            {
                Subscribe_Btn.IsEnabled = CurrentIssueStatus != "Fixed";
            }
            Subscribe_Btn.Content = Constants.IsSubscribedToIssue(CurrentIssueNumber) ? "UNSUBSCRIBE" : "SUBSCRIBE";
            
            IssueStatusColor.Background = Constants.GetResolutionColor(IssueStatus.Text);

            HtmlNode[] aNodes = htmlDoc.DocumentNode.SelectNodes("//a").ToArray();
            foreach (var n in aNodes)
            {
                string InnerText = n.InnerText.ToLower();
                if (n.Attributes["href"] != null 
                    && (InnerText == "answerhub" || InnerText == "udn") 
                    && CurrentIssueAnswerHubLink == null 
                    && (n.Attributes["href"].Value.EndsWith("ask.html") == false || n.Attributes["href"].Value.Contains("udn.unrealengine.com")))
                {
                    CurrentIssueAnswerHubLink = n.Attributes["href"].Value;
                    AnswerHubLinkBtn.Content = "VISIT ANSWER HUB";
                    if (InnerText == "udn")
                    {
                        AnswerHubLinkBtn.Content = "VISIT UDN";
                    }
                    break;
                }
            }
            AnswerHubLinkBtn.IsEnabled = CurrentIssueAnswerHubLink != null;

            SideStackPanel.Children.Clear();
            HtmlNodeCollection thcells = htmlDoc.DocumentNode.SelectNodes("//table[@class='table']/tr/th");
            for (int i = 0; i < thcells.Count; ++i)
            {
                HtmlNodeCollection cells = htmlDoc.DocumentNode.SelectNodes("//table[@class='table']/tr/td");
                var DynamicPanelButton = new Button() { Content = Constants.RemoveHtmlTags(cells[i].InnerText) };
                DynamicPanelButton.IsEnabled = false;
                HtmlNode a = cells[i].SelectSingleNode("a[@href]");
                if (a != null)
                {
                    DynamicPanelButton.Tag = Constants.RemoveHtmlTags(a.Attributes["href"].Value);
                    DynamicPanelButton.Click += DynamicPanelButton_Click;
                    DynamicPanelButton.IsEnabled = true;
                }
                SideStackPanel.Children.Add(DynamicPanelButton);
            }

            var texts = htmlDoc.DocumentNode.SelectNodes("//*[@class='panel-body']").Select(n => n.InnerText.Trim());
            foreach (var text in texts)
            {
                if (bDescriptionSet == false)
                {
                    bDescriptionSet = true;
                    CurrentIssueDescription = text;
                    IssueDescription.Text = CurrentIssueDescription;
                    continue;
                }

                if (bStepsToReproduceSet == false)
                {
                    bStepsToReproduceSet = true;
                    CurrentIssueRepro = text;
                    IssueReproduceSteps.Text = CurrentIssueRepro;
                    break;
                }
            }

            Constants.SaveToHistory(this);
            OwningWindow.LoadHistoryCards();
        }

        private void DynamicPanelButton_Click(object sender, RoutedEventArgs e) //Event which will be triggered on click of DynamicPanelButton
        {
            string Tag = ((Button)sender).Tag.ToString();
            Console.WriteLine("TAG: " + Tag);
            if (Tag.StartsWith(@"/issue/"))
            {
                System.Diagnostics.Process.Start(@"https://issues.unrealengine.com" + Tag);
            }
            else
            {
                System.Diagnostics.Process.Start(Tag);
            }
        }

        private void Subscribe_Btn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)Subscribe_Btn.Content == "UNSUBSCRIBE")
            {
                Constants.UnSubscribeFromIssue(CurrentIssueNumber);
                Subscribe_Btn.Content = "SUBSCRIBE";
            }
            else
            {
                if (Constants.SubscribeToIssue(this))
                {
                    Subscribe_Btn.Content = "UNSUBSCRIBE";
                }                
            }
            OwningWindow.LoadSubscribedIssuesAsync();
        }

        private void AnswerHubLinkBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(CurrentIssueAnswerHubLink);
        }
    }
}
