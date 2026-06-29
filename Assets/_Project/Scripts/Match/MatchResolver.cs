using System.Collections.Generic;
using UnityEngine;

public class MatchResolver
{
    private const int MaxResolveLoopCount = 20;

    public Dictionary<ColorId, int> Resolve(BoardModel boardModel)
    {
        Dictionary<ColorId, int> totalScores = new Dictionary<ColorId, int>();

        if (boardModel == null)
            return totalScores;

        int loopCount = 0;
        bool hasAnyMatch;

        do
        {
            hasAnyMatch = false;

            List<MatchedMiniCell> horizontalMatches = FindHorizontalMatches(boardModel);

            if (horizontalMatches.Count > 0)
            {
                List<MatchedMiniCell> expandedHorizontalMatches = ExpandToConnectedColorGroups(horizontalMatches);
                Dictionary<ColorId, int> horizontalScores = ApplyMatches(boardModel, expandedHorizontalMatches);
                MergeScores(totalScores, horizontalScores);
                hasAnyMatch = true;
            }

            List<MatchedMiniCell> verticalMatches = FindVerticalMatches(boardModel);

            if (verticalMatches.Count > 0)
            {
                List<MatchedMiniCell> expandedVerticalMatches = ExpandToConnectedColorGroups(verticalMatches);
                Dictionary<ColorId, int> verticalScores = ApplyMatches(boardModel, expandedVerticalMatches);
                MergeScores(totalScores, verticalScores);
                hasAnyMatch = true;
            }

            loopCount++;

            if (loopCount >= MaxResolveLoopCount)
            {
                Debug.LogWarning("MatchResolver stopped because max loop count was reached.");
                break;
            }

        } while (hasAnyMatch);

        return totalScores;
    }

    private List<MatchedMiniCell> FindHorizontalMatches(BoardModel boardModel)
    {
        List<MatchedMiniCell> matches = new List<MatchedMiniCell>();

        for (int z = 0; z < boardModel.Height; z++)
        {
            for (int x = 0; x < boardModel.Width - 1; x++)
            {
                PieceView leftPiece = boardModel.GetPiece(x, z);
                PieceView rightPiece = boardModel.GetPiece(x + 1, z);

                if (leftPiece == null || rightPiece == null)
                    continue;

                TryAddPairMatch(
                    matches,
                    leftPiece,
                    MiniSlot.TopRight,
                    rightPiece,
                    MiniSlot.TopLeft
                );

                TryAddPairMatch(
                    matches,
                    leftPiece,
                    MiniSlot.BottomRight,
                    rightPiece,
                    MiniSlot.BottomLeft
                );
            }
        }

        return RemoveDuplicateMatches(matches);
    }

    private List<MatchedMiniCell> FindVerticalMatches(BoardModel boardModel)
    {
        List<MatchedMiniCell> matches = new List<MatchedMiniCell>();

        for (int x = 0; x < boardModel.Width; x++)
        {
            for (int z = 0; z < boardModel.Height - 1; z++)
            {
                PieceView bottomPiece = boardModel.GetPiece(x, z);
                PieceView topPiece = boardModel.GetPiece(x, z + 1);

                if (bottomPiece == null || topPiece == null)
                    continue;

                TryAddPairMatch(
                    matches,
                    bottomPiece,
                    MiniSlot.TopLeft,
                    topPiece,
                    MiniSlot.BottomLeft
                );

                TryAddPairMatch(
                    matches,
                    bottomPiece,
                    MiniSlot.TopRight,
                    topPiece,
                    MiniSlot.BottomRight
                );
            }
        }

        return RemoveDuplicateMatches(matches);
    }

    private void TryAddPairMatch(
        List<MatchedMiniCell> matches,
        PieceView pieceA,
        MiniSlot slotA,
        PieceView pieceB,
        MiniSlot slotB)
    {
        ColorId colorA = pieceA.Data.Get(slotA);
        ColorId colorB = pieceB.Data.Get(slotB);

        if (colorA == ColorId.None || colorB == ColorId.None)
            return;

        if (colorA != colorB)
            return;

        matches.Add(new MatchedMiniCell(pieceA, slotA, colorA));
        matches.Add(new MatchedMiniCell(pieceB, slotB, colorB));

        Debug.Log($"Matched {colorA}: {pieceA.name}.{slotA} <-> {pieceB.name}.{slotB}");
    }

    private List<MatchedMiniCell> ExpandToConnectedColorGroups(List<MatchedMiniCell> rawMatches)
    {
        List<MatchedMiniCell> expandedMatches = new List<MatchedMiniCell>();

        foreach (MatchedMiniCell rawMatch in rawMatches)
        {
            List<MiniSlot> connectedSlots = GetConnectedSameColorSlots(
                rawMatch.Piece,
                rawMatch.Slot,
                rawMatch.Color
            );

            foreach (MiniSlot connectedSlot in connectedSlots)
            {
                expandedMatches.Add(new MatchedMiniCell(
                    rawMatch.Piece,
                    connectedSlot,
                    rawMatch.Color
                ));
            }
        }

        return RemoveDuplicateMatches(expandedMatches);
    }

