using System.Text;

namespace SudokuSolver;

/// <summary>
/// Class for saving a sudoku to a file
/// Uses the Simple Sudoku file format, which is easily readable for a human
/// </summary>
internal static class SudokuGridSaver
{
	public const char blank = '.';
	const int sideLength = 9;
	const char verticalLine = '|';
	const char horizontalLine = '-';
	const int horizontalLineLength = sideLength + 2;
	public const int asciiOffset = 48;
	/// <summary>
	/// Asks the user to select the file location and name to save the sudoku grid to
	/// </summary>
	/// <param name="sudokuDigits"></param>
	internal static void SaveSudoku(in int[,] sudokuDigits)
	{
		SaveFileDialog ofd = new SaveFileDialog() 
		{
			AddExtension = true,
			FileName = "sodoku.ss",
			Title = "Select directory where to save the current sudoku.",
			Filter = "Simple Sudoku file|*.ss"
		};
		DialogResult result = ofd.ShowDialog();
		if (result == DialogResult.OK)
		{
			SaveSudokuToFile(ofd.FileName, sudokuDigits);
		}
		else
		{
			MessageBox.Show(
				"Sudoku could not be saved.",
				"Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error
				);
		}
	}

	/// <summary>
	/// Saves the sudoku to a file specified by <paramref name="filename"/>,
	/// adds vertical and horizontal lines to comply with Simple Sudoku format
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="sudokuDigits"></param>
	private static void SaveSudokuToFile(string filename, in int[,] sudokuDigits) 
	{
		using FileStream fs = new FileStream(filename, FileMode.Create);
		using StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);

		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				char digitToWrite = GetParsedSudokuDigit(sudokuDigits[i, j]);
				sw.Write(digitToWrite);
				if(j == 2 || j == 5)
				{
					sw.Write(verticalLine);
				}
			}
			if(i == 2 || i == 5)
			{	
				sw.Write('\n' + new string(horizontalLine, horizontalLineLength));
			}
			if(i != sideLength -1) sw.Write('\n');
		}
	}

	/// <summary>
	/// Converts <paramref name="digit"/> to <see cref="char"/> with an ASCII offset
	/// </summary>
	/// <param name="digit"></param>
	/// <returns></returns>
	private static char GetParsedSudokuDigit(int digit)
	{
		return digit != -1 ? (char)(digit + asciiOffset) : blank;
	}

}
