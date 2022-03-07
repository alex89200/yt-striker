using System;

namespace YTStriker.Helpers
{
    public class ConsoleLogger : ILogger
    {
        private readonly bool _verboseEnabled;

        public ConsoleLogger(bool verbose)
        {
            _verboseEnabled = verbose;
        }

        public void Log(string message, bool verbose = false)
        {
            // If current message is not verbose or if global verbose is enabled
            if (!verbose || _verboseEnabled)
            {
                Console.WriteLine(message);
            }
        }
    }
}