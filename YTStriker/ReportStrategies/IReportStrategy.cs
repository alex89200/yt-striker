using System.Threading.Tasks;

namespace YTStriker.ReportStrategies
{
    public interface IReportStrategy
    {
        Task Process();
    }
}