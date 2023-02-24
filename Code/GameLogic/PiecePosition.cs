namespace MonoMatch3.Code.GameLogic;

public struct PiecePosition
{
    public int Row;
    public int Column;

    public PiecePosition(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public override string? ToString()
    {
        return $"Row: {Row}; Column: {Column}";
    }
}