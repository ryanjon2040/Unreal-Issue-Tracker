using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Newtonsoft.Json;
using UIssueTracker.UserControls;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UIssueTracker.Classes
{
    public class Constants
    {
        public static readonly string PRODUCT_NAME = "Unreal Issue Tracker";
        public static readonly string BUG_URL = @"https://issues.unrealengine.com/feed/bugs";
        public static readonly string FIX_URL = @"https://issues.unrealengine.com/feed/fixes";

        private static readonly string IssueTrackerFileExtension = ".uitf";        
        private static readonly string IssueFilesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\" + PRODUCT_NAME + @"\";
        private static readonly string IssueTrackerHistoryDirectory = IssueFilesDirectory + @"History\";
        private static readonly string IssueTrackerHistoryFile = IssueTrackerHistoryDirectory + "IssueHistory.uithf";

        public enum IssueResolution
        {
            Backlogged,
            ByDesign,
            CannotReproduce,
            Duplicate,
            Fixed,
            NonIssue,
            Unresolved,
            WontDo,
            WontFix,
            None
        }

        private static void CreateIssueFileLocation()
        {
            if (Directory.Exists(IssueFilesDirectory) == false)
            {
                Directory.CreateDirectory(IssueFilesDirectory);
            }
        }

        private static void CreateHistoryFile()
        {
            if (File.Exists(IssueTrackerHistoryFile) == false)
            {
                if (Directory.Exists(IssueTrackerHistoryDirectory) == false)
                {
                    Directory.CreateDirectory(IssueTrackerHistoryDirectory);
                }
                JObject EmptyJson = new JObject();
                File.WriteAllText(IssueTrackerHistoryFile, EmptyJson.ToString(Formatting.Indented));
            }
        }

        public static bool IsSubscribedToIssue(string IssueNumber)
        {
            return File.Exists(IssueFilesDirectory + IssueNumber + IssueTrackerFileExtension);
        }

        public static bool SaveToHistory(IssueDetails issueDetails)
        {
            CreateHistoryFile();
            HistoryIssue historyIssue = JsonConvert.DeserializeObject<HistoryIssue>(File.ReadAllText(IssueTrackerHistoryFile));
            if (historyIssue.IssueHistory.ContainsKey(GetIssueNumber(issueDetails.IssueURL)) == false)
            {
                historyIssue.IssueHistory.Add(GetIssueNumber(issueDetails.IssueURL), issueDetails.CurrentIssueTitle);
                string serializedJson = JsonConvert.SerializeObject(historyIssue, Formatting.Indented);
                File.WriteAllText(IssueTrackerHistoryFile, serializedJson);
                return true;
            }
            return false;
        }

        public static bool SubscribeToIssue(IssueDetails issueDetails)
        {
            CreateIssueFileLocation();
            string IssueNumber = GetIssueNumber(issueDetails.IssueURL);
            if (IsSubscribedToIssue(IssueNumber) == false)
            {
                SubscribeIssue subscribeIssue = new SubscribeIssue();
                subscribeIssue.SubscribedIssueResolution = issueDetails.CurrentIssueStatus;
                subscribeIssue.SubscribedIssueTitle = issueDetails.CurrentIssueTitle;
                subscribeIssue.SubscribedIssueID = IssueNumber;
                subscribeIssue.SubscribedIssueLink = issueDetails.IssueURL;
                subscribeIssue.SubscribedIssueDescription = issueDetails.CurrentIssueDescription;
                subscribeIssue.SubscribedIssueRepro = issueDetails.CurrentIssueRepro;
                subscribeIssue.SubscribedIssueMoreInfo = issueDetails.CurrentIssueAnswerHubLink;
                string SubscribeIssueJson = JsonConvert.SerializeObject(subscribeIssue, Formatting.Indented);
                File.WriteAllText(IssueFilesDirectory + IssueNumber + IssueTrackerFileExtension, SubscribeIssueJson);
                return true;
            }
            return false;
        }

        public static void UnSubscribeFromIssue(string IssueNumber)
        {
            string FileLocation = IssueFilesDirectory + IssueNumber + IssueTrackerFileExtension;
            File.Delete(FileLocation);
        }

        public static SubscribeIssue GetSubscribedIssue(string IssueFilePath)
        {
            return JsonConvert.DeserializeObject<SubscribeIssue>(File.ReadAllText(IssueFilePath));
        }

        public static string GetIssueNumber(string IssueLink)
        {
            return IssueLink.Replace(@"https://issues.unrealengine.com/issue/", "");
        }

        public static string GetIssueFilesDirectory()
        {
            return IssueFilesDirectory;
        }

        public static string GetIssuesHistoryFile()
        {
            return IssueTrackerHistoryFile;
        }

        public static SolidColorBrush ToSolidColorBrush(string hex_code)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(hex_code);
        }

        public static IssueResolution GetResolution(string ResolutionName)
        {
            IssueResolution issueResolution = IssueResolution.None;
            switch (ResolutionName.ToLower())
            {
                case "backlogged":
                    issueResolution = IssueResolution.Backlogged;
                    break;
                case "by design":
                    issueResolution = IssueResolution.ByDesign;
                    break;
                case "cannot reproduce":
                    issueResolution = IssueResolution.CannotReproduce;
                    break;
                case "duplicate":
                    issueResolution = IssueResolution.Duplicate;
                    break;
                case "fixed":
                    issueResolution = IssueResolution.Fixed;
                    break;
                case "non-issue":
                    issueResolution = IssueResolution.NonIssue;
                    break;
                case "unresolved":
                    issueResolution = IssueResolution.Unresolved;
                    break;
                case "won't do":
                    issueResolution = IssueResolution.WontDo;
                    break;
                case "won't fix":
                    issueResolution = IssueResolution.WontFix;
                    break;
                default:
                    break;

            }
            return issueResolution;
        }

        public static SolidColorBrush GetResolutionColor(string ResolutionName)
        {
            return GetColorByIssueResolution(GetResolution(ResolutionName));
        }

        private static SolidColorBrush GetColorByIssueResolution(IssueResolution issueResolution)
        {
            string hex_code = "#FF304FFE";
            switch(issueResolution)
            {
                case IssueResolution.Backlogged:
                    hex_code = "#FF7F261D";
                    break;
                case IssueResolution.ByDesign:
                    hex_code = "#FF27AE60";
                    break;
                case IssueResolution.CannotReproduce:
                    hex_code = "#FFF39C12";
                    break;
                case IssueResolution.Duplicate:
                    hex_code = "#FF95A5A6";
                    break;
                case IssueResolution.Fixed:
                    hex_code = "#FF2ECC71";
                    break;
                case IssueResolution.NonIssue:
                    hex_code = "#FFBBBBBB";
                    break;
                case IssueResolution.Unresolved:
                    hex_code = "#FFE74C3C";
                    break;
                case IssueResolution.WontDo:
                    hex_code = "#FF171717";
                    break;
                case IssueResolution.WontFix:
                    hex_code = "#FF34495E";
                    break;
                default:
                    break;

            }
            return (SolidColorBrush)new BrushConverter().ConvertFromString(hex_code);
        }

        public static string RemoveHtmlTags(string html)
        {
            string ReturnValue = html.Replace(@"amp;", string.Empty);
            Regex.Replace(ReturnValue, "<.+?>", string.Empty);
            return ReturnValue;
        }
    }

    public class SubscribeIssue
    {
        public string SubscribedIssueID { get; set; }
        public string SubscribedIssueTitle { get; set; }
        public string SubscribedIssueResolution { get; set; }
        public string SubscribedIssueLink { get; set; }
        public string SubscribedIssueDescription { get; set; }
        public string SubscribedIssueRepro { get; set; }
        public string SubscribedIssueMoreInfo { get; set; } // AnswerHub or UDN link.
    }

    public class HistoryIssue
    {
        public Dictionary<string, string> IssueHistory = new Dictionary<string, string>();
    }
}
