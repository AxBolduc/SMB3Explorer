using SMB3Explorer.Enums;

namespace SMB3Explorer.Services.DataService.RosterDataService;

public interface IRosterDataServiceFactory
{
    IRosterDataService create(SelectedGame? game);
}
