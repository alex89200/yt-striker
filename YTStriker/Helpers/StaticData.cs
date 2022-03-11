using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTStriker.Helpers
{
    public static class StaticData
    {
        public static readonly IReadOnlyList<string> ReportNames = new List<string>
        {
            "пожаловаться", "report", "поскаржитися"
        };

        
        public static readonly IReadOnlyList<string> ReportUserNames = new List<string>
        {
            "пожаловаться на пользователя", "report user", "поскаржитися на користувача"
        };
    }
}
