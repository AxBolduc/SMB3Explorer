using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMB3Explorer.Models.Internal;

public record TeamSelection
{
    public string id { get; set; } = string.Empty;
    public string teamName { get; set; } = string.Empty;

    public string DisplayText => $"{teamName}";
}

