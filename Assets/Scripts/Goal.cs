using UnityEngine;

public class Goal : MonoBehaviour
{
    public bool isLeftGoal; // Nếu là goal bên trái thì điểm cho Player 2, ngược lại thì Player 1

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            if (isLeftGoal)
            {
                GameManager.Instance.Player2Scores();
            }
            else
            {
                GameManager.Instance.Player1Scores();
            }

            collision.GetComponent<Ball>().ResetBall();
        }
    }
}
