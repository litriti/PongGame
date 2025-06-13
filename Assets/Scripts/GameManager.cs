using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int player1Score = 0;
    public int player2Score = 0;

    public Text player1Text;
    public Text player2Text;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Player1Scores()
    {
        player1Score++;
        player1Text.text = player1Score.ToString();
    }

    public void Player2Scores()
    {
        player2Score++;
        player2Text.text = player2Score.ToString();
    }

    public void ResetBall(GameObject ball)
    {
        ball.GetComponent<Ball>().ResetBall();
    }
}
