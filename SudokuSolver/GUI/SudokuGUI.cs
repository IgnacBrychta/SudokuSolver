namespace SudokuSolver;

/// <summary>
/// Controls the Sudoku GUI
/// </summary>
public partial class SudokuGUI : Form
{
	readonly SudokuSolver? sudokuSolver;
	readonly SudokuGenerator? sudokuGenerator;
	const int sideLength = 9;
	readonly RichTextBox[,] sudokuGridGUI = new RichTextBox[sideLength, sideLength];
	readonly Color background = Color.White;
	readonly Color newDigit = Color.Beige;
	int iterationDelay = default;
	bool applyIterationDelay = false;
	readonly Color textBoxColorOK = Color.Green;
	readonly Color textBoxColorNotOK = Color.Red;
	bool suspendInputChecking = false;
	bool iterationDelayValid = true;

	/// <summary>
	/// Called when starting the program, it's responsible for rescaling the controls
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void SudokuGUI_Load(object? sender, EventArgs e)
	{
		if (Screen.PrimaryScreen is null) { return; }
		AutoScaleMode = AutoScaleMode.None;

		float devMonitorWidth = 1920;
		float devMonitorHeight = 1080;

		float scaleX = devMonitorWidth / Screen.PrimaryScreen.Bounds.Width;
		float scaleY = devMonitorHeight / Screen.PrimaryScreen.Bounds.Height;
		float scalingFactor = Math.Min(scaleX, scaleY);

		Scale(new SizeF(scalingFactor, scalingFactor));

		// Adjust font sizes of controls
		UpdateControlFontSizes(this, scalingFactor);
	}

	/// <summary>
	/// Rescales fonts based on screen resolution
	/// </summary>
	/// <param name="control"></param>
	/// <param name="scalingFactor"></param>
	private void UpdateControlFontSizes(Control control, float scalingFactor)
	{
		control.Font = new Font(control.Font.FontFamily, control.Font.Size * scalingFactor, control.Font.Style);

		foreach (Control childControl in control.Controls)
		{
			UpdateControlFontSizes(childControl, scalingFactor);
		}
	}

	/// <summary>
	/// Initializes the sudoku grid, subscribes to events by sudokuSolver and sudokuGenerator
	/// </summary>
	public SudokuGUI()
	{
		InitializeComponent();
		Load += SudokuGUI_Load;
		ConfigSudokuGrid();
		sudokuGridGUI = new RichTextBox[sideLength, sideLength]
		{
			{ richTextBox1, richTextBox2, richTextBox3, richTextBox4, richTextBox5, richTextBox6, richTextBox7, richTextBox8, richTextBox9 },
			{ richTextBox10, richTextBox11, richTextBox12, richTextBox13, richTextBox14, richTextBox15, richTextBox16, richTextBox17, richTextBox18 },
			{ richTextBox19, richTextBox20, richTextBox21, richTextBox22, richTextBox23, richTextBox24, richTextBox25, richTextBox26, richTextBox27 },
			{ richTextBox28, richTextBox29, richTextBox30, richTextBox31, richTextBox32, richTextBox33, richTextBox34, richTextBox35, richTextBox36 },
			{ richTextBox37, richTextBox38, richTextBox39, richTextBox40, richTextBox41, richTextBox42, richTextBox43, richTextBox44, richTextBox45 },
			{ richTextBox46, richTextBox47, richTextBox48, richTextBox49, richTextBox50, richTextBox51, richTextBox52, richTextBox53, richTextBox54 },
			{ richTextBox55, richTextBox56, richTextBox57, richTextBox58, richTextBox59, richTextBox60, richTextBox61, richTextBox62, richTextBox63 },
			{ richTextBox64, richTextBox65, richTextBox66, richTextBox67, richTextBox68, richTextBox69, richTextBox70, richTextBox71, richTextBox72 },
			{ richTextBox73, richTextBox74, richTextBox75, richTextBox76, richTextBox77, richTextBox78, richTextBox79, richTextBox80, richTextBox81 }
		};
		ConfigUI();

		sudokuSolver = new SudokuSolver(10);
		sudokuGenerator = new SudokuGenerator();
		sudokuSolver!.NewIterationCompleted += UpdateSudokuGridGUI;
		sudokuSolver!.SudokuSolved += SudokuSolved;
		sudokuSolver!.SudokuUnsolvable += SudokuUnsolvable;
		sudokuGenerator!.GenerationComplete += SudokuGenerated;
	}

