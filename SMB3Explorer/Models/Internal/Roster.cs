using System.Collections.Generic;
using Serilog;

namespace SMB3Explorer.Models.Internal;

public record Roster
{
    public string TeamName { get; set; } = string.Empty;

    public List<Player> Players { get; set; } = new();

    public void UpdatePitchers(List<Player>? pitchers)
    {
        if (pitchers == null)
        {
            Log.Debug("No pitchers to update");
            return;
        }

        foreach (Player pitcher in pitchers)
        {
            var rosterPitcher = Players.Find(p => p.id == pitcher.id);

            if (rosterPitcher == null)
            {
                Log.Debug($"Could not update pitcher with id {pitcher.id}, no pitcher with that id found in roster");
                continue;
            }

            Players.Remove(rosterPitcher);
            Players.Add(pitcher);
        }

    }
}
