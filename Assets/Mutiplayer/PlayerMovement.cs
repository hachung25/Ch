using UnityEngine;
using Fusion;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority)
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            transform.Translate(input * moveSpeed * Runner.DeltaTime);
        }
    }
}
