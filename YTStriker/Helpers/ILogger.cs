namespace YTStriker.Helpers
{
    public interface ILogger
    {
        void Log(string message, bool verbose = false);
    }
}