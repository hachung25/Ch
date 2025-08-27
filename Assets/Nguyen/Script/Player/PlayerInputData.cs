using Fusion;

public struct PlayerInputData : INetworkInput
{
    public float Horizontal;                 // -1..1
    public NetworkBool JumpPressed;          // edge (GetKeyDown)
    public NetworkBool AttackPressed;        // edge (GetKeyDown)
    public NetworkBool AttackHeld;           // giữ phím T
}
