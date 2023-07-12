using System.Text;

namespace SudokuSolver;

internal static class SudokuGridSaver
{
	const char blank = '.';
	const int sideLength = 9;
	const char verticalLine = '|';
	const char horizontalLine = '-';
	const int horizontalLineLength = sideLength + 2;
	const int asciiOffset = 48;
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

	private static char GetParsedSudokuDigit(int digit)
	{
		return digit != -1 ? (char)(digit + asciiOffset) : blank;
	}
}