	/// <summary>
	/// Called when a new sudoku grid is generated
	/// </summary>
	/// <param name="sudokuGrid"></param>
	private void SudokuGenerated(int[,] sudokuGrid)
	{
		suspendInputChecking = true;
		UpdateSudokuGridGUI(sudokuGrid);
		suspendInputChecking = false;
		comboBox_generateSudoku.Enabled = true;
		label_solveTimeIndicator.Text = sudokuGenerator!.TimeSpentSolving.ToString() + " ms";
	}

	/// <summary>
	/// Updates the sudoku grid GUI with digits
	/// </summary>
	/// <param name="sudokuGrid"></param>
	private void UpdateSudokuGridGUI(int[,] sudokuGrid)
	{
		Invoke(() =>
		{
			for (int i = 0; i < sideLength; i++)
			{
				for (int j = 0; j < sideLength; j++)
				{
					string solvedDigit = sudokuGrid[i, j].ToString();
					RichTextBox textBox = sudokuGridGUI[i, j];
					if (textBox.Text == solvedDigit)
					{
						textBox.BackColor = background;
					}
					else
					{
						textBox.BackColor = newDigit;
						textBox.Text = solvedDigit != "-1"
						? solvedDigit
						: "";
					}
					textBox.Update();
				}
			}
		});
	}

