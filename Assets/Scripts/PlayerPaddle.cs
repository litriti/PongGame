using UnityEngine;

public class PlayerPaddle : MonoBehaviour
{
    public float moveSpeed = 10f;
    public string inputAxis = "Vertical";
    public float minY = -4f; // Tùy theo vị trí tường dưới
    public float maxY = 4f;  // Tùy theo vị trí tường trên

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float move = Input.GetAxisRaw(inputAxis);
        rb.linearVelocity = new Vector2(0, move * moveSpeed);

        // Giới hạn vị trí trục Y
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
    }
}
