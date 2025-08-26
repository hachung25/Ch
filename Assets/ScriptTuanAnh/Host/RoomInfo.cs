using System;
using System.Collections.Generic;

[Serializable] public class PlayerInfo { public string name; public long joinedAt; public bool isHost; public int selectedIndex = -1; }

    // Bổ sung roundId + triggerAt để phân biệt đợt start
    [Serializable]
public class SceneEvent
{
    public int index;
    public bool trigger;
    public string roundId;   // GUID cho mỗi lần start
    public long triggerAt;   // Unix ms
}

[Serializable]
public class RoomInfo
{
    public string hostUid;
    public string hostToken;   // nếu dùng "secret host" thì đặt random; nếu không dùng, có thể để trống
    public string status;      // "lobby" | "loading" | "started"
    public SceneEvent sceneToLoad;
    public Dictionary<string, PlayerInfo> players;
}

