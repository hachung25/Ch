using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class DeviceConflictManager : MonoBehaviour
{
    private DatabaseReference reference;

    private void Awake()
    {
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void WriteDeviceStatus(string userId, string field, string value)
    {
        reference.Child($"deviceStatus/{userId}/{field}")
            .SetValueAsync(value)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log($"Ghi thành công {field} = {value}");
                else
                    Debug.LogError($"Lỗi ghi {field}: {task.Exception}");
            });
    }

    public void WriteFullDeviceInfo(string userId, string deviceId)
    {
        WriteDeviceStatus(userId, "deviceId", deviceId);
        WriteDeviceStatus(userId, "deviceName", SystemInfo.deviceName);
        WriteDeviceStatus(userId, "appVersion", Application.version);
        WriteDeviceStatus(userId, "lastOnline", System.DateTime.UtcNow.ToString("o"));
    }
}
