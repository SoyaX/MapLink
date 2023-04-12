using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace MapMate; 

public class MainWindow : Window {
    public MainWindow() : base("Map Mate") {
    }

    public override void PreDraw() {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
    }

    public override void PostDraw() {
        ImGui.PopStyleVar();
    }
    
    public void DrawCharacterRow(string name, long contentId) {
        // Character Name
        ImGui.TableNextColumn();
        ImGui.Dummy(ImGuiHelpers.ScaledVector2(1, 24));
        ImGui.SameLine();
                
        ImGui.Text(name);
                
        if (!Api.TryGetMapSpotByContentId(contentId, out var spot) || spot == null) {
            ImGui.TableNextColumn();
            ImGui.TextDisabled("Not Reported");
                    
            ImGui.TableNextRow();
            return;
        };

        var link = spot.CreateMapLink();
        if (link == null) {
            ImGui.TableNextRow();
            return;
        }
        
        // Zone
        ImGui.TableNextColumn();
        ImGui.Text(link.PlaceName);
        ImGui.SameLine();

        ImGui.Text(link.CoordinateString);
                
        // Buttons
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
                
                
        ImGui.PushFont(UiBuilder.IconFont);
        var clicked = ImGui.Button($"{(char)FontAwesomeIcon.MapPin}##{contentId:X16}", ImGuiHelpers.ScaledVector2(24));
        ImGui.PopFont();
        if (clicked) GameGui.OpenMapWithMapLink(link);
    }
    
    public override void Draw() {
        if (ImGui.BeginTable("MapMateDisplay", 3)) {
            
            ImGui.TableSetupColumn("  Character", ImGuiTableColumnFlags.WidthFixed, 180 * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("Map Location", ImGuiTableColumnFlags.WidthFixed, 250 * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("##Links", ImGuiTableColumnFlags.WidthFixed, 24 * ImGuiHelpers.GlobalScale);
            
            ImGui.TableHeadersRow();
            if (PartyList.Length == 0 && ClientState.LocalPlayer != null) {
                DrawCharacterRow(ClientState.LocalPlayer.Name.TextValue, (long)ClientState.LocalContentId);
            } else {
                foreach (var character in PartyList) {
                    DrawCharacterRow(character.Name.TextValue, character.ContentId);
                }
            }
            ImGui.EndTable();
        }
    }
}
