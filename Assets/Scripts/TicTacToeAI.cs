// Author: Sean Liu
// Date: September 3, 2024

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState { none, cross, circle }

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{
    int _aiLevel;
    TicTacToeState[,] boardState;

    [SerializeField]
    private bool _isPlayerTurn;

    [SerializeField]
    private int _gridSize = 3;

    [SerializeField]
    private TicTacToeState playerState = TicTacToeState.circle;  // Player uses circles (O)
    private TicTacToeState aiState = TicTacToeState.cross;  // AI uses crosses (X)

    [SerializeField]
    private GameObject _xPrefab;

    [SerializeField]
    private GameObject _oPrefab;

    public UnityEvent onGameStarted;

    // Call this event with the player number to denote the winner
    public WinnerEvent onPlayerWin;

    ClickTrigger[,] _triggers;

    private void Awake()
    {
        if (onPlayerWin == null)
        {
            onPlayerWin = new WinnerEvent();
        }
    }

    public void StartAI(int AILevel)
    {
        _aiLevel = AILevel;
        StartGame();
    }

    public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
    {
        _triggers[myCoordX, myCoordY] = clickTrigger;
    }

    private void StartGame()
    {
        _triggers = new ClickTrigger[_gridSize, _gridSize];
        boardState = new TicTacToeState[_gridSize, _gridSize];
        _isPlayerTurn = true;  // Ensure the player starts first
        onGameStarted.Invoke();
    }

    public void PlayerSelects(int coordX, int coordY)
    {
        if (_isPlayerTurn && boardState[coordX, coordY] == TicTacToeState.none)
        {
            SetVisual(coordX, coordY, playerState);
            boardState[coordX, coordY] = playerState;

            if (CheckForWin(playerState))
            {
                onPlayerWin.Invoke(0);  // Player wins
            }
            else if (CheckForTie())
            {
                onPlayerWin.Invoke(-1); // It's a tie
            }
            else
            {
                _isPlayerTurn = false;
                StartCoroutine(AiMoveWithDelay());
            }
        }
    }

    private IEnumerator AiMoveWithDelay()
    {
        float delay = UnityEngine.Random.Range(0.5f, 1.0f);  // Random delay between 0.5 and 1 second
        yield return new WaitForSeconds(delay);
        AiMove();
    }

    public void AiMove()
    {
        // Simple AI: Block player or choose the first available spot
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                // Try to block player's winning move
                if (boardState[i, j] == TicTacToeState.none)
                {
                    boardState[i, j] = playerState;
                    if (CheckForWin(playerState))
                    {
                        boardState[i, j] = aiState;
                        SetVisual(i, j, aiState);
                        if (CheckForWin(aiState))
                        {
                            onPlayerWin.Invoke(1);  // AI wins
                        }
                        else if (CheckForTie())
                        {
                            onPlayerWin.Invoke(-1); // It's a tie
                        }
                        _isPlayerTurn = true;
                        return;
                    }
                    boardState[i, j] = TicTacToeState.none;
                }
            }
        }

        // If no blocking, pick the first available spot
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                if (boardState[i, j] == TicTacToeState.none)
                {
                    boardState[i, j] = aiState;
                    SetVisual(i, j, aiState);
                    if (CheckForWin(aiState))
                    {
                        onPlayerWin.Invoke(0);  // AI wins
                    }
                    else if (CheckForTie())
                    {
                        onPlayerWin.Invoke(-1); // It's a tie
                    }
                    _isPlayerTurn = true;
                    return;
                }
            }
        }
    }

    private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
    {
        Instantiate(
            targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
            _triggers[coordX, coordY].transform.position,
            Quaternion.identity
        );
    }

    private bool CheckForWin(TicTacToeState state)
    {
        // Check rows, columns, and diagonals for a win
        for (int i = 0; i < _gridSize; i++)
        {
            if (CheckLine(state, i, 0, 0, 1) || CheckLine(state, 0, i, 1, 0))
            {
                return true;
            }
        }
        if (CheckLine(state, 0, 0, 1, 1) || CheckLine(state, 0, _gridSize - 1, 1, -1))
        {
            return true;
        }
        return false;
    }

    private bool CheckLine(TicTacToeState state, int startX, int startY, int dx, int dy)
    {
        for (int i = 0; i < _gridSize; i++)
        {
            if (boardState[startX + i * dx, startY + i * dy] != state)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckForTie()
    {
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                if (boardState[i, j] == TicTacToeState.none)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
