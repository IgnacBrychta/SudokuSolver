using System.Diagnostics;

namespace SudokuSolver;

/// <summary>
/// Class for finding a solution to a sudoku
/// </summary>
public class SudokuSolver : Sudoku
{
	public int[,] SudokuGrid
	{
		get => CellArrayToIntArray(sudokuGrid);
		set
		{
			if (value.GetLength(0) != sideLength || value.GetLength(1) != sideLength)
			{
				throw new ArgumentException("Grid either not initialized or is incorrectly sized");
			}
			sudokuGrid = CreateCellGrid(value);
			if (!CheckIfSudokuValid())
			{
				OnSudokuUnsolvable();
				sudokuGrid = null;
				return;
			}
			lastSudokuGridIntArray = value;
		}
	}

	int[,]? lastSudokuGridIntArray;
	Cell[,]? sudokuGrid;
	readonly Stopwatch stopwatch = new Stopwatch();
	int _updateDelay = default;
	public int UpdateDelay
	{
		get => _updateDelay;
		set
		{
			_updateDelay = Math.Abs(value);
		}
	}
	int _iterationDelay = default;
	public int IterationDelay
	{
		get => _iterationDelay;
		set
		{
			_iterationDelay = Math.Abs(value);
		}
	}
	public long TimeSpentSolving { get => stopwatch.ElapsedMilliseconds; }

	public SudokuSolver(int[,] filledSudokuGrid, int updateDelay)
	{
		sudokuGrid = CreateCellGrid(filledSudokuGrid);
		UpdateDelay = updateDelay;
	}

	public SudokuSolver(int[,] filledSudokuGrid)
	{
		SudokuGrid = filledSudokuGrid;
	}

	public SudokuSolver(int updateDelay)
	{
		UpdateDelay = updateDelay;
	}

	public SudokuSolver() { }


	/// <summary>
	/// Starts solving <see cref="SudokuGrid"/> asynchronously
	/// Raises an event <see cref="OnSudokuSolved"/> if the sudoku has been solved
	/// Raises an event <see cref="OnSudokuUnsolvable"</see> if the sudoku is unsolvable
	/// Raises an event <see cref="RaiseIterationEventContinously"/> every <see cref="IterationDelay"/> milliseconds if <see cref="IterationDelay"/> is not equal to 0
	/// </summary>
	/// <returns></returns>
	public async Task SolveAsync()
	{
		if (sudokuGrid is null) return;
		bool sudokuSolved = false;
		stopwatch.Restart();
		await Task.Run(() =>
		{
			if (_iterationDelay != default)
			{
				_ = RaiseIterationEventContinouslyAsync();
				sudokuSolved = SolveSudokuWithProgress();
			}
			else
			{
				sudokuSolved = SolveSudoku();
			}
		});
		stopwatch.Stop();
		if (sudokuSolved)
		{
			OnSudokuSolved();
		}
		else
		{
			OnSudokuUnsolvable();
		}
	}

	/// <summary>
	/// Raises the <see cref="OnNewIterationCompleted"/> event every <see cref="UpdateDelay"/> milliseconds
	/// </summary>
	/// <returns></returns>
	private async Task RaiseIterationEventContinouslyAsync()
	{
		await Task.Run(async () =>
		{
			while (stopwatch.IsRunning)
			{
				OnNewIterationCompleted();
				await Task.Delay(_updateDelay);
			}
		});
	}


