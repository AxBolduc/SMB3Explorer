namespace SMB3Explorer.Models.Internal;

public record Player
{
    internal int id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    public string SecondaryPosition { get; set; } = string.Empty;
    public float PowerRating { get; set; }
    public float ContactRating { get; set; }
    public float MojoRating { get; set; }
    public float SpeedRating { get; set; }
    public float ArmRating { get; set; }
    public float FieldingRating { get; set; }
    public float VelocityRating { get; set; }
    public float JunkRating { get; set; }
    public float AccuracyRating { get; set; }

    public int GetId()
    {
        return id;
    }

    public bool IsPitcher => PrimaryPosition.Equals("P");
}
