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
        SudokuSolver sudokuSolver;
        const short sideLength = 9;
        public Form1()
        {
            InitializeComponent();
            ConfigSudokuGrid();
            ConfigUI();
            Button_solve_Click(null, new EventArgs());
        }
        private void UpdateSudokuGridGUI(short[,] sudokuGrid)
        {
            short gridIndex = 0;
            foreach (var cell in GetSudokuGridCells())
            {
                BeginInvoke(new Action(() =>
                {
                    cell.Text = sudokuGrid[gridIndex / sideLength, gridIndex % sideLength].ToString();
                    gridIndex++;
                }));
            }
        }
        private void ConfigUI()
        {
            button_solve.Click += Button_solve_Click;
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
            short[,] sudokuNumbers = new short[sideLength, sideLength];
            foreach (var cell in GetSudokuGridCells())
            {
                GetRowAndColumnFromButton(cell.Name, out int row, out int column);
                sudokuNumbers[row, column] = short.TryParse(cell.Text, out short number) ? number : (short)-1;
            }

            sudokuNumbers = new short[sideLength, sideLength]
            {
                { 1, -1, 7, -1, -1, 6, 4, 5, -1 },
                { -1, 2, 5, 3, 4, -1, -1, -1, 8 },
                { -1, 6, -1, -1, -1, 1, -1, 7, -1 },
                { -1, 5, 3, -1, -1, -1, -1, 2, 9 },
                { 6, 1, -1, -1, -1, 9, 8, -1, -1 },
                { -1, -1, -1, 6, -1, 2, -1, -1, 7 },
                { -1, -1, 1, -1, 9, 3, 2, -1, -1 },
                { -1, -1, 8, -1, -1, -1, -1, -1, -1 },
                { -1, 4, -1, -1, 7, 6, 5, 9, 1 }
            };

            sudokuSolver = new SudokuSolver(sudokuNumbers);
            button_solve.Enabled = false;
            sudokuSolver!.NewIterationCompleted += UpdateSudokuGridGUI;
            sudokuSolver!.SudokuSolved += SudokuSolved;
            sudokuSolver!.SudokuUnsolvable += SudokuUnsolvable;
            Task solving = sudokuSolver.Solve();
        }

        private void SudokuUnsolvable()
        {
            BeginInvoke(() =>
            {
                throw new Exception("unsolvable");
            });
        }

        private void SudokuSolved()
        {
            BeginInvoke(() =>
            {
                button_solve.Enabled = true;
            });
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
            short number = -1;
            if (cell!.Text.Length == 1) cellText = cell!.Text;
            else if (cell!.Text.Length == 0) return;
            else
            {
                cellText = cell!.Text[^1..];
                lastChar = cell.Text[1];
            }

            if (short.TryParse(cellText, out number) && number != 0)
            {
                cell.Text = cellText;
            }
            else
            {
                cell.Text = cell.Text[..1];
                if (!short.TryParse(cell.Text, out _) || lastChar == ' ' || number == 0) cell.Text = string.Empty;
            }
            cell.SelectionStart = 1; // put cursor right of number
        }
        private bool IsSudokuGridCell(RichTextBox cell)
        {
            int splitAt = 11;
            return int.TryParse(cell.Name[splitAt..], out _);
        }
    }
    public class SudokuSolver
    {
        Cell[,] sudokuGrid;
        const short sideLength = 9;
        const short emptyCellValue = -1;
        const short smallestDigit = 1;
        const short largestDigit = 9;
        readonly Stopwatch stopwatch = new Stopwatch();
        public long TimeSpentSolving { get => stopwatch.ElapsedMilliseconds; }
        public SudokuSolver(short[,] filledSudokuGrid)
        {
            sudokuGrid = CreateCellGrid(filledSudokuGrid);
        }
        private static Cell[,] CreateCellGrid(short[,] grid)
        {
            Cell[,] cellGrid = new Cell[sideLength, sideLength];
            for (short i = 0; i < sideLength; i++)
            {
                for (short j = 0; j < sideLength; j++)
                {
                    short cellValue = grid[i, j];
                    cellGrid[i, j] = new Cell(cellValue, cellValue != emptyCellValue, new Point(j, i));
                }
            }
            return cellGrid;
        }
        private static Cell Backtrack(Cell original)
        {
            Cell parent = original;
            while(parent.parent is not null)
            {
                parent = parent.parent;
            }
            return parent;
        }
        public async Task Solve()
        {
            await Task.Run(async () =>
            {
                sudokuGrid.GetLength(0);
                Cell? currentCell = FindEmptyCell();
                bool forceCellAssign = false;
                while(true)
                {
                    if(currentCell is null) break;
                    bool cellAssigned = false;
                    for (short i = smallestDigit; i <= largestDigit; i++)
                    {
                        currentCell.value = i;
                        if (cellAssigned = IsStepValid(currentCell))
                        {
                            Cell newCell = FindEmptyCell();
                            newCell!.parent = currentCell;
                            currentCell = newCell;
                            break;
                        }
                    }
                }
                
                OnSudokuUnsolvable();
            });
        }
        private Cell? FindNextCell()
        {
            Cell emptyCell = FindEmptyCell();
            return null;

        }
        private bool IsSudokuSolved()
        {
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    if (sudokuGrid[i, j].value == emptyCellValue) return false;
                }
            }
            return true;
        }
        private bool IsStepValid(Cell cell)
        {
            var rowGroup = GetRowDigits(cell.location.y);
            var columnGroup = GetColumnDigits(cell.location.x);
            var squareGroup = GetDigitsInSquare(PointToSquare(cell.location));
            bool rowOK = !DoesDigitRepeat(cell.value, rowGroup);
            bool columnOK = !DoesDigitRepeat(cell.value, columnGroup);
            bool squareOK = !DoesDigitRepeat(cell.value, squareGroup);
            return !cell.isReadOnly && rowOK && columnOK && squareOK;
        }
        private Cell? FindEmptyCell()
        {
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    Cell cell = sudokuGrid[i, j];
                    if (!cell.isReadOnly && cell.value == emptyCellValue) return cell;
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
        private bool IsGroupComplete(Dictionary<short, short> group, out short missingMemberIndex)
        {
            for (missingMemberIndex = 0; missingMemberIndex < sideLength; missingMemberIndex++)
            {
                if (group[missingMemberIndex] == 0) return false;
            }
            return true;
        }
        private bool DoesDigitRepeat(short digit, Dictionary<short, short> group)
        {
            if (digit == emptyCellValue) return false;
            return group[digit] > 1;
        }
        private Cell GetStartingCell()
        {
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    if(!sudokuGrid[i, j].isReadOnly) return sudokuGrid[i, j];
                }
            }
            throw new ArgumentException("There are no missing numbers");
        }
        private Dictionary<short, short> GetRowDigits(short row)
        {
            Dictionary<short, short> groupedNumbers = CreateGroupedNumbersDictionary();
            for (int j = 0; j < sideLength; j++)
            {
                short value = sudokuGrid[row, j].value;
                if (value != emptyCellValue) groupedNumbers[value]++;
            }
            return groupedNumbers;
        }
        private Dictionary<short, short> GetColumnDigits(short column)
        {
            Dictionary<short, short> groupedNumbers = CreateGroupedNumbersDictionary();
            for (int i = 0; i < sideLength; i++)
            {
                short value = sudokuGrid[i, column].value;
                if (value != emptyCellValue) groupedNumbers[value]++;
            }
            return groupedNumbers;
        }
        private Dictionary<short, short> GetDigitsInSquare(SudokuSquare square)
        {
            Dictionary<short, short> groupedNumbers = CreateGroupedNumbersDictionary();
            Point squareCoordsMultiplier = map[square];
            short multiplierX = squareCoordsMultiplier.x;
            short multiplierY = squareCoordsMultiplier.y;
            short squareSideLength = 3;
            int limitX = squareSideLength * (multiplierX + 1);
            int limitY = squareSideLength * (multiplierY + 1);
            for (int i = squareSideLength + squareSideLength * multiplierX; i < limitX; i++)
            {
                for (int j = squareSideLength + squareSideLength * multiplierY; j < limitY; j++)
                {
                    short value = sudokuGrid[i, j].value;
                    if (value != emptyCellValue) groupedNumbers[value]++;
                }
            }
            return groupedNumbers;
        }
        private Dictionary<short, short> CreateGroupedNumbersDictionary()
        {
            return new Dictionary<short, short>()
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
        public delegate void NewIterationCompletedHandler(short[,] sudokuGrid);
        public event NewIterationCompletedHandler? NewIterationCompleted;
        public delegate void SudokuSolvedHandler();
        public event SudokuSolvedHandler? SudokuSolved;
        public delegate void SudokuUnsolvableHandler();
        public event SudokuUnsolvableHandler? SudokuUnsolvable;
        protected virtual void OnNewIterationCompleted()
        {
            NewIterationCompleted?.Invoke(RandomArray());
        }
        protected virtual void OnSudokuSolved()
        {
            SudokuSolved?.Invoke();
        }
        protected virtual void OnSudokuUnsolvable()
        {
            SudokuUnsolvable?.Invoke();
        }
        private short[,] RandomArray()
        {
            short[,] arr = new short[sideLength, sideLength];
            Random random = new Random();
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    arr[i, j] = (short)random.Next(1, 10);
                }
            }
            return arr;
        }
        protected class Cell
        {
            internal short value;
            internal bool isReadOnly;
            internal Point location;
            internal Cell? parent;
            internal Cell(short value, bool isReadOnly, Point location)
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
            internal short x;
            internal short y;
            internal Point(short x, short y)
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
            { SudokuSquare.N,      new Point(0, -1)  },
            { SudokuSquare.NE,     new Point(1, -1)  },
            { SudokuSquare.E,      new Point(1, 0)   },
            { SudokuSquare.SE,     new Point(1, 1)   },
            { SudokuSquare.S,      new Point(0, -1)  },
            { SudokuSquare.SW,     new Point(-1, 1)  },
            { SudokuSquare.W,      new Point(-1, 0)  },
            { SudokuSquare.NW,     new Point(-1, -1) },
            { SudokuSquare.CENTER, new Point(0, 0)   }
        };

    }
}