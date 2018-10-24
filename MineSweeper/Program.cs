using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            { 

                int size = GetSize();
                int bombCount = GetBombCount();

                Board playBoard = new Board(size, bombCount);

                while (!playBoard.GameOver())
                {
                    playBoard.Render();

                    playBoard.HandleInput(Console.ReadLine());
                }

                playBoard.Render();

                Console.WriteLine("You Lose! Try again!");
            }
        }


        // TODO get bomb count and size from user
        private static int GetBombCount()
        {
            return 10;
        }

        private static int GetSize()
        {
            return 10;
        }
    }

    /// <summary>
    /// Board to keep track of the state of the game. Can be recreated.
    /// </summary>
    internal class Board
    {
        private readonly int _size;
        private readonly int _bombCount;
        private int[,] _boardMap;
        private bool[,] _selectedMap;
        private bool _gameOver;

        // Construct the board
        public Board(int size, int bombCount)
        {
            _size = size;
            _bombCount = bombCount;

            Initialize();
        }

        #region Board Initialization
        public void Initialize()
        {
            // Create the board Map and Populate the Bombs
            _boardMap = new int[_size, _size];
            _selectedMap = new bool[_size, _size];
            PopulateBombs();
        }

        private void PopulateBombs()
        {
            // Initialize rand with millisecond seed
            Random rand = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < _bombCount; i++)
            {
                int yPos;
                int xPos;

                // Get two random numbers and check if there is already a bomb
                do
                {
                    xPos = rand.Next(0, _size - 1);
                    yPos = rand.Next(0, _size - 1);
                } while (_boardMap[xPos, yPos] == -1);

                // Assign the bomb if there isn't one.
                _boardMap[xPos, yPos] = -1;

                ResolveAdjacent(xPos, yPos);
            }
        }

        /// <summary>
        /// Find and populate the numbers of adjacent squares
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        private void ResolveAdjacent(int xPos, int yPos)
        {
            // Run through adjacent indexes.
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Check if the spot is occupied by a bomb, if not, add to the space count
                    if (IndexInRange(xPos + dx, yPos + dy, _boardMap))
                    {
                        if (_boardMap[xPos + dx, yPos + dy] != -1)
                        {
                            _boardMap[xPos + dx, yPos + dy] += 1;
                        }
                    }
                }
            }
            
        }

        #endregion

        #region Events

        /// <summary>
        /// Handle the input from the main code loop
        /// </summary>
        /// <param name="readLine"></param>
        public void HandleInput(string readLine)
        {
            string[] strings = readLine.Split(',');

            // Check if X is an integer
            if (!int.TryParse(strings[0], out int xIndex))
            {
                Console.WriteLine("X Index not an integer");
                return;
            }

            // Check if Y is an integer
            if (!int.TryParse(strings[1], out int yIndex))
            {
                Console.WriteLine("Y Index not an integer");
                return;
            }

            // Keep the selection in bounds
            if (xIndex >= _size)
            {
                Console.WriteLine("X index out of range");
                return;
            }

            // Keep the selection in bounds
            if (yIndex >= _size)
            {
                Console.WriteLine("Y index out of range");
                return;
            }

            // Check the selection and make the board reveal
            CheckSelection(xIndex, yIndex);


        }

        /// <summary>
        /// Renders the current board
        /// </summary>
        public void Render()
        {
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    // Print 1 space if it's a bomb, one if not.
                    if (_selectedMap[j, i] == true)
                    {
                        Console.Write((_boardMap[j, i] == -1) ? "  " : "   ");
                        Console.Write(_boardMap[j, i]);
                    }
                    else
                    {
                        Console.Write("   ");
                        Console.Write("*");
                    }

                }

                Console.WriteLine();
            }
        }


        #endregion

        #region BoardControl

        private void CheckSelection(int xIndex, int yIndex)
        {
            _selectedMap[xIndex, yIndex] = true;

            // Clear the visited nodes list and start recursive function
            _visitedNodes = new bool[_size,_size];
            if (_boardMap[xIndex, yIndex] == 0) RevealAdjacent(xIndex, yIndex);

            if (_boardMap[xIndex, yIndex] == -1) RevealBoard();
            
        }

        private bool[,] _visitedNodes;
        /// <summary>
        /// Recursive function to reveal adjacent squares if they are all 0.
        /// We also need an additional visited array to deal with BFS
        /// </summary>
        /// <param name="xIndex"></param>
        /// <param name="yIndex"></param>
        private void RevealAdjacent(int xIndex, int yIndex)
        {
            _visitedNodes[xIndex, yIndex] = true;
            // Run through adjacent indexes.
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Check if the index is in range
                    if (IndexInRange(xIndex + dx, yIndex + dy, _boardMap))
                    {
                        // Reveal it
                        _selectedMap[xIndex + dx, yIndex + dy] = true;

                        // If it's 0, then we should check if we've already visited,
                        // if not then run the recursive function
                        if (_boardMap[xIndex + dx, yIndex + dy] == 0 &&
                            !_visitedNodes[xIndex + dx, yIndex + dy])
                        {
                            RevealAdjacent(xIndex + dx, yIndex + dy);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reveal all the spaces to show.
        /// </summary>
        private void RevealBoard()
        {
            //populating 2D Array
            for (int m = 0; m < _size; m++)
            {
                for (int n = 0; n < _size; n++)
                {
                    _selectedMap[m, n] = true;
                }
            }

            _gameOver = true;
        }

        public bool GameOver()
        {
            return _gameOver;
        }

        #endregion

        #region Utility

        /// <summary>
        ///  Finds if the given indexes are in range
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="array"></param>
        /// <returns> bool ifInRange</returns>
        private bool IndexInRange(int x, int y, int[,] array)
        {
            if (x > 9 || x < 0 || y > 9 || y < 0)
            {
                return false;
            }

            return true;
        }

        #endregion

        
    }

}
