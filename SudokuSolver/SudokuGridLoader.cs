using System.Text;

namespace SudokuSolver;

internal static class SudokuGridLoader
{
	const char blank = '.';
	const int sideLength = 9;
	const char verticalLine = '|';
	const char horizontalLine = '-';
	const int noDigit = -1;
	internal static int[,]? LoadSudoku()
	{
		OpenFileDialog ofd = new OpenFileDialog()
		{
			FileName = "sodoku.ss",
			Title = "Select directory where to load a sudoku grid.",
			Filter = "Simple Sudoku file|*.ss"
		};
		DialogResult result = ofd.ShowDialog();
		if (result == DialogResult.OK)
		{
			int[,]? sudokuGrid = LoadSudokuFromFile(ofd.FileName);
			if (sudokuGrid is null) ShowError();
			return sudokuGrid;
		}
		else
		{
			ShowError();
		}
		return null;
	}

	private static void ShowError()
	{
		MessageBox.Show(
				"Sudoku could not be loaded.",
				"Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error
				);
	}

	private static int[,]? LoadSudokuFromFile(string filename)
	{
		using FileStream fs = new FileStream(filename, FileMode.Open);
		using StreamReader sr = new StreamReader(fs, Encoding.Unicode);

		int[,] sudokuDigits = new int[sideLength, sideLength];
		string rawSudokuGrid = sr.ReadToEnd();
		string[] sudokuGridLines;

		try
		{
			sudokuGridLines = rawSudokuGrid
			.Split('\n')
			.Where(line => !line.StartsWith(horizontalLine)) // removes horizontal lines that are just for human readability
			.Select(line => line.Replace(verticalLine.ToString(), "")) // removes vertical lines tha are just for human readability
			.ToArray();
		}
		catch(Exception)
		{
			return null;
		}

		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				int digit;
				try
				{
					digit = GetParsedSudokuDigit(sudokuGridLines[i][j]);
				}
				catch (Exception) { return null; }
				sudokuDigits[i, j] = digit;
			}
		}
		return sudokuDigits;
	}

	private static int GetParsedSudokuDigit(char digit)
	{
		int numericValue = default;
		if(digit != blank)
		{
			numericValue = (int)char.GetNumericValue(digit);
			if (numericValue == noDigit) throw new FormatException();
		}
		return numericValue;
	}
}
