namespace SudokuSolver;

/// <summary>
/// A sudoku grid cell that stores its digit (<see cref="value"/>) and location (<see cref="location"/>)
/// </summary>
public class Cell
{
	internal int value;
	internal Point location;
	internal Cell(int value, Point location)
	{
		this.value = value;
		this.location = location;
	}

	/// <summary>
	/// Retrieves data about this <see cref="Cell"/> instance useful for debugging
	/// </summary>
	/// <returns></returns>
	public override string ToString()
	{
		return $"val:{value};{location}";
	}
}
