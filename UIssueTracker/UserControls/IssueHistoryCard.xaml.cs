using System.IO;
using System.Windows.Controls;
using UIssueTracker.Classes;

namespace UIssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for IssueHistoryCard.xaml
    /// </summary>
    public partial class IssueHistoryCard : UserControl
    {
        private string IssueKey = null;
        private MainWindow OwningWindow = null;
        public IssueHistoryCard(string NewIssueNumber, string NewIssueTitle, MainWindow mainWindow)
        {
            InitializeComponent();
            CardTitle.Text = string.Format("({0}) {1}", NewIssueNumber, NewIssueTitle);
            IssueKey = NewIssueNumber;
            OwningWindow = mainWindow;
        }

        private void RemoveFromHistory_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            HistoryIssue historyIssue = Newtonsoft.Json.JsonConvert.DeserializeObject<HistoryIssue>(File.ReadAllText(Constants.GetIssuesHistoryFile()));
            historyIssue.IssueHistory.Remove(IssueKey);
            string Result = Newtonsoft.Json.JsonConvert.SerializeObject(historyIssue, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Constants.GetIssuesHistoryFile(), Result);
            OwningWindow.LoadHistoryCards();
        }
    }
}
