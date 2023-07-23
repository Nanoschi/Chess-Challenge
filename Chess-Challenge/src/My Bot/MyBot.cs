using ChessChallenge.API;
using System;
using System.Collections.Generic;

class TreeNode
{
    public const int MaxDepth = 3;
    public const float StdValEnemyTurn = float.NegativeInfinity;
    public const float StdValBotTurn = float.PositiveInfinity;
    public static uint TotalNodeCount { get; set; }

    public List<TreeNode> Children { get; set; } = new List<TreeNode>();
    public float Value { get; set; } = 0;
    public bool IsRoot { get; set; } = true;
    public Move BranchMove { get; set; }
    public TreeNode Parent { get; set; }

    public TreeNode(Move move, TreeNode parent)
    {
        BranchMove = move;
        Parent = parent;
        IsRoot = false;
        TotalNodeCount++;
    }
    public TreeNode()
    {
        IsRoot = true;
        TotalNodeCount++;
    }

    public bool IsLeaf(int depth) => depth == MaxDepth - 1;

    public void GenerateTree(Board board, int current_depth = 0)
    {
        if (current_depth == MaxDepth)
        {
            return;
        }

        bool is_opponent_turn = current_depth % 2 == 0;

        Move[] possible_moves = board.GetLegalMoves();
        float value = 0;

        foreach (Move move in possible_moves)
        {
            TreeNode node = new TreeNode(move, this);

            board.MakeMove(move);

            if (IsLeaf(current_depth))
            {
                value = Evaluator.GetWhiteLead(board);
            }
            else if (is_opponent_turn)
            {
                value = StdValEnemyTurn;
            }
            else
            {
                value = StdValBotTurn;
            }
            node.Value = value;

            node.GenerateTree(board, current_depth + 1);
            board.UndoMove(move);

            Children.Add(node);
        }
    }

    private void PropagateValueUpwards(float value, TreeNode start_node, bool is_bot_turn)
    {
        TreeNode parent = start_node.Parent;
        while (!parent.IsRoot)
        {
            if (is_bot_turn)
            {
                parent.Value = Math.Max(value, parent.Value);
            }
            else
            {
                parent.Value = Math.Min(value, parent.Value);
            }

        }
    }

}

static class Evaluator
{
    public static float GetPieceValue(PieceType piece)
    {
        switch (piece)
        {
            case PieceType.Pawn:
                return 1f;
            case PieceType.Knight:
            case PieceType.Bishop:
                return 3f;
            case PieceType.Rook:
                return 5f;
            case PieceType.Queen:
                return 9f;
            case PieceType.King:
                return 1000f;
        }
        return 0;
    }

    public static float GetMaterialAdvantage(Board board)
    {
        float total = 0;
        foreach (PieceList list in board.GetAllPieceLists())
        {
            float abs_value = GetPieceValue(list.TypeOfPieceInList) * list.Count;
            total += list.IsWhitePieceList
            ? abs_value
            : -abs_value;
        }
        return total;
    }

    public static float GetWhiteLead(Board board)
    {
        return GetMaterialAdvantage(board);
    }
}

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        TreeNode tree = new TreeNode();
        tree.GenerateTree(board);
        Move[] moves = board.GetLegalMoves();
        return moves[0];
    }
}