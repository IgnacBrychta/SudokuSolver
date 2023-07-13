#define OnlyOneSolutionPossible
using System.Diagnostics;

namespace SudokuSolver;

/// <summary>
/// Class used for generating a sudoku grid
/// </summary>
public sealed class SudokuGenerator : Sudoku
{
	Cell[,]? sudokuGrid = new Cell[sideLength, sideLength];
	public Difficulty difficulty;
	readonly Stopwatch stopwatch = new Stopwatch();
	public long TimeSpentSolving { get => stopwatch.ElapsedMilliseconds; }
	readonly Random random = new Random();
	public SudokuGenerator(Difficulty difficulty)
	{
		this.difficulty = difficulty;
	}

	public SudokuGenerator() { }

	/// <summary>
	/// Starts the process of generating a sudoku grid
	/// </summary>
	/// <returns></returns>
	public async Task GenerateAsync()
	{
		stopwatch.Restart();
		await Task.Run(() =>
		{
			GenerateGrid();
			FillInSudokuGrid(x: 0, y: 0);
			ApplySelectedDifficulty();
		});
		stopwatch.Stop();
		OnGenerationComplete();
	}

	/// <summary>
	/// Removes a number of <see cref="Cell"/>s based on <see cref="Difficulty"/>
	/// </summary>
	private void ApplySelectedDifficulty()
	{
		int numbersToRemove;
		switch (difficulty)
		{
			case Difficulty.EASY:
				numbersToRemove = random.Next(27, 31);
				break;
			case Difficulty.MEDIUM:
				numbersToRemove = random.Next(40, 45);
				break;
			case Difficulty.HARD:
				numbersToRemove = random.Next(50, 55);
				break;
			default:
				return;
		}
#if OnlyOneSolutionPossible
		int emptyCells;
		while((emptyCells = GetEmptyCellsCount()) <= numbersToRemove)
		{
			int randomRow = random.Next(sideLength);
			int randomColumn = random.Next(sideLength);
			Cell selectedCell = sudokuGrid![randomRow, randomColumn];
			int originalCellValue = selectedCell.value;
			if (originalCellValue == emptyCellValue) continue;
			selectedCell.value = emptyCellValue;

			int[,] sudokuDigits = CellArrayToIntArray(sudokuGrid);
			SudokuSolutionCount solutions = SudokuSolver.DoesSudokuHaveMoreThanOneSolution(sudokuDigits);
			// revert if change results in more than one possible solution
			if (solutions != SudokuSolutionCount.One)
			{
				selectedCell.value = originalCellValue;
			}

		}
#else
		List<Point> uniqueRandomCoordinates = GetUniqueRandomPoints(numbersToRemove);
		for (int i = numbersToRemove - 1; i >= 0; i--)
		{
			sudokuGrid![uniqueRandomCoordinates[i].x, uniqueRandomCoordinates[i].y].value = emptyCellValue;
		}
#endif
	}

	/// <summary>
	/// Converts the sudoku board into a 81-character string, useful for debugging, for example
	/// "...79.5.....1.5.89..8.6......5.....6...2....48...7619..37.8..2........3.412......"
	/// </summary>
	/// <returns></returns>
	private string SudokuToString()
	{
		char[] chars = new char[sideLength * sideLength];
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				int position = i * sideLength + j;
				int cellValue = sudokuGrid![i, j].value;
				chars[position] = cellValue == emptyCellValue ? SudokuGridSaver.blank : (char)(cellValue + SudokuGridSaver.asciiOffset);
			}
		}
		return new string(chars);
	}

	/// <summary>
	/// Counts the number of empty cells
	/// </summary>
	/// <returns></returns>
	private int GetEmptyCellsCount()
	{
		int emptyCells = 0;
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				if (sudokuGrid![i, j].value == emptyCellValue) emptyCells++;
			}
		}
		return emptyCells;
	}

	/// <summary>
	/// Fills in the digits of a newly generated sudoku
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	private bool FillInSudokuGrid(int x, int y)
	{
		if (x == sideLength)
		{
			y++;
			x = 0;
		}
		if (y == sideLength)
		{
			return true;
		}
		int[] digits = GetRandomlySortedDigitArray();
		Cell currentCell = sudokuGrid![x, y];
		for (int i = 0; i < sideLength; i++)
		{
			int digit = digits[i];
			if (IsStepValid(currentCell, digit, ref sudokuGrid))
			{
				currentCell.value = digit;
				if (FillInSudokuGrid(x + 1, y))
				{
					return true;
				}
			}
		}
		currentCell.value = emptyCellValue;
		return false;
	}

	/// <summary>
	/// Creates an array of 9 non-repeating digits in a random order
	/// </summary>
	/// <returns></returns>
	private int[] GetRandomlySortedDigitArray()
	{
		int[] digits = new int[sideLength];
		for (int i = 0; i < sideLength; i++)
		{
			digits[i] = i + 1;
		}
		for (int i = 0; i < sideLength; i++)
		{
			int randomIndex = random.Next(sideLength);
			(digits[i], digits[randomIndex]) = (digits[randomIndex], digits[i]);
		}
		return digits;
	}

#if !OnlyOneSolutionPossible
	/// <summary>
	/// Creates a <see cref="List{Point}"/> of <see cref="Point"/>s with unique coordinates
	/// </summary>
	/// <param name="desiredLength"></param>
	/// <returns></returns>
	private List<Point> GetUniqueRandomPoints(int desiredLength)
	{
		List<List<Point>> array = new List<List<Point>>(sideLength)
		{
			new List<Point>(), new List<Point>(), new List<Point>(), new List<Point>(), new List<Point>(), new List<Point>(), new List<Point>(), new List<Point>(), new List<Point>()
		};
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				array[i].Add(new Point(i, j));
			}
		}
		int totalArrayCount = sideLength * sideLength;
		int[] subarrayCounts = new int[] { 9, 9, 9, 9, 9, 9, 9, 9, 9 };
		while (totalArrayCount > desiredLength)
		{
			int selectedSubArrayIndex = random.Next(sideLength);
			if (subarrayCounts[selectedSubArrayIndex] == 0) continue;

			array[selectedSubArrayIndex].RemoveAt(random.Next(subarrayCounts[selectedSubArrayIndex]));
			subarrayCounts[selectedSubArrayIndex]--;
			totalArrayCount--;
		}
		List<Point> finalArray = new List<Point>();
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				if (j < array[i].Count)
				{
					finalArray.Add(array[i][j]);
				}
				else
				{
					continue;
				}
			}
		}

		return finalArray;
	}
#endif

	/// <summary>
	/// Fills in an empty sudoku grid with <see cref="Cell"/>s
	/// </summary>
	private void GenerateGrid()
	{
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				sudokuGrid![i, j] = new Cell(emptyCellValue, new Point(i, j));
			}
		}
	}

	public delegate void GenerationCompleteHandler(int[,] sudokuGrid);
	public event GenerationCompleteHandler? GenerationComplete;

	/// <summary>
	/// Raises an event when sudoku generation is complete
	/// </summary>
	private void OnGenerationComplete()
	{
		GenerationComplete?.Invoke(CellArrayToIntArray(sudokuGrid));
	}
}
