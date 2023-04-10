using Dalamud.Utility.Signatures;

namespace MapLink; 

public class GameFunction {

    private static GameFunction Instance { get; }

    static GameFunction() {
        Instance = new GameFunction();
        SignatureHelper.Initialise(Instance);
    }
    
    
    private GameFunction() { }

    private delegate uint GetCurrentTreasureHuntRankDelegate(nint a1);
    private delegate uint GetCurrentTreasureHuntSpotDelegate(nint a1);

    [Signature("E8 ?? ?? ?? ?? 0F B6 D0 45 0F B6 CE", ScanType = ScanType.Text)]
    private GetCurrentTreasureHuntRankDelegate getCurrentTreasureHuntRank = null!;
    
    [Signature("E8 ?? ?? ?? ?? 44 0F B7 C0 45 33 C9 0F B7 D3", ScanType = ScanType.Text)]
    private GetCurrentTreasureHuntSpotDelegate getCurrentTreasureHuntSpot = null!;

    [Signature("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 75 E4", Offset = 3, ScanType = ScanType.StaticAddress)]
    private nint treasureHuntManager = nint.Zero;

    public static uint GetCurrentTreasureHuntRank() => Instance.getCurrentTreasureHuntRank(Instance.treasureHuntManager);
    public static uint GetCurrentTreasureHuntSpot() => Instance.getCurrentTreasureHuntSpot(Instance.treasureHuntManager);
    
}
