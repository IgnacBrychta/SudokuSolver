using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Data.Common;

namespace SudokuSolver
{
    public partial class Form1 : Form
    {
        SudokuSolver? sudokuSolver;
        const int sideLength = 9;
        RichTextBox[,] sudokuGridGUI = new RichTextBox[sideLength, sideLength];
        readonly Color background = Color.White;
        readonly Color newDigit = Color.Beige;
        int iterationDelay = default;
        bool applyIterationDelay = false;
        readonly Color textBoxColorOK = Color.Green;
        readonly Color textBoxColorNotOK = Color.Red;
        public Form1()
        {
            InitializeComponent();
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
            sudokuSolver!.NewIterationCompleted += UpdateSudokuGridGUI;
            sudokuSolver!.SudokuSolved += SudokuSolved;
            sudokuSolver!.SudokuUnsolvable += SudokuUnsolvable;
        }
        private void UpdateSudokuGridGUI(int[,] sudokuGrid)
        {
            Invoke(() =>
            {
                label_solveTimeIndicator.Text = sudokuSolver!.TimeSpentSolving.ToString() + " ms";
                for (int i = 0; i < sideLength; i++)
                {
                    for (int j = 0; j < sideLength; j++)
                    {
                        string solvedDigit = sudokuGrid[i, j].ToString();
                        RichTextBox textBox = sudokuGridGUI[i, j];
                        if(textBox.Text == solvedDigit)
                        {
                            textBox.BackColor = background;
                        }
                        else
                        {
                            textBox.BackColor = newDigit;
                            textBox.Text = solvedDigit;
                        }
                        textBox.Update();
                    }
                }
            });
        }
        private void ConfigUI()
        {
            button_solve.Click += Button_solve_Click;
            button_reset.Click += Button_reset_Click;
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    sudokuGridGUI[j, i].TabIndex = i * sideLength + j + 2;
                }
            }
        }

        private void Button_reset_Click(object? sender, EventArgs e)
        {
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    RichTextBox textBox = sudokuGridGUI[i, j];
                    textBox.BackColor = background;
                    textBox.Text = String.Empty;
                }
            }
        }

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
        private void Button_solve_Click(object? sender, EventArgs e)
        {
            int[,] sudokuNumbers = new int[sideLength, sideLength];
            foreach (var cell in GetSudokuGridCells())
            {
                GetRowAndColumnFromButton(cell.Name, out int row, out int column);
                sudokuNumbers[row, column] = int.TryParse(cell.Text, out int number) ? number : -1;
            }

            button_solve.Enabled = false;
            button_reset.Enabled = false;
            checkBox1.Enabled = false;
            richTextBox_setSolvingDelay.Enabled = false;
            sudokuSolver!.SudokuGrid = sudokuNumbers;
            if (iterationDelay != default && applyIterationDelay) sudokuSolver.IterationDelay = iterationDelay;
            else sudokuSolver.IterationDelay = default;
            Task solving = sudokuSolver.SolveAsync();
        }

        private void SudokuUnsolvable()
        {
            BeginInvoke(() =>
            {
                button_solve.Enabled = true;
                button_reset.Enabled = true;
                checkBox1.Enabled = true;
                richTextBox_setSolvingDelay.Enabled = true;
                MessageBox.Show("The specified sudoku is unsolvable.", "Sudoku unsolvable", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void SudokuSolved(int[,] sudokuGrid)
        {
            Invoke(() =>
            {
                button_solve.Enabled = true;
                button_reset.Enabled = true;
                checkBox1.Enabled = true;
                richTextBox_setSolvingDelay.Enabled = true;
                //label_solveTimeIndicator.Text = sudokuSolver!.TimeSpentSolving.ToString() + " ms";
            });
            UpdateSudokuGridGUI(sudokuGrid);
        }
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
        private void ConfigSudokuGrid()
        {
            foreach (var cell in GetSudokuGridCells())
            {
                cell.TextChanged += Cell_TextChanged;
            }
        }
        private void Cell_TextChanged(object? sender, EventArgs e)
        {
            RichTextBox? cell = sender as RichTextBox;

            string cellText;
            char lastChar = 'x';
            int number = -1;
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
        private bool IsSudokuGridCell(RichTextBox cell)
        {
            int splitAt = 11;
            return int.TryParse(cell.Name[splitAt..], out _);
        }
        private void richTextBox_setSolvingDelay_TextChanged(object sender, EventArgs e)
        {
            if(richTextBox_setSolvingDelay.Text.Length > 4)
            {
                richTextBox_setSolvingDelay.Text = richTextBox_setSolvingDelay.Text[..4];
                return;
            }
            if(int.TryParse(richTextBox_setSolvingDelay.Text, out iterationDelay))
            {
                richTextBox_setSolvingDelay.BackColor = textBoxColorOK;
            }
            else
            {
                richTextBox_setSolvingDelay.BackColor = textBoxColorNotOK;
                iterationDelay = 0;
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            applyIterationDelay = !applyIterationDelay;
        }
    }
    public class SudokuSolver
    {
        public int[,] SudokuGrid {
            get => CellArrayToShortArray(sudokuGrid);
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
        const int sideLength = 9;
        const int emptyCellValue = -1;
        const int smallestDigit = 1;
        const int largestDigit = 9;
        readonly Stopwatch stopwatch = new Stopwatch();
        int _updateDelay = default;
        public int UpdateDelay { 
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
        public SudokuSolver()
        {
        }
        private static Cell[,] CreateCellGrid(int[,] grid)
        {
            Cell[,] cellGrid = new Cell[sideLength, sideLength];
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    int cellValue = grid[i, j];
                    cellGrid[i, j] = new Cell(cellValue, cellValue != emptyCellValue, new Point(i, j));
                }
            }
            return cellGrid;
        }
        public async Task SolveAsync()
        {
            if (sudokuGrid is null) return;
            stopwatch.Restart();
            await Task.Run(async () =>
            {
                if(_iterationDelay != default)
                {
                    Task continuous = RaiseIterationEventContinously();
                    await IterateAsyncWithDelay();
                }
                else
                {
                    await IterateAsync();
                }
                stopwatch.Stop();
                OnSudokuSolved();
            });
        }
        private async Task RaiseIterationEventContinously()
        {
            await Task.Run(async () =>
            {
                while(stopwatch.IsRunning)
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
                var rowDigits = GetRowDigits(i);
                var columnDigits = GetColumnDigits(i);
                var subgridDigits = GetDigitsInSquare(square);
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
        private async Task<bool> IterateAsyncWithDelay()
        {
            await Task.Delay(_iterationDelay);
            Cell? nextEmptyCell = FindEmptyCell();
            if (nextEmptyCell is null) return true;

            for (int digit = smallestDigit; digit <= largestDigit; digit++)
            {
                if(IsStepValid(nextEmptyCell, digit))
                {
                    sudokuGrid![nextEmptyCell.location.x, nextEmptyCell.location.y].value = digit;

                    if (await IterateAsyncWithDelay()) return true;

                    sudokuGrid[nextEmptyCell.location.x, nextEmptyCell.location.y].value = emptyCellValue;
                }
            }
            return false;
        }
        private async Task<bool> IterateAsync()
        {
            Cell? nextEmptyCell = FindEmptyCell();
            if (nextEmptyCell is null) return true;

            for (int digit = smallestDigit; digit <= largestDigit; digit++)
            {
                if (IsStepValid(nextEmptyCell, digit))
                {
                    sudokuGrid![nextEmptyCell.location.x, nextEmptyCell.location.y].value = digit;

                    if (await IterateAsync()) return true;

                    sudokuGrid[nextEmptyCell.location.x, nextEmptyCell.location.y].value = emptyCellValue;
                }
            }
            return false;
        }
        private bool IsStepValid(Cell cell, int digit)
        {
            var rowGroup = GetRowDigits(cell.location.x);
            var columnGroup = GetColumnDigits(cell.location.y);
            var squareGroup = GetDigitsInSquare(PointToSquare(cell.location));
            bool rowOK = !DoesDigitRepeat(digit, rowGroup);
            bool columnOK = !DoesDigitRepeat(digit, columnGroup);
            bool squareOK = !DoesDigitRepeat(digit, squareGroup);
            return rowOK && columnOK && squareOK;
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
        private SudokuSquare PointToSquare(Point point)
        {
            switch((point.x, point.y))
            {
                case ( < 3, < 3):
                    return SudokuSquare.NW;
                case ( < 6, < 3):
                    return SudokuSquare.N;
                case ( < 9, < 3):
                    return SudokuSquare.NE;
                case ( < 3, < 6):
                    return SudokuSquare.W;
                case ( < 6, < 6):
                    return SudokuSquare.CENTER;
                case ( < 9, < 6):
                    return SudokuSquare.E;
                case ( < 3, < 9):
                    return SudokuSquare.SW;
                case ( < 6, < 9):
                    return SudokuSquare.S;
                case ( < 9, < 9):
                    return SudokuSquare.SE;
                default:
                    return SudokuSquare.NONE;
            }
        }
        private bool DoesDigitRepeat(int digit, Dictionary<int, int> group)
        {
            if (digit == emptyCellValue) return false;
            return group[digit] == 1;
        }
        private Dictionary<int, int> GetRowDigits(int row)
        {
            Dictionary<int, int> groupedNumbers = CreateGroupedNumbersDictionary();
            for (int j = 0; j < sideLength; j++)
            {
                int value = sudokuGrid![row, j].value;
                if (value != emptyCellValue) groupedNumbers[value]++;
            }
            return groupedNumbers;
        }
        private Dictionary<int, int> GetColumnDigits(int column)
        {
            Dictionary<int, int> groupedNumbers = CreateGroupedNumbersDictionary();
            for (int i = 0; i < sideLength; i++)
            {
                int value = sudokuGrid![i, column].value;
                if (value != emptyCellValue) groupedNumbers[value]++;
            }
            return groupedNumbers;
        }
        private Dictionary<int, int> GetDigitsInSquare(SudokuSquare square)
        {
            Dictionary<int, int> groupedNumbers = CreateGroupedNumbersDictionary();
            Point squareCoordsMultiplier = map[square];
            int multiplierX = squareCoordsMultiplier.x;
            int multiplierY = squareCoordsMultiplier.y;
            int squareSideLength = 3;
            int limitX = squareSideLength + squareSideLength * (multiplierX + 1);
            int limitY = squareSideLength + squareSideLength * (multiplierY + 1);
            for (int i = squareSideLength + squareSideLength * multiplierX; i < limitX; i++)
            {
                for (int j = squareSideLength + squareSideLength * multiplierY; j < limitY; j++)
                {
                    int value = sudokuGrid![i, j].value;
                    if (value != emptyCellValue) groupedNumbers[value]++;
                }
            }
            return groupedNumbers;
        }
        private Dictionary<int, int> CreateGroupedNumbersDictionary()
        {
            return new Dictionary<int, int>()
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 4, 0 },
                { 5, 0 },
                { 6, 0 },
                { 7, 0 },
                { 8, 0 },
                { 9, 0 }
            };
        }
        public delegate void NewIterationCompletedHandler(int[,] sudokuGrid);
        public event NewIterationCompletedHandler? NewIterationCompleted;
        public delegate void SudokuSolvedHandler(int[,] sudokuGrid);
        public event SudokuSolvedHandler? SudokuSolved;
        public delegate void SudokuUnsolvableHandler();
        public event SudokuUnsolvableHandler? SudokuUnsolvable;
        protected virtual void OnNewIterationCompleted()
        {
            NewIterationCompleted?.Invoke(CellArrayToShortArray(sudokuGrid));
        }
        protected virtual void OnSudokuSolved()
        {
            SudokuSolved?.Invoke(CellArrayToShortArray(sudokuGrid));
        }
        protected virtual void OnSudokuUnsolvable()
        {
            SudokuUnsolvable?.Invoke();
        }
        static int[,] CellArrayToShortArray(Cell[,]? cells)
        {
            int[,] array = new int[sideLength, sideLength];
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    array[i, j] = cells[i, j].value;
                }
            }
            return array;
        }
        protected class Cell
        {
            internal int value;
            internal bool isReadOnly;
            internal Point location;
            internal Cell(int value, bool isReadOnly, Point location)
            {
                this.value = value;
                this.isReadOnly = isReadOnly;
                this.location = location;
            }
            public override string ToString()
            {
                return $"val:{value};{isReadOnly};{location}";
            }
        }
        protected struct Point
        {
            internal int x;
            internal int y;
            internal Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public override string ToString()
            {
                return $"[{x};{y}]";
            }
        }
        enum SudokuSquare
        {
            N = 1,
            NE = 2,
            E = 4,
            SE = 8,
            S = 16,
            SW = 32,
            W = 64,
            NW = 128,
            CENTER = 256,
            NONE = 512
        }

        static readonly Dictionary<SudokuSquare, Point> map = new Dictionary<SudokuSquare, Point>()
        {
            { SudokuSquare.N,      new Point( 0, -1) },
            { SudokuSquare.NE,     new Point( 1, -1) },
            { SudokuSquare.E,      new Point( 1,  0) },
            { SudokuSquare.SE,     new Point( 1,  1) },
            { SudokuSquare.S,      new Point( 0,  1) },
            { SudokuSquare.SW,     new Point(-1,  1) },
            { SudokuSquare.W,      new Point(-1,  0) },
            { SudokuSquare.NW,     new Point(-1, -1) },
            { SudokuSquare.CENTER, new Point( 0,  0) }
        };

    }
}