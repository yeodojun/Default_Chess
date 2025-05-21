using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject boardBackground; // board.png
    public GameObject[] whitePiecePrefabs;
    public GameObject[] blackPiecePrefabs;

    private GameObject[,] pieces = new GameObject[8, 8];

    public GameObject moveTilePrefab;
    private List<GameObject> moveTiles = new List<GameObject>();
    private Queue<GameObject> tilePool = new Queue<GameObject>();
    public int poolSize = 32;

    public bool whiteTurn = true;

    private List<GameObject> capturedWhite = new List<GameObject>();
    private List<GameObject> capturedBlack = new List<GameObject>();
    private float spacing = 0.58f;

    private Vector2 capturedWhiteStart = new Vector2(-2.02f, 2.7f); // black captured
    private Vector2 capturedBlackStart = new Vector2(-2.02f, -2.7f); // white captured

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject tile = Instantiate(moveTilePrefab);
            tile.SetActive(false);
            tile.AddComponent<MoveTile>();
            tilePool.Enqueue(tile);
        }

        SetupBoard();
    }

    void SetupBoard()
    {
        // 폰 배치
        for (int x = 0; x < 8; x++)
        {
            SpawnPiece(blackPiecePrefabs[0], new Vector2Int(x, 6), false); // black pawn
            SpawnPiece(whitePiecePrefabs[0], new Vector2Int(x, 1), true);  // white pawn
        }

        // 흰색 말 배치 (아래쪽, y = 0)
        int[] whiteLayout = { 1, 2, 5, 4, 3, 5, 2, 1 }; // 퀸(4), 킹(3)
        for (int x = 0; x < 8; x++)
            SpawnPiece(whitePiecePrefabs[whiteLayout[x]], new Vector2Int(x, 0), true);

        // 검은색 말 배치 (위쪽, y = 7)
        int[] blackLayout = { 1, 2, 5, 3, 4, 5, 2, 1 }; // 킹(3), 퀸(4)
        for (int x = 0; x < 8; x++)
            SpawnPiece(blackPiecePrefabs[blackLayout[x]], new Vector2Int(x, 7), false);
    }


    void SpawnPiece(GameObject prefab, Vector2Int pos, bool isWhite)
    {
        Vector2 worldPos = BoardToWorldPos(pos);
        GameObject pieceObj = Instantiate(prefab, new Vector3(worldPos.x, worldPos.y, 0), Quaternion.identity);
        Piece piece = pieceObj.GetComponent<Piece>();
        piece.isWhite = isWhite;
        piece.SetPosition(pos);
        pieces[pos.x, pos.y] = pieceObj;
    }


    public Vector2 BoardToWorldPos(Vector2Int boardPos)
    {
        float startX = -2.02f;
        float startY = -2.00f;
        float tileWidth = (2.02f - (-2.02f)) / 7f; // 약 0.58
        float tileHeight = (2.00f - (-2.00f)) / 7f;

        float worldX = startX + tileWidth * boardPos.x;
        float worldY = startY + tileHeight * boardPos.y;
        return new Vector2(worldX, worldY);
    }

    public void HighlightTiles(List<Vector2Int> positions, Piece piece)
    {
        foreach (Vector2Int pos in positions)
        {
            if (tilePool.Count > 0)
            {
                GameObject tile = tilePool.Dequeue();
                tile.transform.position = BoardToWorldPos(pos);
                tile.GetComponent<MoveTile>().Setup(pos, piece);
                tile.SetActive(true);
                moveTiles.Add(tile);
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (var tile in moveTiles)
        {
            tile.SetActive(false);
            tilePool.Enqueue(tile);
        }
        moveTiles.Clear();
    }


    public bool IsTileEmpty(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= 8 || pos.y < 0 || pos.y >= 8)
            return false;
        return pieces[pos.x, pos.y] == null;
    }

    public void MovePiece(Piece piece, Vector2Int newPos)
    {
        GameObject target = pieces[newPos.x, newPos.y];

        // 공격 대상 처리
        if (target != null)
        {
            var captured = target.GetComponent<Piece>();
            StoreCaptured(target, captured.isWhite);
            pieces[newPos.x, newPos.y] = null;
        }

        // 위치 이동
        pieces[piece.boardPosition.x, piece.boardPosition.y] = null;
        pieces[newPos.x, newPos.y] = piece.gameObject;
        piece.SetPosition(newPos);

        ClearHighlights();
        NextTurn();
        if (IsCheckmate(!piece.isWhite))
        {
            Debug.Log((piece.isWhite ? "White" : "Black") + " wins by checkmate!");
            // UI 띄우거나 게임 종료 로직 실행
        }
    }

    public bool IsWithinBoard(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }


    public bool IsEnemyAt(Vector2Int pos, bool isWhite)
    {
        if (pos.x < 0 || pos.x >= 8 || pos.y < 0 || pos.y >= 8) return false;

        var target = pieces[pos.x, pos.y];
        if (target == null) return false;

        return target.GetComponent<Piece>().isWhite != isWhite;
    }

    public bool IsCurrentTurn(Piece piece)
    {
        return piece.isWhite == whiteTurn;
    }

    public void NextTurn()
    {
        whiteTurn = !whiteTurn;
    }

    private void StoreCaptured(GameObject pieceObj, bool isWhite)
    {
        Vector2 basePos = isWhite ? capturedBlackStart : capturedWhiteStart;
        List<GameObject> list = isWhite ? capturedBlack : capturedWhite;

        int index = list.Count;
        float x = basePos.x + (index % 8) * spacing;
        float y;
        if (isWhite)
        {
            // 검정 말이 잡힌 경우 → 위로 정렬 (y 증가)
            y = basePos.y + (index / 8) * spacing;
        }
        else
        {
            // 흰 말이 잡힌 경우 → 아래로 정렬 (y 감소)
            y = basePos.y - (index / 8) * spacing;
        }

        pieceObj.transform.position = new Vector3(x, y, 0);
        list.Add(pieceObj);
    }

    public Piece FindKing(bool white)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var obj = pieces[x, y];
                if (obj == null) continue;

                Piece p = obj.GetComponent<Piece>();
                if (p.pieceType == PieceType.King && p.isWhite == white)
                    return p;
            }
        }
        return null;
    }

    public bool IsTileAttacked(Vector2Int pos, bool byWhite)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var obj = pieces[x, y];
                if (obj == null) continue;

                Piece p = obj.GetComponent<Piece>();
                if (p.isWhite != byWhite) continue;

                var moves = p.GetAvailableMovesForCheckTest();
                if (moves.Contains(pos))
                    return true;
            }
        }
        return false;
    }

    public bool IsInCheck(bool white)
    {
        Piece king = FindKing(white);
        return IsTileAttacked(king.boardPosition, !white);
    }

    public bool IsCheckmate(bool white)
    {
        if (!IsInCheck(white)) return false;

        // 모든 말에 대해 가능한 이동이 1개도 없으면 체크메이트
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var obj = pieces[x, y];
                if (obj == null) continue;

                Piece p = obj.GetComponent<Piece>();
                if (p.isWhite != white) continue;

                var moves = p.GetAvailableMoves();
                foreach (var move in moves)
                {
                    if (!WouldStillBeInCheckAfterMove(p, move))
                        return false;
                }
            }
        }

        return true;
    }

    private bool WouldStillBeInCheckAfterMove(Piece piece, Vector2Int toPos)
    {
        // 가상 이동
        Vector2Int originalPos = piece.boardPosition;
        GameObject targetPiece = pieces[toPos.x, toPos.y];

        pieces[originalPos.x, originalPos.y] = null;
        pieces[toPos.x, toPos.y] = piece.gameObject;
        piece.boardPosition = toPos;

        bool stillInCheck = IsInCheck(piece.isWhite);

        // 되돌리기
        pieces[originalPos.x, originalPos.y] = piece.gameObject;
        pieces[toPos.x, toPos.y] = targetPiece;
        piece.boardPosition = originalPos;

        return stillInCheck;
    }

}
