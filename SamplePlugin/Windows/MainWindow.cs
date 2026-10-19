using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private string GoatImagePath;
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin) : base("Party Finder Notifier###MainNotifyWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 110),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw() {
	ImGui.Text($"Status: Monitoring Party Finder...");
        
        if (Plugin.ClientState.IsLoggedIn)
        {
            ImGui.Text($"Logged in as: {Plugin.ClientState.LocalPlayerName}");
        }

        ImGui.Spacing();

        if (ImGui.Button("Configure Webhook"))
        {
            Plugin.ToggleConfigUI();
        }
    }
}
