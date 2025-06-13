using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 8f;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Launch();
    }

    void Launch()
    {
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(-1f, 1f);
        rb.linearVelocity = new Vector2(x, y).normalized * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Tăng nhẹ tốc độ khi chạm paddle
        if (collision.gameObject.CompareTag("Paddle"))
        {
            rb.linearVelocity *= 1.05f;
        }
    }

    public void ResetBall()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = Vector2.zero;
        Invoke("Launch", 1f); // delay rồi bắn lại
    }
}
