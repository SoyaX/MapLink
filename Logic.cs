using System.Diagnostics.CodeAnalysis;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel.GeneratedSheets;

namespace MapLink; 

public static class Logic {
    public static bool TryGetCurrentTreasureSpot([NotNullWhen(true)] out TreasureSpot? spot) {
        spot = null;
        var treasureRank = GameFunction.GetCurrentTreasureHuntRank();
        if (treasureRank == 0) return false;
        var treasureSpot = GameFunction.GetCurrentTreasureHuntSpot();
        if (treasureSpot == 0) return false;
        spot = DataManager.GetExcelSheet<TreasureSpot>()?.GetRow(treasureRank, treasureSpot);
        return spot != null;
    }

    public static MapLinkPayload? CreateMapLink(this TreasureSpot? spot) {
        var location = spot?.Location?.Value;
        var map = location?.Map?.Value;
        var territory = map?.TerritoryType?.Value;
        if (territory == null || location == null || map == null) return null;

        var scale = map.SizeFactor / 100f;

        var xPosition = 41f / scale * ((location.X * scale + 1024f) / 2048f) + 1;
        var yPosition = 41f / scale * ((location.Z * scale + 1024f) / 2048f) + 1;
        return new MapLinkPayload(territory.RowId, map.RowId, xPosition, yPosition);
    }
}
