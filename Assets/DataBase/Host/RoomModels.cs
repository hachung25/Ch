using System;
using System.Collections.Generic;

[Serializable] public class PlayerInfo { public string name; public long joinedAt; public bool isHost; }
[Serializable] public class SceneEvent { public int index; public bool trigger; }

[Serializable]
public class RoomInfo
{
    public string hostUid;
    public string hostToken;   // tùy chọn, có thể lưu uid host
    public string status;      // "lobby" | "loading" | "started"
    public SceneEvent sceneToLoad;
    public Dictionary<string, PlayerInfo> players;
}
