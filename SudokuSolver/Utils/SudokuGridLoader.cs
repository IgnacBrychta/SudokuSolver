using System.Text;

namespace SudokuSolver;

/// <summary>
/// Provides a way to load a sudoku
/// </summary>
internal static class SudokuGridLoader
{
	const char blank = '.';
	const int sideLength = 9;
	const char verticalLine = '|';
	const char horizontalLine = '-';
	const int noDigit = -1;

	/// <summary>
	/// Asks the user to select a file to load a sudoku from
	/// </summary>
	/// <returns></returns>
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

	/// <summary>
	/// Shows the user a popup error (<see cref="MessageBox"/>) 
	/// </summary>
	private static void ShowError()
	{
		MessageBox.Show(
				"Sudoku could not be loaded.",
				"Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error
				);
	}

	/// <summary>
	/// Opens the file specified, reads its data, parses it and returns a new two-dimensional array of sudoku digits if the data in the file is in the right format
	/// </summary>
	/// <param name="filename"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Tries to convert <paramref name="digit"/> into a sudoku digit
	/// </summary>
	/// <param name="digit"></param>
	/// <returns></returns>
	/// <exception cref="FormatException">Occurs when <paramref name="digit"/> is not a valid sudoku digit</exception>
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
