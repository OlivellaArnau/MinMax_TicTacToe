using System.Collections;
using UnityEngine;

public enum States
{
    CanMove,
    CantMove
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public BoxCollider2D collider;
    public GameObject token1, token2;
    public int Size = 3;
    public int[,] Matrix;
    private int currentStartingPlayer = -1; // -1 = IA, 1 = Jugador
    [SerializeField] private States state = States.CanMove;
    public Camera camera;

    void Start()
    {
        Instance = this;
        Matrix = new int[Size, Size];
        Calculs.CalculateDistances(collider, Size);
        ResetGame();
    }

    void Update()
    {
        if (state == States.CanMove)
        {
            Vector3 m = Input.mousePosition;
            m.z = 10f;
            Vector3 mousepos = camera.ScreenToWorldPoint(m);
            if (Input.GetMouseButtonDown(0))
            {
                if (Calculs.CheckIfValidClick((Vector2)mousepos, Matrix))
                {
                    state = States.CantMove;
                    if (Calculs.EvaluateWin(Matrix) == 2)
                        StartCoroutine(WaitingABit());
                }
            }
        }
    }

    private IEnumerator WaitingABit()
    {
        yield return new WaitForSeconds(1f);
        MinMaxAI();
    }

    public void MinMaxAI()
    {
        Vector2Int move = Calculs.GetBestMove(Matrix);
        if (move.x != -1 && move.y != -1)
        {
            DoMove(move.x, move.y, -1);
            state = States.CanMove;
        }
    }

    public void DoMove(int x, int y, int team)
    {
        Matrix[x, y] = team;
        Instantiate(team == 1 ? token1 : token2, Calculs.CalculatePoint(x, y), Quaternion.identity);

        int result = Calculs.EvaluateWin(Matrix);
        switch (result)
        {
            case 0:
                Debug.Log("Draw");
                StartCoroutine(RestartAfterDelay());
                break;
            case 1:
                Debug.Log("You Win");
                StartCoroutine(RestartAfterDelay());
                break;
            case -1:
                Debug.Log("You Lose");
                StartCoroutine(RestartAfterDelay());
                break;
            case 2:
                if (state == States.CantMove)
                    state = States.CanMove;
                break;
        }

    }
    public void ResetGame()
    {
        // Borrar todas las fichas del tablero
        foreach (var token in GameObject.FindGameObjectsWithTag("Token"))
        {
            Destroy(token);
        }

        // Reiniciar matriz
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                Matrix[i, j] = 0;

        // Cambiar quién empieza
        currentStartingPlayer *= -1;

        state = States.CantMove;

        if (currentStartingPlayer == -1)
        {
            // Si empieza la IA, esperar un momento y hacer el primer movimiento
            StartCoroutine(FirstMoveByAI());
        }
        else
        {
            // Si empieza el jugador
            state = States.CanMove;
        }
    }

    private IEnumerator FirstMoveByAI()
    {
        yield return new WaitForSeconds(0.5f);
        MinMaxAI();
    }
    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        ResetGame();
    }

}
