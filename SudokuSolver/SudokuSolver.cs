using System.Diagnostics;

namespace SudokuSolver;

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
			}
		}
	}
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

	private static Cell[,] CreateCellGrid(int[,] grid)
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

	public async Task SolveAsync()
	{
		if (sudokuGrid is null) return;
		stopwatch.Restart();
		await Task.Run(() =>
		{
			if (_iterationDelay != default)
			{
				Task continuous = RaiseIterationEventContinously();
				bool solved = IterateWithDelay();
				if(!solved) OnSudokuUnsolvable();
			}
			else
			{
				bool solved = Iterate();
				if(!solved) OnSudokuUnsolvable();
			}
		});
		stopwatch.Stop();
		OnSudokuSolved();
	}

	private async Task RaiseIterationEventContinously()
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

	private bool IterateWithDelay()
	{
		Thread.Sleep(_iterationDelay);
		Cell? nextEmptyCell = FindEmptyCell();
		if (nextEmptyCell is null) return true;

		for (int digit = smallestDigit; digit <= largestDigit; digit++)
		{
			if (IsStepValid(nextEmptyCell, digit, ref sudokuGrid))
			{
				sudokuGrid![nextEmptyCell.location.x, nextEmptyCell.location.y].value = digit;

				if (IterateWithDelay()) return true;

				sudokuGrid[nextEmptyCell.location.x, nextEmptyCell.location.y].value = emptyCellValue;
			}
		}
		return false;
	}

	private bool Iterate()
	{
		Cell? nextEmptyCell = FindEmptyCell();
		if (nextEmptyCell is null) return true;

		for (int digit = smallestDigit; digit <= largestDigit; digit++)
		{
			if (IsStepValid(nextEmptyCell, digit, ref sudokuGrid))
			{
				sudokuGrid![nextEmptyCell.location.x, nextEmptyCell.location.y].value = digit;

				if (Iterate()) return true;

				sudokuGrid[nextEmptyCell.location.x, nextEmptyCell.location.y].value = emptyCellValue;
			}
		}
		return false;
	}

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

	public delegate void NewIterationCompletedHandler(int[,] sudokuGrid);
	public event NewIterationCompletedHandler? NewIterationCompleted;
	public delegate void SudokuSolvedHandler(int[,] sudokuGrid);
	public event SudokuSolvedHandler? SudokuSolved;
	public delegate void SudokuUnsolvableHandler();
	public event SudokuUnsolvableHandler? SudokuUnsolvable;
	protected virtual void OnNewIterationCompleted()
	{
		NewIterationCompleted?.Invoke(CellArrayToIntArray(sudokuGrid));
	}

	protected virtual void OnSudokuSolved()
	{
		SudokuSolved?.Invoke(CellArrayToIntArray(sudokuGrid));
	}

	protected virtual void OnSudokuUnsolvable()
	{
		SudokuUnsolvable?.Invoke();
	}
}
