using System.Collections.Generic;
using Fusion;

public static class FusionIdentityBridge
{
    // Map người chơi trong Fusion -> Firebase UID và characterIndex
    public static readonly Dictionary<PlayerRef, string> PlayerToFirebaseUid = new();
    public static readonly Dictionary<PlayerRef, int> PlayerToCharIndex = new();

    public static void Clear(PlayerRef player)
    {
        PlayerToFirebaseUid.Remove(player);
        PlayerToCharIndex.Remove(player);
    }
}
