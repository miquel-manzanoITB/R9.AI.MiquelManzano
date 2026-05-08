using UnityEngine;

// Controlador de movimiento básico para probar el AI
[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;

    private CharacterController _cc;
    private float _velocityY;

    void Start() => _cc = GetComponent<CharacterController>();

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;

        if (_cc.isGrounded) _velocityY = -1f;
        _velocityY += gravity * Time.deltaTime;
        move.y = _velocityY;

        _cc.Move(move * speed * Time.deltaTime);
    }
}