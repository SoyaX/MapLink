using System.Numerics;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using ImGuiNET;

namespace MapMate; 

public unsafe class DebugWindow : Window {
    public DebugWindow() : base(nameof(MapMate) + " - DEBUG") {
        this.SizeConstraints = new WindowSizeConstraints() { MinimumSize = new Vector2(200, 400), MaximumSize = new Vector2(float.MaxValue) };
    }
    
    public override void Draw() {

        ImGui.Separator();
        ImGui.Text("Current Map: ");
        ImGui.Indent();
        if (Logic.TryGetCurrentTreasureSpot(out var spot)) {
            var link = spot.CreateMapLink();
            if (link != null) {
                if (ImGui.Button("Show on Map")) GameGui.OpenMapWithMapLink(link);
                ImGui.SameLine();
                ImGui.Text($"{spot.RowId}:{spot.SubRowId} => {link.XCoord:F1}, {link.YCoord:F1}");
            }

        } else {
            ImGui.Text("None Found");
        }
        ImGui.Unindent();
        
        ImGui.Text("API:");
        
        ImGui.Text($"HashedContentId: {Api.GetHashedContentId(ClientState.LocalContentId)}");

        if (ImGui.Button("UPDATE")) {
            Api.UpdateOwnMap();
        }
        
        ImGui.Separator();
        ImGui.Text("Party:");

        var groupManager = GroupManager.Instance();
        if (groupManager != null) {
            ImGui.Text($"Party ID: {groupManager->PartyId} : {groupManager->PartyId_2}");
        }
        
        
        ImGui.SameLine();
        if (ImGui.SmallButton("Force Refresh")) {
            Api.UpdateParty();
        }
        
        
        if (ImGui.BeginTable("partyDebug", 4)) {

            foreach (var partyMember in PartyList) {
                if (partyMember.ContentId == (long) ClientState.LocalContentId) continue;
                ImGui.TableNextColumn();
                ImGui.Text($"{partyMember.Name.TextValue}");
                ImGui.TableNextColumn();
                ImGui.Text($"{partyMember.ContentId:X16}");
                ImGui.Text($"{Api.GetHashedContentId(partyMember.ContentId)}");
                ImGui.TableNextColumn();

                if (Api.PartyMaps.TryGetValue(Api.GetHashedContentId(partyMember.ContentId), out var partySpot)) {
                    if (partySpot != null) {
                        ImGui.Text($"({partySpot.RowId}, {partySpot.SubRowId})");
                        ImGui.TableNextColumn();
                        var link = partySpot.CreateMapLink();
                        if (link != null && ImGui.Button($"Show on Map##{partyMember.ContentId:X16}")) {
                            GameGui.OpenMapWithMapLink(link);
                        }
                    } else {
                        ImGui.Text("No Map Reported");
                        ImGui.TableNextColumn();
                    }
                    
                } else {
                    ImGui.Text("Not Fetched");
                    ImGui.TableNextColumn();
                    
                }
                
                ImGui.TableNextColumn();
            }
            
            
            ImGui.EndTable();
        }
        
        
        
        
        
        
        
    }
}
