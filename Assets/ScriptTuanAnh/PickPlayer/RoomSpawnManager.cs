using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class RoomSpawnManager : MonoBehaviour
{
    [Header("Prefabs theo index đã chọn")]
    public GameObject[] characterPrefabs;

    [Header("Vị trí spawn (tùy chọn)")]
    public Transform[] spawnPoints;

    [Header("Nếu bạn CHƯA dùng networking, cho client cũng tự spawn để nhìn thấy")]
    public bool alsoSpawnOnClientsWhenNoNetworking = true;

    async void Start()
    {
        // Chỉ host spawn tất cả; nếu chưa có networking, có thể bật alsoSpawnOnClientsWhenNoNetworking
        string myUid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        bool iAmHost = (!string.IsNullOrEmpty(RoomService.I?.LastKnownHostUid) &&
                        RoomService.I.LastKnownHostUid == myUid);

        if (!iAmHost && !alsoSpawnOnClientsWhenNoNetworking)
        {
            Debug.Log("[RoomSpawnManager] Not host → skip spawning (waiting for networking to replicate).");
            return;
        }

        string roomId = RoomService.I?.CurrentRoomId;
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogWarning("[RoomSpawnManager] No room id.");
            return;
        }

        var playersSnap = await FirebaseDatabase.DefaultInstance
            .GetReference("rooms").Child(roomId).Child("players")
            .GetValueAsync();

        if (!playersSnap.Exists)
        {
            Debug.LogWarning("[RoomSpawnManager] No players node.");
            return;
        }

        int i = 0;
        foreach (var ch in playersSnap.Children)
        {
            var json = ch.GetRawJsonValue();
            var pinfo = string.IsNullOrEmpty(json) ? new PlayerInfo() : JsonUtility.FromJson<PlayerInfo>(json);

            int idx = (pinfo.selectedIndex >= 0) ? pinfo.selectedIndex : 0;
            if (characterPrefabs == null || characterPrefabs.Length == 0)
            {
                Debug.LogError("[RoomSpawnManager] characterPrefabs chưa gán!");
                return;
            }
            idx = Mathf.Clamp(idx, 0, characterPrefabs.Length - 1);

            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                var t = spawnPoints[i % spawnPoints.Length];
                if (t) { pos = t.position; rot = t.rotation; }
                else { pos = new Vector3(i * 2f, 0f, 0f); }
            }
            else
            {
                pos = new Vector3(i * 2f, 0f, 0f); // simple fallback
            }

            Instantiate(characterPrefabs[idx], pos, rot);
            i++;
        }

        Debug.Log($"[RoomSpawnManager] Spawned {i} players.");
    }
}