    private List<MiniSlot> GetConnectedSameColorSlots(PieceView piece, MiniSlot startSlot, ColorId color)
    {
        List<MiniSlot> result = new List<MiniSlot>();
        Queue<MiniSlot> queue = new Queue<MiniSlot>();
        HashSet<MiniSlot> visited = new HashSet<MiniSlot>();

        queue.Enqueue(startSlot);
        visited.Add(startSlot);

        while (queue.Count > 0)
        {
            MiniSlot currentSlot = queue.Dequeue();

            if (piece.Data.Get(currentSlot) != color)
                continue;

            result.Add(currentSlot);

            foreach (MiniSlot neighborSlot in GetNeighborSlots(currentSlot))
            {
                if (visited.Contains(neighborSlot))
                    continue;

                if (piece.Data.Get(neighborSlot) != color)
                    continue;

                visited.Add(neighborSlot);
                queue.Enqueue(neighborSlot);
            }
        }

        return result;
    }

    private IEnumerable<MiniSlot> GetNeighborSlots(MiniSlot slot)
    {
        switch (slot)
        {
            case MiniSlot.TopLeft:
                yield return MiniSlot.TopRight;
                yield return MiniSlot.BottomLeft;
                break;

            case MiniSlot.TopRight:
                yield return MiniSlot.TopLeft;
                yield return MiniSlot.BottomRight;
                break;

            case MiniSlot.BottomLeft:
                yield return MiniSlot.TopLeft;
                yield return MiniSlot.BottomRight;
                break;

            case MiniSlot.BottomRight:
                yield return MiniSlot.TopRight;
                yield return MiniSlot.BottomLeft;
                break;
        }
    }

    private Dictionary<ColorId, int> ApplyMatches(BoardModel boardModel, List<MatchedMiniCell> matches)
    {
        Dictionary<ColorId, int> scoreByColor = CalculateScoreByColor(matches);
        HashSet<PieceView> affectedPieces = new HashSet<PieceView>();

        foreach (MatchedMiniCell match in matches)
        {
            match.Piece.PlayMatchDisappearEffect(match.Slot, match.Color);

            match.Piece.Data.Set(match.Slot, ColorId.None);
            affectedPieces.Add(match.Piece);
        }

        foreach (PieceView piece in affectedPieces)
        {
            piece.Refresh();

            List<FillMove> fillMoves = piece.Data.ResolveEmptySlots();

            if (piece.Data.IsEmpty())
            {
                boardModel.RemovePiece(piece);
                Object.Destroy(piece.gameObject);

                Debug.Log($"Removed empty piece: {piece.name}");
                continue;
            }

            piece.PlayFillAnimations(fillMoves);
            piece.RefreshAfterDelay(0.45f);
        }

        Debug.Log($"Resolved {matches.Count} mini cells after connected-group expansion.");

        return scoreByColor;
    }

    private Dictionary<ColorId, int> CalculateScoreByColor(List<MatchedMiniCell> matches)
    {
        Dictionary<ColorId, HashSet<PieceView>> piecesByColor = new Dictionary<ColorId, HashSet<PieceView>>();

        foreach (MatchedMiniCell match in matches)
        {
            if (match.Color == ColorId.None)
                continue;

            if (!piecesByColor.ContainsKey(match.Color))
            {
                piecesByColor[match.Color] = new HashSet<PieceView>();
            }

            piecesByColor[match.Color].Add(match.Piece);
        }

        Dictionary<ColorId, int> scoreByColor = new Dictionary<ColorId, int>();

        foreach (KeyValuePair<ColorId, HashSet<PieceView>> pair in piecesByColor)
        {
            scoreByColor[pair.Key] = pair.Value.Count;
        }

        return scoreByColor;
    }

    private void MergeScores(Dictionary<ColorId, int> totalScores, Dictionary<ColorId, int> addedScores)
    {
        foreach (KeyValuePair<ColorId, int> pair in addedScores)
        {
            if (!totalScores.ContainsKey(pair.Key))
            {
                totalScores[pair.Key] = 0;
            }

            totalScores[pair.Key] += pair.Value;
        }
    }

    private List<MatchedMiniCell> RemoveDuplicateMatches(List<MatchedMiniCell> matches)
    {
        List<MatchedMiniCell> uniqueMatches = new List<MatchedMiniCell>();
        HashSet<string> usedKeys = new HashSet<string>();

        foreach (MatchedMiniCell match in matches)
        {
            string key = $"{match.Piece.GetInstanceID()}_{match.Slot}";

            if (usedKeys.Add(key))
            {
                uniqueMatches.Add(match);
            }
        }

        return uniqueMatches;
    }
}