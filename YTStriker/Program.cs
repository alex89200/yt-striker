using System;
using System.Collections.Generic;
using CommandLine;
using YTStriker.Helpers;
using YTStriker.Model;
using YTStriker.ReportStrategies;

namespace YTStriker
{
    class Program
    {
        private static void Main(string[] args)
        {
#if DEBUG
            args = GetDebugArgs(ReportMode.videos, "TARAFTARKANALIHD", 2, 0, 5, WebBrowser.edge, false);
            //args = GetDebugArgs(ReportMode.channel, "TARAFTARKANALIHD", 5, -1, 20, WebBrowser.edge, false);
#endif

            CommandLineArguments parsedArgs;
            IEnumerable<Error> errors;
            if (TryParseArgs(args, out parsedArgs, out errors))
            {
                ILogger logger = new ConsoleLogger(parsedArgs.Verbose);
                IReportStrategy proc; 
                switch (parsedArgs.Mode)
                {
                    case ReportMode.channel:
                        // Select violence as a default option
                        if (parsedArgs.MainComplaint == -1)
                            parsedArgs.MainComplaint = 5;

                        proc = new ReportChannelStrategy(parsedArgs, logger);
                        break;

                    case ReportMode.videos:
                        // Select violence as a default option
                        if (parsedArgs.MainComplaint == -1)
                            parsedArgs.MainComplaint = 2;

                        // Select first sub-option as a default
                        if (parsedArgs.SubComplaint == -1)
                            parsedArgs.SubComplaint = 0;

                        proc = new ReportVideosStrategy(parsedArgs, logger);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(parsedArgs.Mode), "Unknown mode");
                }

                proc.Process().Wait();

                Console.WriteLine("DONE\n\nPress any key to close the window...");
                Console.ReadKey();
            }
            else
            {
                Console.Error.Write("Please check passed parameters and try again.");
                Console.ReadKey();
            }
        }

        private static bool TryParseArgs(IEnumerable<string> args, out CommandLineArguments parsedArgs, out IEnumerable<Error> errors)
        {
            ParserResult<CommandLineArguments> argsRaw = CommandLine.Parser.Default.ParseArguments<CommandLineArguments>(args);
            parsedArgs = (argsRaw as Parsed<CommandLineArguments>)?.Value;
            errors = (argsRaw as NotParsed<CommandLineArguments>)?.Errors;
            
            return parsedArgs != null;
        }

        private static string[] GetDebugArgs(ReportMode mode, string channel, int option = 2, int subOption = 0, int limit = 50, WebBrowser browser = WebBrowser.chrome, bool verbose = false, bool dryRun = true)
        {
            return new string[]
            {
                "-m", mode.ToString(),
                "-c", channel,
                "-l", limit.ToString(),
                "-b", browser.ToString(),
                "-i", option.ToString(),
                "-o", subOption.ToString(),
                (verbose ? "-v" : ""),
                (dryRun ? "--dry-run" : "")
            };
        }
    }
}
