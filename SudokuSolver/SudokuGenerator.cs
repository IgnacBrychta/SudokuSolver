namespace SudokuSolver;

public sealed class SudokuGenerator : Sudoku
{
	Cell[,]? sudokuGrid = new Cell[sideLength, sideLength];
	public Difficulty difficulty;
	Random random = new Random();
	public SudokuGenerator(Difficulty difficulty)
	{
		this.difficulty = difficulty;
	}

	public SudokuGenerator() { }

	public async Task GenerateAsync()
	{
		await Task.Run(() =>
		{
			GenerateGrid();
			Iterate(x: 0, y: 0);
		});
		ApplySelectedDifficulty();
		OnGenerationComplete();
	}

	private void ApplySelectedDifficulty()
	{
		int numbersToRemove = 0;
		switch (difficulty)
		{
			case Difficulty.EASY:
				numbersToRemove = 30;
				break;
			case Difficulty.MEDIUM:
				numbersToRemove = 45;
				break;
			case Difficulty.HARD:
				numbersToRemove = 60;
				break;
			default:
				return;
		}

		List<Point> uniqueRandomCoordinates = GetUniqueRandomPoints(numbersToRemove);
		for (int i = numbersToRemove - 1; i >= 0; i--)
		{
			sudokuGrid![uniqueRandomCoordinates[i].x, uniqueRandomCoordinates[i].y].value = emptyCellValue;
		}
	}

	private bool Iterate(int x, int y)
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
				if (Iterate(x + 1, y))
				{
					return true;
				}
			}
		}
		currentCell.value = emptyCellValue;
		return false;
	}

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
	private void OnGenerationComplete()
	{
		GenerationComplete?.Invoke(CellArrayToIntArray(sudokuGrid));
	}
}
