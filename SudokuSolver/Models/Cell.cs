namespace SudokuSolver;

public class Cell
{
	internal int value;
	internal Point location;
	internal Cell(int value, Point location)
	{
		this.value = value;
		this.location = location;
	}

	public override string ToString()
	{
		return $"val:{value};{location}";
	}
}
