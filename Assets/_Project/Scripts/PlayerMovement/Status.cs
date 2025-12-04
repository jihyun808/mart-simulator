// Status.cs
using UnityEngine;

public class Status : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
}