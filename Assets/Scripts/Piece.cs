using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    Pawn, Rook, Knight, Bishop, Queen, King
}
public class Piece : MonoBehaviour
{
    public bool isWhite;
    public Vector2Int boardPosition;
    private BoardManager boardManager;

    public PieceType pieceType;

    void Awake()
    {
        boardManager = Object.FindFirstObjectByType<BoardManager>();
    }

    public void SetPosition(Vector2Int pos)
    {
        boardPosition = pos;
        Vector2 worldPos = boardManager.BoardToWorldPos(pos);
        transform.position = new Vector3(worldPos.x, worldPos.y, 0);
    }

    private void OnMouseDown()
    {
        if (!boardManager.IsCurrentTurn(this)) return;

        boardManager.ClearHighlights();
        List<Vector2Int> moves = GetAvailableMoves();
        boardManager.HighlightTiles(moves, this);
    }

    public List<Vector2Int> GetAvailableMovesForCheckTest()
    {
        // 체킹용 간단한 가상 이동 (왕 공격 여부만 판단)
        switch (pieceType)
        {
            case PieceType.Pawn: return GetPawnMoves();
            case PieceType.Rook: return GetRookMoves();
            case PieceType.Bishop: return GetBishopMoves();
            case PieceType.Queen: return GetQueenMoves();
            case PieceType.King: return GetKingMoves();
            case PieceType.Knight: return GetKnightMoves();
            default: return new List<Vector2Int>();
        }
    }


    public List<Vector2Int> GetAvailableMoves()
    {
        switch (pieceType)
        {
            case PieceType.Pawn: return GetPawnMoves();
            case PieceType.Rook: return GetRookMoves();
            case PieceType.Bishop: return GetBishopMoves();
            case PieceType.Queen: return GetQueenMoves();
            case PieceType.King: return GetKingMoves();
            case PieceType.Knight: return GetKnightMoves();
            default: return new List<Vector2Int>();
        }
    }
    private List<Vector2Int> GetMovesInDirections(Vector2Int[] directions)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        foreach (var dir in directions)
        {
            Vector2Int current = boardPosition + dir;
            while (boardManager.IsWithinBoard(current))
            {
                if (boardManager.IsTileEmpty(current))
                {
                    moves.Add(current);
                }
                else
                {
                    if (boardManager.IsEnemyAt(current, isWhite))
                        moves.Add(current);
                    break;
                }
                current += dir;
            }
        }

        return moves;
    }


    private List<Vector2Int> GetPawnMoves()
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        Vector2Int forward = isWhite ? Vector2Int.up : Vector2Int.down;
        Vector2Int next1 = boardPosition + forward;
        Vector2Int next2 = boardPosition + forward * 2;

        if (boardManager.IsTileEmpty(next1))
        {
            moves.Add(next1);

            bool onStartRow = (isWhite && boardPosition.y == 1) || (!isWhite && boardPosition.y == 6);
            if (onStartRow && boardManager.IsTileEmpty(next2))
                moves.Add(next2);
        }

        Vector2Int[] diagonals = { new Vector2Int(1, 1), new Vector2Int(-1, 1) };
        if (!isWhite) diagonals = new Vector2Int[] { new Vector2Int(1, -1), new Vector2Int(-1, -1) };

        foreach (var dir in diagonals)
        {
            Vector2Int targetPos = boardPosition + dir;
            if (boardManager.IsEnemyAt(targetPos, isWhite))
            {
                moves.Add(targetPos);
            }
        }

        return moves;
    }

    private List<Vector2Int> GetRookMoves()
    {
        return GetMovesInDirections(new Vector2Int[] {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    });
    }

    private List<Vector2Int> GetBishopMoves()
    {
        return GetMovesInDirections(new Vector2Int[] {
        new Vector2Int(1,1), new Vector2Int(-1,1),
        new Vector2Int(1,-1), new Vector2Int(-1,-1)
    });
    }

    private List<Vector2Int> GetQueenMoves()
    {
        return GetMovesInDirections(new Vector2Int[] {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1,1), new Vector2Int(-1,1),
        new Vector2Int(1,-1), new Vector2Int(-1,-1)
    });
    }

    private List<Vector2Int> GetKingMoves()
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        Vector2Int[] directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1,1), new Vector2Int(-1,1),
        new Vector2Int(1,-1), new Vector2Int(-1,-1)
    };

        foreach (var dir in directions)
        {
            Vector2Int target = boardPosition + dir;
            if (boardManager.IsTileEmpty(target) || boardManager.IsEnemyAt(target, isWhite))
                moves.Add(target);
        }

        return moves;
    }

    private List<Vector2Int> GetKnightMoves()
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        Vector2Int[] deltas = {
        new Vector2Int(1,2), new Vector2Int(2,1),
        new Vector2Int(-1,2), new Vector2Int(-2,1),
        new Vector2Int(1,-2), new Vector2Int(2,-1),
        new Vector2Int(-1,-2), new Vector2Int(-2,-1)
    };

        foreach (var delta in deltas)
        {
            Vector2Int target = boardPosition + delta;
            if (boardManager.IsTileEmpty(target) || boardManager.IsEnemyAt(target, isWhite))
                moves.Add(target);
        }

        return moves;
    }

}
