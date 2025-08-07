using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class DeviceConflictManager : MonoBehaviour
{
    private DatabaseReference reference;
    

    private void Awake()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;

        // 🔒 Tắt chế độ lưu offline (tránh lỗi LOCK)
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);

        
    }

    private void Start()
    {
        TilemapDetail tilemapDetail = new TilemapDetail(1, 1, TilemapSate.Tilemap);

        WriteDataBase("123", tilemapDetail.ToString());

        ReadDataBase("123");
    }

    // ✅ Ghi toàn bộ dữ liệu User (ví dụ: class User → ToString)
    public void WriteUserData(string id, string message)
    {
        reference.Child("Users").Child(id).SetValueAsync(message).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("✅ Ghi dữ liệu thành công.");
            }
            else
            {
                Debug.LogError("❌ Ghi dữ liệu thất bại: " + task.Exception);
            }
        });
    }

    // ✅ Ghi giá trị đơn vào path cụ thể (ví dụ: Users/userId/deviceId)
    public void WriteDataBase(string path, string message)
    {
        reference.Child(path).SetValueAsync(message).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("✅ Ghi dữ liệu thành công tại path: " + path);
            }
            else
            {
                Debug.LogError("❌ Ghi dữ liệu thất bại tại path " + path + ": " + task.Exception);
            }
        });
    }


    // ✅ Đọc path tùy ý, trả về string (dùng cho: Users/userId/deviceId...)
    public void ReadDataBase(string path, System.Action<string> onResult)
    {
        reference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string value = snapshot?.Value?.ToString();
                onResult?.Invoke(value);
            }
            else
            {
                Debug.LogError("❌ Lỗi khi đọc dữ liệu: " + task.Exception);
                onResult?.Invoke(null);
            }
        });
    }

    // ✅ Chỉ dùng test debug toàn bộ node user
    public void ReadDataBase(string id)
    {
        reference.Child("User").Child(id).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("Doc du lieu thanh cong: " + snapshot.Value.ToString());
            }
            else
            {
                Debug.Log("Doc du lieu that bai: " + task.Exception);
            }
        });
    }
}
