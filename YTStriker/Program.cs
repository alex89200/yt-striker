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
            args = GetDebugArgs(ReportMode.videos, "RT", 2, 0, 20, WebBrowser.edge, true);
#endif

            CommandLineArguments parsedArgs;
            IEnumerable<Error> errors;
            if (TryParseArgs(args, out parsedArgs, out errors))
            {
                Logger logger = new Logger(parsedArgs.Verbose);
                IReportStrategy proc; 
                switch (parsedArgs.Mode)
                {
                    case ReportMode.channel:
                        proc = new ReportChannelStrategy(parsedArgs, logger);
                        break;

                    case ReportMode.videos:
                        proc = new ReportVideosStrategy(parsedArgs, logger);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                proc.Process();
                
                Console.WriteLine("Processing done");
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

        private static string[] GetDebugArgs(ReportMode mode, string channel, int option = 2, int subOption = 0, int limit = 50, WebBrowser browser = WebBrowser.chrome, bool verbose = false)
        {
            return new string[]
            {
                "-m", mode.ToString(),
                "-c", channel,
                "-l", limit.ToString(),
                "-b", browser.ToString(),
                "-i", option.ToString(),
                "-o", subOption.ToString(),
                (verbose ? "-v" : "")
            };
        }
    }
}
