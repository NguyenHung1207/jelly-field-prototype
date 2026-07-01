using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchResolver
{
    private const int MaxResolveLoopCount = 20;

    private readonly MiniSlot[] allSlots =
    {
        MiniSlot.TopLeft,
        MiniSlot.TopRight,
        MiniSlot.BottomLeft,
        MiniSlot.BottomRight
    };

    public IEnumerator ResolveSequential(BoardModel boardModel, Action<Dictionary<ColorId, int>> onComplete)
    {
        Dictionary<ColorId, int> totalScores = new Dictionary<ColorId, int>();

        if (boardModel == null)
        {
            onComplete?.Invoke(totalScores);
            yield break;
        }

        int loopCount = 0;

        while (loopCount < MaxResolveLoopCount)
        {
            List<MatchedMiniCell> matches = FindAllConnectedMatches(boardModel);

            if (matches.Count == 0)
                break;

            Dictionary<ColorId, int> phaseScores = CalculateScoreByColor(matches);
            MergeScores(totalScores, phaseScores);

            yield return ProcessMatchedCells(boardModel, matches);

            loopCount++;
        }

        if (loopCount >= MaxResolveLoopCount)
        {
            Debug.LogWarning("MatchResolver stopped because max loop count was reached.");
        }

        onComplete?.Invoke(totalScores);
    }

    private List<MatchedMiniCell> FindAllConnectedMatches(BoardModel boardModel)
    {
        List<MatchedMiniCell> result = new List<MatchedMiniCell>();
        HashSet<string> globalVisited = new HashSet<string>();
        HashSet<string> addedMatches = new HashSet<string>();

        for (int z = 0; z < boardModel.Height; z++)
        {
            for (int x = 0; x < boardModel.Width; x++)
            {
                PieceView piece = boardModel.GetPiece(x, z);

                if (piece == null || piece.Data == null)
                    continue;

                foreach (MiniSlot slot in allSlots)
                {
                    ColorId color = piece.Data.Get(slot);

                    if (color == ColorId.None)
                        continue;

                    MiniNode startNode = new MiniNode(x, z, piece, slot, color);
                    string startKey = startNode.Key;

                    if (globalVisited.Contains(startKey))
                        continue;

                    List<MiniNode> component = new List<MiniNode>();
                    Queue<MiniNode> queue = new Queue<MiniNode>();
                    bool hasExternalTouch = false;

                    queue.Enqueue(startNode);
                    globalVisited.Add(startKey);

                    while (queue.Count > 0)
                    {
                        MiniNode current = queue.Dequeue();
                        component.Add(current);

                        foreach (NeighborResult neighborResult in GetSameColorNeighbors(boardModel, current))
                        {
                            if (neighborResult.IsExternal)
                            {
                                hasExternalTouch = true;
                            }

                            MiniNode neighbor = neighborResult.Node;
                            string neighborKey = neighbor.Key;

                            if (globalVisited.Contains(neighborKey))
                                continue;

                            globalVisited.Add(neighborKey);
                            queue.Enqueue(neighbor);
                        }
                    }

                    if (!hasExternalTouch)
                        continue;

                    foreach (MiniNode node in component)
                    {
                        string matchKey = node.Key;

                        if (!addedMatches.Add(matchKey))
                            continue;

                        result.Add(new MatchedMiniCell(node.Piece, node.Slot, node.Color));
                    }
                }
            }
        }

        return result;
    }

    private IEnumerable<NeighborResult> GetSameColorNeighbors(BoardModel boardModel, MiniNode node)
    {
        foreach (MiniSlot internalSlot in GetInternalNeighborSlots(node.Slot))
        {
            ColorId internalColor = node.Piece.Data.Get(internalSlot);

            if (internalColor == node.Color)
            {
                yield return new NeighborResult(
                    new MiniNode(node.X, node.Z, node.Piece, internalSlot, node.Color),
                    false
                );
            }
        }

        foreach (ExternalNeighbor externalNeighbor in GetExternalNeighbors(node.Slot))
        {
            int neighborX = node.X + externalNeighbor.OffsetX;
            int neighborZ = node.Z + externalNeighbor.OffsetZ;

            if (!boardModel.IsInsideBounds(neighborX, neighborZ))
                continue;

            PieceView neighborPiece = boardModel.GetPiece(neighborX, neighborZ);

            if (neighborPiece == null || neighborPiece.Data == null)
                continue;

            ColorId neighborColor = neighborPiece.Data.Get(externalNeighbor.TargetSlot);

            if (neighborColor != node.Color)
                continue;

            yield return new NeighborResult(
                new MiniNode(neighborX, neighborZ, neighborPiece, externalNeighbor.TargetSlot, node.Color),
                true
            );
        }
    }

    private IEnumerable<MiniSlot> GetInternalNeighborSlots(MiniSlot slot)
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

    private IEnumerable<ExternalNeighbor> GetExternalNeighbors(MiniSlot slot)
    {
        switch (slot)
        {
            case MiniSlot.TopLeft:
                yield return new ExternalNeighbor(-1, 0, MiniSlot.TopRight);
                yield return new ExternalNeighbor(0, 1, MiniSlot.BottomLeft);
                break;

            case MiniSlot.TopRight:
                yield return new ExternalNeighbor(1, 0, MiniSlot.TopLeft);
                yield return new ExternalNeighbor(0, 1, MiniSlot.BottomRight);
                break;

            case MiniSlot.BottomLeft:
                yield return new ExternalNeighbor(-1, 0, MiniSlot.BottomRight);
                yield return new ExternalNeighbor(0, -1, MiniSlot.TopLeft);
                break;

            case MiniSlot.BottomRight:
                yield return new ExternalNeighbor(1, 0, MiniSlot.BottomLeft);
                yield return new ExternalNeighbor(0, -1, MiniSlot.TopRight);
                break;
        }
    }

    private IEnumerator ProcessMatchedCells(BoardModel boardModel, List<MatchedMiniCell> matches)
    {
        HashSet<PieceView> affectedPieces = new HashSet<PieceView>();

        foreach (MatchedMiniCell match in matches)
        {
            if (match.Piece == null || match.Piece.Data == null)
                continue;

            match.Piece.PlayMatchDisappearEffect(match.Slot, match.Color);
            match.Piece.Data.Set(match.Slot, ColorId.None);
            affectedPieces.Add(match.Piece);
        }

        foreach (PieceView piece in affectedPieces)
        {
            if (piece != null)
            {
                piece.Refresh();
            }
        }

        yield return new WaitForSeconds(GameAnimationTiming.MatchDisappearDuration);

        List<PieceView> piecesToRefreshAfterFill = new List<PieceView>();
        bool hasFillAnimation = false;

        foreach (PieceView piece in affectedPieces)
        {
            if (piece == null || piece.Data == null)
                continue;

            if (piece.Data.IsEmpty())
            {
                boardModel.RemovePiece(piece);
                UnityEngine.Object.Destroy(piece.gameObject);
                continue;
            }

            List<FillMove> fillMoves = piece.Data.ResolveEmptySlots();

            if (fillMoves != null && fillMoves.Count > 0)
            {
                piece.PlayFillAnimations(fillMoves);
                piecesToRefreshAfterFill.Add(piece);
                hasFillAnimation = true;
            }
            else
            {
                piece.Refresh();
            }
        }

        if (hasFillAnimation)
        {
            yield return new WaitForSeconds(GameAnimationTiming.FillAnimationDuration);
        }

        foreach (PieceView piece in piecesToRefreshAfterFill)
        {
            if (piece != null)
            {
                piece.Refresh();
            }
        }
    }

    private Dictionary<ColorId, int> CalculateScoreByColor(List<MatchedMiniCell> matches)
    {
        Dictionary<ColorId, HashSet<PieceView>> piecesByColor = new Dictionary<ColorId, HashSet<PieceView>>();

        foreach (MatchedMiniCell match in matches)
        {
            if (match.Color == ColorId.None || match.Piece == null)
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

    private class MatchedMiniCell
    {
        public readonly PieceView Piece;
        public readonly MiniSlot Slot;
        public readonly ColorId Color;

        public MatchedMiniCell(PieceView piece, MiniSlot slot, ColorId color)
        {
            Piece = piece;
            Slot = slot;
            Color = color;
        }
    }

    private struct MiniNode
    {
        public readonly int X;
        public readonly int Z;
        public readonly PieceView Piece;
        public readonly MiniSlot Slot;
        public readonly ColorId Color;

        public string Key => $"{X}_{Z}_{Slot}";

        public MiniNode(int x, int z, PieceView piece, MiniSlot slot, ColorId color)
        {
            X = x;
            Z = z;
            Piece = piece;
            Slot = slot;
            Color = color;
        }
    }

    private struct NeighborResult
    {
        public readonly MiniNode Node;
        public readonly bool IsExternal;

        public NeighborResult(MiniNode node, bool isExternal)
        {
            Node = node;
            IsExternal = isExternal;
        }
    }

    private struct ExternalNeighbor
    {
        public readonly int OffsetX;
        public readonly int OffsetZ;
        public readonly MiniSlot TargetSlot;

        public ExternalNeighbor(int offsetX, int offsetZ, MiniSlot targetSlot)
        {
            OffsetX = offsetX;
            OffsetZ = offsetZ;
            TargetSlot = targetSlot;
        }
    }
}