	/// <summary>
	/// Checks the <see cref="sudokuGrid"/> whether it violates the rules of sudoku or not
	/// </summary>
	/// <returns></returns>
	public bool CheckIfSudokuValid()
	{
		for (int i = 0; i < sideLength; i++)
		{
			SudokuSquare square = (SudokuSquare)Math.Pow(2, i);
			var rowDigits = GetRowDigits(i, ref sudokuGrid);
			var columnDigits = GetColumnDigits(i, ref sudokuGrid);
			var subgridDigits = GetDigitsInSquare(square, ref sudokuGrid);
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
	/// Recursively solves the <see cref="SudokuGrid"/>
	/// Sleeps for <see cref="IterationDelay"/> milliseconds after each recursive call
	/// </summary>
	/// <returns>A <see cref="Boolean"/> whether the sudoku can be solved</returns>
	private bool SolveSudokuWithProgress()
	{
		Thread.Sleep(_iterationDelay);
		Cell? nextEmptyCell = FindEmptyCell();
		if (nextEmptyCell is null) return true;

		for (int digit = smallestDigit; digit <= largestDigit; digit++)
		{
			if (IsStepValid(nextEmptyCell, digit, ref sudokuGrid))
			{
				sudokuGrid![nextEmptyCell.location.x, nextEmptyCell.location.y].value = digit;

				if (SolveSudokuWithProgress()) return true;

				sudokuGrid[nextEmptyCell.location.x, nextEmptyCell.location.y].value = emptyCellValue;
			}
		}
		return false;
	}

	/// <summary>
	/// Recursively solves the <see cref="SudokuGrid"/>
	/// </summary>
	/// <returns>A <see cref="Boolean"/> whether the sudoku can be solved</returns>
	private bool SolveSudoku()
	{
		Cell? nextEmptyCell = FindEmptyCell();
		if (nextEmptyCell is null) return true;

		for (int digit = smallestDigit; digit <= largestDigit; digit++)
		{
			if (IsStepValid(nextEmptyCell, digit, ref sudokuGrid))
			{
				sudokuGrid![nextEmptyCell.location.x, nextEmptyCell.location.y].value = digit;

				if (SolveSudoku()) return true;

				sudokuGrid[nextEmptyCell.location.x, nextEmptyCell.location.y].value = emptyCellValue;
			}
		}
		return false;
	}

	/// <summary>
	/// Counts all possible solutions of <see cref="SudokuGrid"/>
	/// </summary>
	/// <returns>A <see cref="Boolean"/> whether the sudoku can be solved</returns>
	private int FindAllPossibleSolutionsToSudoku()
	{
		// local grid not to disturb the stored one
		Cell[,]? localSudokuGrid = CreateCellGrid(lastSudokuGridIntArray!);
		int solutions = 0;
		int[] algorithmPosition = new[] { sideLength - 1, sideLength - 1 };
		bool CountSolutionsRecursively()
		{
			Cell? nextEmptyCell = FindEmptyCell();
			if (nextEmptyCell is null)
			{
				solutions++;
				if (NextFilledInCellPosition(ref algorithmPosition))
				{
					nextEmptyCell = localSudokuGrid![algorithmPosition[0], algorithmPosition[1]];
				}
				else
				{
					return true;
				}
			}

			for (int digit = smallestDigit; digit <= largestDigit; digit++)
			{
				if (IsStepValid(nextEmptyCell, digit, ref localSudokuGrid))
				{
					localSudokuGrid![nextEmptyCell.location.x, nextEmptyCell.location.y].value = digit;

					if (CountSolutionsRecursively()) return true;

					localSudokuGrid[nextEmptyCell.location.x, nextEmptyCell.location.y].value = emptyCellValue;
				}
			}
			return false;
		}
		_ = CountSolutionsRecursively();
		return solutions;
	}

	/// <summary>
	/// When <see cref="DoesSudokuHaveMoreThanOneSolution"/> or <see cref="FindAllPossibleSolutionsToSudoku"/>
	/// find a valid solution, this method provides them with a new position to try and find a new possible
	/// solution
	/// </summary>
	/// <returns></returns>
	private static bool NextFilledInCellPosition(ref int[] algorithmPosition)
	{
		algorithmPosition[1]--;
		if (algorithmPosition[1] < 0)
		{
			algorithmPosition[1] = sideLength - 1;
			algorithmPosition[0]--;

			if (algorithmPosition[0] < 0)
			{
				algorithmPosition[0] = sideLength - 1;
				algorithmPosition[1] = sideLength - 1;
				return false;
			}
			return true;
		}
		return true;
	}

	/// <summary>
	/// Checks if sudoku has more than one solution
	/// </summary>
	/// <returns><see cref="SudokuSolutionCount"/> whether the sudoku has zero, one or multiple solutions</returns>
	internal static SudokuSolutionCount DoesSudokuHaveMoreThanOneSolution(int[,] lastSudokuGridIntArray)
	{
		// local grid not to disturb the stored one
		Cell[,]? localSudokuGrid = CreateCellGrid(lastSudokuGridIntArray!);
		
		int solutionCount = 0;
		int[] algorithmPosition = new[] { sideLength - 1, sideLength - 1 };
		bool FindSolutionsRecursively()
		{
			Cell? nextEmptyCell = FindEmptyCell(localSudokuGrid);
			if (nextEmptyCell is null)
			{
				solutionCount++;
				if (solutionCount > 1) return true;
				if (NextFilledInCellPosition(ref algorithmPosition))
				{
					nextEmptyCell = localSudokuGrid![algorithmPosition[0], algorithmPosition[1]];
				}
				else
				{
					return true;
				}
			}

			for (int digit = smallestDigit; digit <= largestDigit; digit++)
			{
				if (IsStepValid(nextEmptyCell, digit, ref localSudokuGrid))
				{
					localSudokuGrid![nextEmptyCell.location.x, nextEmptyCell.location.y].value = digit;

					if (FindSolutionsRecursively()) return true;

					localSudokuGrid[nextEmptyCell.location.x, nextEmptyCell.location.y].value = emptyCellValue;
				}
			}
			return false;
		}
		_ = FindSolutionsRecursively();
		return solutionCount switch
		{
			0 => SudokuSolutionCount.Zero,
			1 => SudokuSolutionCount.One,
			_ => SudokuSolutionCount.Multiple
		};
	}

	/// <summary>
	/// Finds the first empty <see cref="Cell"/> (no digit) of each column of each row
	/// </summary>
	/// <returns>The first <see cref="Cell"/> with no digit,
	/// <see cref="null"/> if all cells are filled in</returns>
	private Cell? FindEmptyCell()
	{
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				Cell cell = sudokuGrid![i, j];
				if (cell.value == emptyCellValue) return cell;
			}
		}
		return null;
	}

