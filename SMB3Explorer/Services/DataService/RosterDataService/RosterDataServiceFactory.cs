using System;
using System.Collections.Generic;
using System.Linq;
using SMB3Explorer.Enums;

namespace SMB3Explorer.Services.DataService.RosterDataService;

public class RosterDataServiceFactory : IRosterDataServiceFactory
{
    private readonly Func<IEnumerable<IRosterDataService>> _func;

    public RosterDataServiceFactory(Func<IEnumerable<IRosterDataService>> func)
    {
        _func = func;
    }

    public IRosterDataService create(SelectedGame? game)
    {
        IEnumerable<IRosterDataService> dataServices = _func();
        IRosterDataService? correctDataService = dataServices.First(x => x.GameType.Equals(game));

        if (correctDataService == null)
        {
            throw new NotImplementedException();
        }

        return correctDataService;
    }
}