	/// <summary>
	/// Sets the right tab index for the sudoku grid, subscribing to clicks on UI elements
	/// </summary>
	private void ConfigUI()
	{
		button_solve.Click += Button_solve_Click;
		button_reset.Click += Button_reset_Click;
		button_load.Click += Button_load_Click;
		button_save.Click += Button_save_Click;
		button_checkSolved.Click += Button_checkSolved_Click;
		button_repeatingDigits.Click += Button_repeatingDigits_Click;
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				sudokuGridGUI[i, j].TabIndex = i * sideLength + j + 2;
			}
		}
	}

	/// <summary>
	/// Called when the user clicks on the Verify button
	/// Used for when the user wants to verify whether the sudoku on screen is solved
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Button_checkSolved_Click(object? sender, EventArgs e)
	{
		if (Sudoku.CheckIfSudokuSolved(GetSudokuDigits()))
		{
			MessageBox.Show(
				"This sudoku is solved.",
				"Solved",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information
				);
		}
		else
		{
			MessageBox.Show(
				"This sudoku is not solved.",
				"Not solved",
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation
				);
		}
	}

	/// <summary>
	/// Called when the user clicks on the Verify button
	/// Used for when the user wants to verify whether the sudoku on screen is solved
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Button_repeatingDigits_Click(object? sender, EventArgs e)
	{
		if (Sudoku.CheckIfSudokuValid(GetSudokuDigits()))
		{
			MessageBox.Show(
				"This sudoku is valid.",
				"Valid",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information
				);
		}
		else
		{
			MessageBox.Show(
				"This sudoku is invalid.",
				"Invalid",
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation
				);
		}
	}

	/// <summary>
	/// Called when user clicks on the save button
	/// Tries to save the sudoku to a file
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Button_save_Click(object? sender, EventArgs e)
	{
		int[,] sudokuDigits = GetSudokuDigits();
		SudokuGridSaver.SaveSudoku(sudokuDigits);
	}

	/// <summary>
	/// Called when user clicks on the load button
	/// Tries to load the sudoku from a file
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Button_load_Click(object? sender, EventArgs e)
	{
		int[,]? sudokuDigits = SudokuGridLoader.LoadSudoku();
		if (sudokuDigits is not null)
		{
			UpdateSudokuGridGUI(sudokuDigits);
			ResetTimeIndicator();
		}
	}

	/// <summary>
	/// Called when the users clicks on the reset button
	/// Resets the sudoku grid
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Button_reset_Click(object? sender, EventArgs e)
	{
		ResetSudokuGridBackground();
		ResetTimeIndicator();
	}

	/// <summary>
	/// Resets the time spent solving label
	/// </summary>
	private void ResetTimeIndicator()
	{
		label_solveTimeIndicator.Text = "--- ms";
	}

	/// <summary>
	/// Clears the sudoku grid of all digits
	/// </summary>
	private void ResetSudokuGridBackground()
	{
		for (int i = 0; i < sideLength; i++)
		{
			for (int j = 0; j < sideLength; j++)
			{
				RichTextBox textBox = sudokuGridGUI[i, j];
				textBox.BackColor = background;
				textBox.Text = string.Empty;
			}
		}
	}

	/// <summary>
	/// Converts the name of a sudoku grid cell to its coordinates
	/// </summary>
	/// <param name="buttonName"></param>
	/// <param name="row"></param>
	/// <param name="column"></param>
	private static void GetRowAndColumnFromButton(string buttonName, out int row, out int column)
	{
		int splitAt = 11;
		string str_buttonNumber = buttonName[splitAt..];
		int buttonNumber = int.Parse(str_buttonNumber);
		row = buttonNumber / sideLength;
		column = buttonNumber % sideLength - 1;
		if (column < 0)
		{
			column = sideLength - 1;
			row--;
		}
	}

	/// <summary>
	/// Extracts the digits from the sudoku grid
	/// </summary>
	/// <returns>A two-dimensional int array of sudoku digits</returns>
	private int[,] GetSudokuDigits()
	{
		int[,] sudokuDigits = new int[sideLength, sideLength];
		foreach (var cell in GetSudokuGridCells())
		{
			GetRowAndColumnFromButton(cell.Name, out int row, out int column);
			sudokuDigits[row, column] = int.TryParse(cell.Text, out int number) ? number : -1;
		}
		return sudokuDigits;
	}

	/// <summary>
	/// Called when the user clicks on the solve button
	/// Tries to solve the sudoku if iteration delay is valid or not requested
	/// Disables GUI buttons
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Button_solve_Click(object? sender, EventArgs e)
	{
		if (applyIterationDelay && !iterationDelayValid)
		{
			MessageBox.Show(
				"Cannot solve, iteration delay is checked, but its value is invalid.",
				"Cannot solve",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error
				);
			return;
		}

		int[,] sudokuDigits = GetSudokuDigits();

		DisableButtons();
		sudokuSolver!.SudokuGrid = sudokuDigits;
		if (iterationDelay != default && applyIterationDelay) sudokuSolver.IterationDelay = iterationDelay;
		else sudokuSolver.IterationDelay = default;
		_ = sudokuSolver.SolveAsync();
	}

	/// <summary>
	/// Called when the sudoku grid is unsolvable
	/// Enables GUI buttons
	/// </summary>
	private void SudokuUnsolvable()
	{
		Invoke(() =>
		{
			EnableButtons();
			MessageBox.Show("The specified sudoku is unsolvable.", "Sudoku unsolvable", MessageBoxButtons.OK, MessageBoxIcon.Error);
		});
	}

	/// <summary>
	/// Called when the sudoku grid has been solved
	/// Enables GUI buttons
	/// Updates the sudoku grid with solved digits
	/// </summary>
	/// <param name="sudokuGrid"></param>
	private void SudokuSolved(int[,] sudokuGrid)
	{
		Invoke(() => {
			EnableButtons();
			label_solveTimeIndicator.Text = sudokuSolver!.TimeSpentSolving.ToString() + " ms";
		});
		UpdateSudokuGridGUI(sudokuGrid);

	}

	/// <summary>
	/// Disables GUI buttons
	/// </summary>
	private void DisableButtons()
	{
		button_solve.Enabled = false;
		button_reset.Enabled = false;
		checkBox1.Enabled = false;
		richTextBox_setSolvingDelay.Enabled = false;
		comboBox_generateSudoku.Enabled = false;
		button_load.Enabled = false;
		button_save.Enabled = false;
		button_checkSolved.Enabled = false;
		button_repeatingDigits.Enabled = false;
	}

	/// <summary>
	/// Enables GUI buttons
	/// </summary>
	private void EnableButtons()
	{
		button_solve.Enabled = true;
		button_reset.Enabled = true;
		checkBox1.Enabled = true;
		richTextBox_setSolvingDelay.Enabled = true;
		comboBox_generateSudoku.Enabled = true;
		button_load.Enabled = true;
		button_save.Enabled = true;
		button_checkSolved.Enabled = true;
		button_repeatingDigits.Enabled = true;
	}

	/// <summary>
	/// Finds and returns all sudoku grid cells
	/// </summary>
	/// <returns><see cref="IEnumerable{RichTextBox}"/> of sudoku grid cells</returns>
	private IEnumerable<RichTextBox> GetSudokuGridCells()
	{
		Control.ControlCollection controlCollection = groupBox1.Controls;
		for (int i = 0; i < controlCollection.Count; i++)
		{
			Control.ControlCollection cellGroup = controlCollection[i].Controls;
			for (int j = 0; j < sideLength; j++)
			{
				Control control = cellGroup[j];
				if (control is not RichTextBox cell) continue;
				if (!IsSudokuGridCell(cell)) continue;
				yield return cell;
			}
		}
		yield break;
	}

	/// <summary>
	/// Subscribes all sudoku grid cells TextChanged events to <see cref="Cell_TextChanged(object?, EventArgs)"/>
	/// </summary>
	private void ConfigSudokuGrid()
	{
		foreach (var cell in GetSudokuGridCells())
		{
			cell.TextChanged += Cell_TextChanged;
		}
	}

	/// <summary>
	/// Input check to ensure the user only enters digits into the sudoku grid cells
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Cell_TextChanged(object? sender, EventArgs e)
	{
		if (suspendInputChecking) return;
		RichTextBox? cell = sender as RichTextBox;

		string cellText;
		char lastChar = 'x';
		int number;
		if (cell!.Text.Length == 1) cellText = cell!.Text;
		else if (cell!.Text.Length == 0) return;
		else
		{
			cellText = cell!.Text[^1..];
			lastChar = cell.Text[1];
		}

		if (int.TryParse(cellText, out number) && number != 0)
		{
			cell.Text = cellText;
		}
		else
		{
			cell.Text = cell.Text[..1];
			if (!int.TryParse(cell.Text, out _) || lastChar == ' ' || number == 0) cell.Text = string.Empty;
		}
		cell.SelectionStart = 1; // put cursor right of number
	}

	/// <summary>
	/// Determines whether the <paramref name="cell"/> is a sudoku grid cell
	/// </summary>
	/// <param name="cell"></param>
	/// <returns></returns>
	private static bool IsSudokuGridCell(RichTextBox cell)
	{
		int splitAt = 11;
		return int.TryParse(cell.Name[splitAt..], out _);
	}

	/// <summary>
	/// Input check for show progress delay
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void RichTextBox_setSolvingDelay_TextChanged(object sender, EventArgs e)
	{
		if (richTextBox_setSolvingDelay.Text.Length > 4)
		{
			richTextBox_setSolvingDelay.Text = richTextBox_setSolvingDelay.Text[..4];
		}
		if (int.TryParse(richTextBox_setSolvingDelay.Text, out iterationDelay))
		{
			richTextBox_setSolvingDelay.BackColor = textBoxColorOK;
			iterationDelayValid = true;
		}
		else
		{
			richTextBox_setSolvingDelay.BackColor = textBoxColorNotOK;
			iterationDelayValid = false;
			iterationDelay = 0;
		}
	}

	/// <summary>
	/// Called when users clicks on the check to toggle applying showing progress
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void CheckBox1_CheckedChanged(object sender, EventArgs e)
	{
		applyIterationDelay = !applyIterationDelay;
	}

	/// <summary>
	/// Retrieves <see cref="Difficulty"/> from the <see cref="ComboBox"/> for generating a sudoku
	/// Tries to generate a sudoku if <see cref="Difficulty"/> is selected
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ComboBox_generateSudoku_SelectedIndexChanged(object sender, EventArgs e)
	{
		Difficulty selectedDifficulty = (Difficulty)((ComboBox)sender).SelectedIndex;
		if (selectedDifficulty == Difficulty.NONE) return;
		comboBox_generateSudoku.Enabled = false;
		sudokuGenerator!.difficulty = selectedDifficulty;
		_ = sudokuGenerator!.GenerateAsync();
		ResetSudokuGridBackground();
	}
}