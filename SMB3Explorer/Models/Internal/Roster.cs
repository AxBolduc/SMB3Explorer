using System.Collections.Generic;

namespace SMB3Explorer.Models.Internal;

public record Roster
{
    public string TeamName { get; set; } = string.Empty;

    public List<Player> Players { get; set; } = new();
}
