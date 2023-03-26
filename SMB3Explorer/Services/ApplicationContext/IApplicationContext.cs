﻿using System.Collections.Concurrent;
using System.ComponentModel;
using SMB3Explorer.Models.Internal;

namespace SMB3Explorer.Services.ApplicationContext;

public interface IApplicationContext
{
    FranchiseSelection? SelectedFranchise { get; set; }
    bool IsFranchiseSelected { get; }
    ConcurrentBag<FranchiseSeason> FranchiseSeasons { get; }
    FranchiseSeason? MostRecentFranchiseSeason { get; }
    bool FranchiseSeasonsLoading { get; set; }
    event PropertyChangedEventHandler? PropertyChanged;
}