	/// <summary>
	/// Finds the first empty <see cref="Cell"/> (no digit) of each column of each row
	/// </summary>
	/// <returns>The first <see cref="Cell"/> with no digit,
	/// <see cref="null"/> if all cells are filled in</returns>
	private static Cell? FindEmptyCell(in Cell[,] sudokuGrid)
	{
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				Cell cell = sudokuGrid![i, j];
				if (cell.value == emptyCellValue) return cell;
			}
		}
		return null;
	}

	public delegate void NewIterationCompletedHandler(int[,] sudokuGrid);
	public event NewIterationCompletedHandler? NewIterationCompleted;
	public delegate void SudokuSolvedHandler(int[,] sudokuGrid);
	public event SudokuSolvedHandler? SudokuSolved;
	public delegate void SudokuUnsolvableHandler();
	public event SudokuUnsolvableHandler? SudokuUnsolvable;

	/// <summary>
	/// Raised every <see cref="IterationDelay"/> milliseconds when solving and with progress indication turned on
	/// </summary>
	protected virtual void OnNewIterationCompleted()
	{
		NewIterationCompleted?.Invoke(CellArrayToIntArray(sudokuGrid));
	}

	/// <summary>
	/// Raised when sudoku is solved
	/// </summary>
	protected virtual void OnSudokuSolved()
	{
		SudokuSolved?.Invoke(CellArrayToIntArray(sudokuGrid));
	}

	/// <summary>
	/// Raised when sudoku is not solvable
	/// </summary>
	protected virtual void OnSudokuUnsolvable()
	{
		SudokuUnsolvable?.Invoke();
	}

}
