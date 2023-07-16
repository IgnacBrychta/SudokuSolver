namespace SudokuSolver;

public partial class SudokuGUI
{
	int[,]? lastGeneratedSudokuGrid;
	readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
	private TimeSpan timeSpentSolving = new TimeSpan();
	const int secondsInMillisecond = 1000;
	readonly TimeSpan oneSecond = new TimeSpan(0, 0, 1);

	/// <summary>
	/// Called when user wants to solve the sudoku themselves
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void CheckBox_lockForSolving_CheckedChanged(object? sender, EventArgs e)
	{
		lockForUserSolving = !lockForUserSolving;
		if (lockForUserSolving) LetUserSolve();
		else RestoreFunctionality();
	}

	/// <summary>
	/// Updates time spent solving on screen
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Timer_Tick(object? sender, EventArgs e)
	{
		Invoke(() =>
		{
			timeSpentSolving += oneSecond;
			label_solveTimeIndicator.Text = timeSpentSolving.ToString();
		});
	}

	/// <summary>
	/// Changes the UI to allow the user to solve the sudoku themselves
	/// </summary>
	private void LetUserSolve()
	{
		timer.Enabled = true;
		LockUI();
		checkBox_lockForSolving.Enabled = true;
		button_checkSolved.Enabled = true;
		button_repeatingDigits.Enabled = true;
		LockGeneratedSudokuDigits();
	}

	/// <summary>
	/// Show user they successfully solved the sudoku
	/// </summary>
	private void CelebrateSuccessfulSolve()
	{
		timer.Enabled = false;
		MessageBox.Show(
				"You have successfully solved this sudoku!",
				"Solved!",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information
				);
	}

	/// <summary>
	/// Unlocks UI elements
	/// </summary>
	private void RestoreFunctionality()
	{
		timer.Enabled = false;
		timeSpentSolving = TimeSpan.Zero;
		UnlockUI();
		UnlockAllSudokuDigits();
	}

	/// <summary>
	/// Locks sudoku UI digits created by this program so that user can solve the sudoku themselves
	/// </summary>
	private void LockGeneratedSudokuDigits()
	{
		if (lastGeneratedSudokuGrid is null) return;

		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				RichTextBox sudokuCell = sudokuGridGUI[i, j];
				int lastDigit = lastGeneratedSudokuGrid[i, j];
				if (lastDigit != Sudoku.emptyCellValue && sudokuCell.Text != string.Empty)
				{
					sudokuCell.Enabled = false;
				}
			}
		}
	}

	/// <summary>
	/// Unlocks all sudoku UI digits locked by <see cref="LockGeneratedSudokuDigits"/>
	/// </summary>
	private void UnlockAllSudokuDigits()
	{
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				RichTextBox sudokuCell = sudokuGridGUI[i, j];
				sudokuCell.Enabled = true;
			}
		}
	}

}
