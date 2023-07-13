namespace SudokuSolver;

public abstract class Sudoku
{
	private protected const int sideLength = 9;
	private protected const int smallestDigit = 1;
	private protected const int largestDigit = 9;
	private protected const int emptyCellValue = -1;

	/// <summary>
	/// Determines whether <paramref name="digit"/> in the position of <paramref name="cell"/> doesn't violate the rules of sudoku in <paramref name="sudokuGrid"/> 
	/// </summary>
	/// <param name="cell"></param>
	/// <param name="digit"></param>
	/// <param name="sudokuGrid"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Converts the x, y coordinates of <paramref name="point"/> to <see cref="SudokuSquare"/>
	/// </summary>
	/// <param name="point"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Determines whether the specified <paramref name="digit"/> is present more than once in specified <paramref name="group"/>
	/// </summary>
	/// <param name="digit"></param>
	/// <param name="group"></param>
	/// <returns></returns>
	private protected static bool DoesDigitRepeat(int digit, Dictionary<int, int> group)
	{
		if (digit == emptyCellValue) return false;
		return group[digit] == 1;
	}

	/// <summary>
	/// Retrieves digits in the row specified
	/// </summary>
	/// <param name="row"></param>
	/// <param name="sudokuGrid"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Retrieves digits in the column specified
	/// </summary>
	/// <param name="column"></param>
	/// <param name="sudokuGrid"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Retrieves digits in the square specified
	/// </summary>
	/// <param name="square"></param>
	/// <param name="sudokuGrid"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Creates a new <see cref="Dictionary{int, int}"/> to store the count of digits present in a row, column or square
	/// </summary>
	/// <returns></returns>
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

	/// <summary>
	/// Creates a sudoku <see cref="Cell"/>[,] grid from <paramref name="grid"/>
	/// </summary>
	/// <param name="grid"></param>
	/// <returns></returns>
	protected static Cell[,] CreateCellGrid(int[,] grid)
	{
		Cell[,] cellGrid = new Cell[sideLength, sideLength];
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				int cellValue = grid[i, j];
				cellGrid[i, j] = new Cell(cellValue, new Point(i, j));
			}
		}
		return cellGrid;
	}

	/// <summary>
	/// Checks the <paramref name="sudokuDigits"/> whether it violates the rules of sudoku or not
	/// </summary>
	/// <returns></returns>
	public static bool CheckIfSudokuValid(int[,] sudokuDigits)
	{
		Cell[,]? localSudokuGrid = CreateCellGrid(sudokuDigits);
		if (localSudokuGrid is null) return false;

		for (int i = 0; i < sideLength; i++)
		{
			SudokuSquare square = (SudokuSquare)Math.Pow(2, i);
			var rowDigits = GetRowDigits(i, ref localSudokuGrid);
			var columnDigits = GetColumnDigits(i, ref localSudokuGrid);
			var subgridDigits = GetDigitsInSquare(square, ref localSudokuGrid);
			for (int digit = smallestDigit; digit <= largestDigit; digit++)
			{
				bool rowOK = rowDigits[digit] <= 1;
				bool columnOK = columnDigits[digit] <= 1;
				bool squareOK = subgridDigits[digit] <= 1;
				if (!(rowOK && columnOK && squareOK)) return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Checks the <paramref name="sudokuDigits"/> whether it is solved
	/// </summary>
	/// <returns></returns>
	public static bool CheckIfSudokuSolved(int[,] sudokuDigits)
	{
		Cell[,]? localSudokuGrid = CreateCellGrid(sudokuDigits);
		if (localSudokuGrid is null) return false;

		for (int i = 0; i < sideLength; i++)
		{
			SudokuSquare square = (SudokuSquare)Math.Pow(2, i);
			var rowDigits = GetRowDigits(i, ref localSudokuGrid);
			var columnDigits = GetColumnDigits(i, ref localSudokuGrid);
			var subgridDigits = GetDigitsInSquare(square, ref localSudokuGrid);
			for (int digit = smallestDigit; digit <= largestDigit; digit++)
			{
				bool rowOK = rowDigits[digit] <= 1 && rowDigits.Values.Sum() == sideLength;
				bool columnOK = columnDigits[digit] <= 1 && columnDigits.Values.Sum() == sideLength;
				bool squareOK = subgridDigits[digit] <= 1 && subgridDigits.Values.Sum() == sideLength;
				if (!(rowOK && columnOK && squareOK)) return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Retrieves the values of a sudoku grid 
	/// </summary>
	/// <param name="cells"></param>
	/// <returns></returns>
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
