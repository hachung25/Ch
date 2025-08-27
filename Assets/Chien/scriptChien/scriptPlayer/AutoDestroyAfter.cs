using UnityEngine;
using System;

public class AutoDestroyAfter : MonoBehaviour
{
    private float _time;
    private Action _onDone;
    private float _timer;

    public AutoDestroyAfter Init(float time, Action onDone)
    {
        _time = time;
        _onDone = onDone;
        return this;
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime; // dùng unscaled để không phụ thuộc Time.timeScale
        if (_timer >= _time)
        {
            _onDone?.Invoke();
            // Không tự Destroy ở đây vì chủ sở hữu muốn quản lý; hoặc bạn có thể Destroy(gameObject);
            // nhưng trong PlayerHealth.CleanupOverAudio đã hủy rồi, nên để trống.
            Destroy(this);
        }
    }
}

