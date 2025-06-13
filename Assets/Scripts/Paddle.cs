using UnityEngine;

public class Paddle : MonoBehaviour
{
    public float speed = 10f;
    public string inputAxis;

    private void Update()
    {
        float move = Input.GetAxisRaw(inputAxis);
        transform.Translate(Vector2.up * move * speed * Time.deltaTime);
    }
}
