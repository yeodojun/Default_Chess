using UnityEngine;

public class MoveTile : MonoBehaviour
{
    private Vector2Int targetPos;
    private Piece selectedPiece;

    public void Setup(Vector2Int pos, Piece piece)
    {
        targetPos = pos;
        selectedPiece = piece;
    }

    private void OnMouseDown()
    {
        BoardManager manager = Object.FindFirstObjectByType<BoardManager>();
        manager.MovePiece(selectedPiece, targetPos);
    }
}
