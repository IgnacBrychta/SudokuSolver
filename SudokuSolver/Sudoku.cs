namespace SudokuSolver;

public abstract class Sudoku
{
	private protected const int sideLength = 9;
	private protected const int smallestDigit = 1;
	private protected const int largestDigit = 9;
	private protected const int emptyCellValue = -1;
	private protected static bool IsStepValid(Cell cell, int digit, ref Cell[,]? sudokuGrid)
	{
		var rowGroup = GetRowDigits(cell.location.x, ref sudokuGrid);
		var columnGroup = GetColumnDigits(cell.location.y, ref sudokuGrid);
		var squareGroup = GetDigitsInSquare(PointToSquare(cell.location), ref sudokuGrid);
		bool rowOK = !DoesDigitRepeat(digit, rowGroup);
		bool columnOK = !DoesDigitRepeat(digit, columnGroup);
		bool squareOK = !DoesDigitRepeat(digit, squareGroup);
		return rowOK && columnOK && squareOK;
	}

	private protected static SudokuSquare PointToSquare(Point point)
	{
		switch ((point.x, point.y))
		{
			case ( < 3, < 3):
				return SudokuSquare.NW;
			case ( < 6, < 3):
				return SudokuSquare.N;
			case ( < 9, < 3):
				return SudokuSquare.NE;
			case ( < 3, < 6):
				return SudokuSquare.W;
			case ( < 6, < 6):
				return SudokuSquare.CENTER;
			case ( < 9, < 6):
				return SudokuSquare.E;
			case ( < 3, < 9):
				return SudokuSquare.SW;
			case ( < 6, < 9):
				return SudokuSquare.S;
			case ( < 9, < 9):
				return SudokuSquare.SE;
			default:
				return SudokuSquare.NONE;
		}
	}

	private protected static bool DoesDigitRepeat(int digit, Dictionary<int, int> group)
	{
		if (digit == emptyCellValue) return false;
		return group[digit] == 1;
	}

	private protected static Dictionary<int, int> GetRowDigits(int row, ref Cell[,]? sudokuGrid)
	{
		Dictionary<int, int> groupedNumbers = CreateGroupedNumbersDictionary();
		for (int j = 0; j < sideLength; j++)
		{
			int value = sudokuGrid![row, j].value;
			if (value != emptyCellValue) groupedNumbers[value]++;
		}
		return groupedNumbers;
	}

	private protected static Dictionary<int, int> GetColumnDigits(int column, ref Cell[,]? sudokuGrid)
	{
		Dictionary<int, int> groupedNumbers = CreateGroupedNumbersDictionary();
		for (int i = 0; i < sideLength; i++)
		{
			int value = sudokuGrid![i, column].value;
			if (value != emptyCellValue) groupedNumbers[value]++;
		}
		return groupedNumbers;
	}

	private protected static Dictionary<int, int> GetDigitsInSquare(SudokuSquare square, ref Cell[,]? sudokuGrid)
	{
		Dictionary<int, int> groupedNumbers = CreateGroupedNumbersDictionary();
		Point squareCoordsMultiplier = map[square];
		int multiplierX = squareCoordsMultiplier.x;
		int multiplierY = squareCoordsMultiplier.y;
		int squareSideLength = 3;
		int limitX = squareSideLength + squareSideLength * (multiplierX + 1);
		int limitY = squareSideLength + squareSideLength * (multiplierY + 1);
		for (int i = squareSideLength + squareSideLength * multiplierX; i < limitX; i++)
		{
			for (int j = squareSideLength + squareSideLength * multiplierY; j < limitY; j++)
			{
				int value = sudokuGrid![i, j].value;
				if (value != emptyCellValue) groupedNumbers[value]++;
			}
		}
		return groupedNumbers;
	}

	private protected enum SudokuSquare
	{
		N = 1,
		NE = 2,
		E = 4,
		SE = 8,
		S = 16,
		SW = 32,
		W = 64,
		NW = 128,
		CENTER = 256,
		NONE = 512
	}

	private protected static readonly Dictionary<SudokuSquare, Point> map = new Dictionary<SudokuSquare, Point>()
	{
		{ SudokuSquare.N,      new Point( 0, -1) },
		{ SudokuSquare.NE,     new Point( 1, -1) },
		{ SudokuSquare.E,      new Point( 1,  0) },
		{ SudokuSquare.SE,     new Point( 1,  1) },
		{ SudokuSquare.S,      new Point( 0,  1) },
		{ SudokuSquare.SW,     new Point(-1,  1) },
		{ SudokuSquare.W,      new Point(-1,  0) },
		{ SudokuSquare.NW,     new Point(-1, -1) },
		{ SudokuSquare.CENTER, new Point( 0,  0) }
	};

	private protected static Dictionary<int, int> CreateGroupedNumbersDictionary()
	{
		return new Dictionary<int, int>()
		{
			{ 1, 0 },
			{ 2, 0 },
			{ 3, 0 },
			{ 4, 0 },
			{ 5, 0 },
			{ 6, 0 },
			{ 7, 0 },
			{ 8, 0 },
			{ 9, 0 }
		};
	}

	private protected static int[,] CellArrayToIntArray(Cell[,]? cells)
	{
		int[,] array = new int[sideLength, sideLength];
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				array[i, j] = cells![i, j].value;
			}
		}
		return array;
	}
}
