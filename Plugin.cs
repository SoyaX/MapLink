global using static MapLink.Plugin;
using System.Diagnostics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Condition = Dalamud.Game.ClientState.Conditions.Condition;

namespace MapLink;

public class Plugin : IDalamudPlugin {
    public string Name => "Map Link";

    
    [PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static DataManager DataManager { get; private set; } = null!;
    [PluginService] internal static GameGui GameGui { get; private set; } = null!;
    [PluginService] internal static ClientState ClientState { get; private set; } = null!;
    [PluginService] internal static Framework Framework { get; private set; } = null!;
    [PluginService] internal static Condition Condition { get; private set; } = null!;
    [PluginService] internal static PartyList PartyList { get; private set; } = null!;
    [PluginService] internal static CommandManager CommandManager { get; private set; } = null!;

    private WindowSystem WindowSystem { get; } = new(typeof(Plugin).FullName);
    internal static DebugWindow DebugWindow { get; } = new();
    internal static MainWindow MainWindow { get; } = new();
    
    private Stopwatch UpdateThrottle = Stopwatch.StartNew();
    
    
    public Plugin() {
        WindowSystem.AddWindow(DebugWindow);
        WindowSystem.AddWindow(MainWindow);
#if DEBUG
        DebugWindow.IsOpen = true;
#endif
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Framework.Update += FrameworkUpdate;
        Condition.ConditionChange += ConditionOnConditionChange;

        CommandManager.AddHandler("/maplink", new CommandInfo(CommandHandle));
    }

    private void CommandHandle(string command, string arguments) {
        switch (arguments.ToLower()) {
            case "debug": {
                DebugWindow.Toggle();
                return;
            }
            default: {
                MainWindow.Toggle();
                return;
            }
        }
    }

    private void ConditionOnConditionChange(ConditionFlag flag, bool value) {
        if (flag == ConditionFlag.LoggingOut && value) Api.Delete();
    }

    public void FrameworkUpdate(Framework framework) {
        if (UpdateThrottle.ElapsedMilliseconds < 1000) return;
        if (Api.LastReported.ElapsedMilliseconds < 5000) return;
        
        if (Api.RequestUpdate) {
            Api.LastReported.Restart();
            Api.RequestUpdate = false;
            Api.UpdateOwnMap();
            Api.UpdateParty();
            return;
        }
        
        if (Api.LastReported.ElapsedMilliseconds > 60000) {
            Api.RequestUpdate = true;
            return;
        }

        if (Logic.TryGetCurrentTreasureSpot(out var spot)) {
            if (Api.LastReportedMap != (spot.RowId, spot.SubRowId)) {
                Api.RequestUpdate = true;
            }
        } else if (Api.LastReportedMap != null) {
            Api.RequestUpdate = true;
        }
        
    }
    
    public void Dispose() {
        CommandManager.RemoveHandler("/maplink");
        Condition.ConditionChange -= ConditionOnConditionChange;
        Framework.Update -= FrameworkUpdate;
        WindowSystem.RemoveAllWindows();
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        Api.Delete();
    }
}
