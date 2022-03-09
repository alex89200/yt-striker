using CommandLine;

namespace YTStriker.Model
{
    public class CommandLineArguments
    {
        private const int VIDEOS_LIMIT = 30;

        [Option('m', "mode", Default = ReportMode.videos, Required = false,
            HelpText = "Processing mode. Possible values:\nchannel - report author of the channel\nvideos - report separate videos\nlogin - login to YouTube account.")]
        public ReportMode Mode { get; set; }

        [Option('b', "browser", Default = WebBrowser.edge, Required = false,
            HelpText = "Which browser to use.\nPossible values: chrome, firefox, edge.")]
        public WebBrowser Browser { get; set; }

        [Option('c', "channel", Required = false, Group = "input",
            HelpText = "Name of the channel to process.")]
        public string ChannelName { get; set; }

        [Option('f', "file", Required = false, Group = "input",
            HelpText = "File which contains URLs of YouTube channels to process. Each URL must be on a separate line.")]
        public string InputFile { get; set; }

        [Option('l', "limit", Default = VIDEOS_LIMIT, Required = false,
            HelpText = "Maximum number of videos to process on each channel. Parameter is omitted when in 'channel' mode (see -m parameter).")]
        public int Limit { get; set; }

        [Option('i', "complaint", Default = -1, Required = false,
            HelpText = "Index of the main complaint. Default is 'Violence'.")]
        public int MainComplaint { get; set; }

        [Option('o', "sub-complaint", Default = -1, Required = false,
            HelpText = "Index of the sub-complaint in a dropdown if relevant. Default is the first in the list.")]
        public int SubComplaint { get; set; }

        [Option('d', "desc", Default = "description.txt" , Required = false,
            HelpText = "Path to the file with text which will be used as a violation description in the report.")]
        public string DescriptionFile { get; set; }

        [Option('t', "timeout", Default = 60, Required = false, 
            HelpText = "Page loading timeout.")]
        public int Timeout { get; set; }

        [Option("dry-run", Default = false,
            HelpText = "Do the full flow, but don't press the 'Submit' button in the end.")]
        public bool DryRun { get; set; }

        [Option('v', "verbose", Default = false, Required = false,
            HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }
}
