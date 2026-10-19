using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;


namespace SamplePlugin;
private void OnFrameworkUpdate(IFramework framework)
{
    if (!ClientState.IsLoggedIn) return;

    int currentPartySize = PartyList.Length; 

    if (currentPartySize == 8)
    {
        if (!wasPartyFullLastFrame)
        {
            wasPartyFullLastFrame = true;
            
            Task.Run(async () => {
                await SendDiscordNotificationAsync("we filled btw");
            });
        }
    }
    else
    {
        wasPartyFullLastFrame = false;
    }
}
private async Task SendDiscordNotificationAsync(string message)
{
    string webhookUrl = Configuration.DiscordWebhookUrl;

    if (string.IsNullOrWhiteSpace(webhookUrl))
    {
        Log.Warning("you didnt enter the link dipshit");
        return;
    }

    try
    {
        var payload = new { content = message };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(webhookUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            Log.Error($"Failed to send Discord ping. Status: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while sending the Discord webhook request.");
    }
}

public sealed class Plugin : IDalamudPlugin
{

    private bool wasPartyFullLastFrame = false;
    private static readonly HttpClient httpClient = new();

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/pmycommand";

    Framework.Update += OnFrameworkUpdate;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });
        CommandManager.AddHandler("/ptestping", new CommandInfo(OnTestCommand) { HelpMessage = "TestTEST123123" });

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
	Framework.Update -= OnFrameworkUpdate;
    CommandManager.RemoveHandler("/ptestping");

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
    private void OnTestCommand(string command, string args)
    {
        Log.Information("Sending test ping to Discord...");
        _ = SendDiscordNotificationAsync("we filled btw hey its me dd im testing hahahahahahahahahaha");
    }
}
