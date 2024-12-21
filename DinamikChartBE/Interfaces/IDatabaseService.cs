using DinamikChartBE.Models;

namespace DinamikChartBE.Interfaces
{
    public interface IDatabaseService
    {
        bool TestAndSaveConnection(ConnectionDetails connectionDetails, ISession session);

        List<string> RetrieveStoredProcedures(ISession session);

        List<dynamic> ExecuteStoredProcedure(string procedureIdentifier, ISession session);

        List<string> RetrieveViews(ISession session);

        List<dynamic> ExecuteViewQuery(string viewIdentifier, ISession session);
    }
}
