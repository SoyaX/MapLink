using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;

namespace MapLink; 

public class MainWindow : Window {
    public MainWindow() : base("Map Link", ImGuiWindowFlags.AlwaysAutoResize) {
    }

    public override void PreDraw() {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
    }

    public override void PostDraw() {
        ImGui.PopStyleVar();
    }

    private string[] HiddenNames = new[] { "Member One", "Member Two", "Member Three", "Member Four", "Member Five", "Member Six", "Member Seven", "Member Eight" };
    
    public override void Draw() {

        if (ImGui.BeginTable("MapLinkDisplay", 3)) {
            
            ImGui.TableSetupColumn("  Character", ImGuiTableColumnFlags.WidthFixed, 180 * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("Map Location", ImGuiTableColumnFlags.WidthFixed, 250 * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("##Links", ImGuiTableColumnFlags.WidthFixed, 24 * ImGuiHelpers.GlobalScale);
            
            ImGui.TableHeadersRow();


            var i = 0;
            
            
            
            
            foreach (var character in PartyList) {
                // Character Name
                ImGui.TableNextColumn();
                ImGui.Dummy(ImGuiHelpers.ScaledVector2(1, 24));
                ImGui.SameLine();
                
                
                ImGui.Text(HiddenNames[i++ % HiddenNames.Length]);
                
                if (!Api.TryGetMapSpotByContentId(character.ContentId, out var spot) || spot == null) {
                    ImGui.TableNextColumn();
                    ImGui.TextDisabled("Not Reported");
                    
                    
                    ImGui.TableNextRow();
                    continue;
                };

                var link = spot.CreateMapLink();
                if (link == null) {
                    ImGui.TableNextRow();
                    continue;
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
                var clicked = ImGui.Button($"{(char)FontAwesomeIcon.MapPin}##{character.ContentId:X16}", ImGuiHelpers.ScaledVector2(24));
                ImGui.PopFont();
                if (clicked) GameGui.OpenMapWithMapLink(link);



            }
            
            
            
            ImGui.EndTable();
        }
        
        
    }
}
