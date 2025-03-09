using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Calculs
{
    public static float LinearDistance;
    public static Vector2 FirstPosition;
    private static float offset = 0.1f;

    public static void CalculateDistances(BoxCollider2D coll, float size)
    {
        LinearDistance = coll.size.x / size;
        FirstPosition = new Vector2(-size / 2f, size / 2f);
    }

    public static Vector2 CalculatePoint(int x, int y)
    {
        return FirstPosition + new Vector2(x * LinearDistance, -y * LinearDistance);
    }

    public static int EvaluateWin(int[,] matrix)
    {
        // Comprobamos filas y columnas
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            int rowSum = 0;
            int colSum = 0;
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                rowSum += matrix[i, j];
                colSum += matrix[j, i];
            }
            if (rowSum == 3 || colSum == 3) return 1;     // Jugador gana
            if (rowSum == -3 || colSum == -3) return -1;  // IA gana
        }

        // Comprobamos diagonales
        int diag1 = 0, diag2 = 0;
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            diag1 += matrix[i, i];
            diag2 += matrix[i, matrix.GetLength(0) - 1 - i];
        }
        if (diag1 == 3 || diag2 == 3) return 1;     // Jugador gana
        if (diag1 == -3 || diag2 == -3) return -1;  // IA gana

        // Comprobamos si aún hay espacios vacíos (partida sigue)
        foreach (int cell in matrix)
            if (cell == 0)
                return 2;

        return 0; // Empate
    }


    public static bool CheckIfValidClick(Vector2 mousePosition, int[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Vector2 point = CalculatePoint(i, j);
                if (Mathf.Abs(mousePosition.x - point.x) < LinearDistance / 2f - offset &&
                    Mathf.Abs(mousePosition.y - point.y) < LinearDistance / 2f - offset)
                {
                    if (matrix[i, j] == 0)
                    {
                        GameManager.Instance.DoMove(i, j, 1);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // Algoritmo MinMax con poda alfa-beta para tomar decisiones óptimas
    public static int MinMax(int[,] board, int depth, bool isMaximizing, int alpha, int beta)
    {
        int result = EvaluateWin(board);
        if (result != 2 || depth == 0)
        {
            // Invertimos puntuación para que IA (maximizador) obtenga valores altos en su victoria
            if (result == -1) return 10 + depth;   // IA gana
            if (result == 1) return -10 - depth;   // Jugador gana
            return 0; // Empate o sin movimientos
        }

        if (isMaximizing) // IA
        {
            int maxEval = int.MinValue;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = -1;
                        int eval = MinMax(board, depth - 1, false, alpha, beta);
                        board[i, j] = 0;
                        maxEval = Mathf.Max(maxEval, eval);
                        alpha = Mathf.Max(alpha, eval);
                        if (beta <= alpha)
                            return maxEval;
                    }
                }
            }
            return maxEval;
        }
        else // Jugador
        {
            int minEval = int.MaxValue;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = 1;
                        int eval = MinMax(board, depth - 1, true, alpha, beta);
                        board[i, j] = 0;
                        minEval = Mathf.Min(minEval, eval);
                        beta = Mathf.Min(beta, eval);
                        if (beta <= alpha)
                            return minEval;
                    }
                }
            }
            return minEval;
        }
    }

    public static Vector2Int GetBestMove(int[,] board)
    {
        int bestValue = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == 0)
                {
                    board[i, j] = -1;
                    int moveValue = MinMax(board, 5, false, int.MinValue, int.MaxValue);
                    board[i, j] = 0;

                    if (moveValue > bestValue)
                    {
                        bestValue = moveValue;
                        bestMove = new Vector2Int(i, j);
                    }
                }
            }
        }
        return bestMove;
    }
}
