namespace SMB3Explorer.Models.Internal;

public class TeamConfigurationSelection
{
    public int id {get; set; }
    public string name { get; set; } = string.Empty;
    public string DisplayText => $"{id} - {name}";
}
