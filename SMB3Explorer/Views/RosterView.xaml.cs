using System.Windows.Controls;
using Serilog;

namespace SMB3Explorer.Views;

public partial class RosterView : UserControl
{
    public RosterView()
    {
        InitializeComponent();
    }

    private void RosterGrid_OnRowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
    {
        Log.Debug("Edited");
    }
}
