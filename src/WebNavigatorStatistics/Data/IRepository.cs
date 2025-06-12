using WebNavigatorStatistics.Models;

namespace WebNavigatorStatistics.Data
{
    public interface IRepository
    {
        List<Statistics> GetStatistics();
        void Update(Statistics statistics);
    }
}