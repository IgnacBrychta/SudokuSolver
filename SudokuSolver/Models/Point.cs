namespace SudokuSolver;

/// <summary>
/// Represents a point in 2D space
/// </summary>
public struct Point
{
	internal int x;
	internal int y;
	internal Point(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public override string ToString()
	{
		return $"[{x};{y}]";
	}
}
