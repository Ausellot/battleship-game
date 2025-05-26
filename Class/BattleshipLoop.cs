using System;
using System.Collections.Generic;
using System.Linq;

namespace battleship.Class
{
    public class BattleshipLoop
    {
        private const int boardSize = 10;
        private readonly Random random = new Random();
        private List<Ship> ships = new List<Ship>();

        private char[,] playerBoard; // playerBoard: Tracks player's ships
        private char[,] playerShotBoard; // playerShotBoard: Tracks player's shots against computer
        private char[,] computerBoard; // computerBoard: Tracks computer's ships
        private char[,] computerShotBoard; // computerShotBoard: Tracks computer's shots against player

        // Main game loop
        public void Play()
        {
            InitialiseGrids(); // Set up all game boards

            PlaceShipsRandomly(playerBoard); // Place player's ships

            PlaceShipsRandomly(computerBoard); // Place computer's ships

            // Main game loop - alternates between player and computer turns
            while (true)
            {
                // Player's turn
                Console.WriteLine("\nYour turn:");
                DisplayBoard(playerShotBoard, "Target Board");
                PlayerTurn();

                // Check if player has won
                if (CheckWin(computerBoard))
                {
                    Console.WriteLine("Congratulations! You sunk all computer's ships!");
                    break;
                }

                // Computer's turn
                Console.WriteLine("\nComputer's turn:");
                ComputerTurn();

                // Check if computer has won
                if (CheckWin(playerBoard))
                {
                    Console.WriteLine("Game over! The computer sunk all your ships!");
                    break;
                }

                // Show player's board with hits/misses
                DisplayBoard(playerBoard, "Your Board (Hits/Misses Only)");
            }

            // Game over - show final boards
            Console.WriteLine("\nFinal boards:");
            DisplayBoard(playerBoard, "Your Board");
            DisplayBoard(computerBoard, "Computer's Board (Revealed)");
        }

        // Initializes all game grids and ships
        public void InitialiseGrids()
        {
            // Create all boards
            playerBoard = new char[boardSize, boardSize];
            playerShotBoard = new char[boardSize, boardSize];
            computerBoard = new char[boardSize, boardSize];
            computerShotBoard = new char[boardSize, boardSize];

            ships.Clear();
            int shipId = 0;

            // Add ships to the game:
            // 1 battleship (size 5)
            ships.Add(new Ship { type = ShipType.Battleship, size = 5, identifier = (char)('B' + shipId++) });

            // 2 destroyers (size 4 each)
            ships.Add(new Ship { type = ShipType.Destroyer, size = 4, identifier = (char)('D' + shipId++) });
            ships.Add(new Ship { type = ShipType.Destroyer, size = 4, identifier = (char)('D' + shipId++) });

            // Initialize all board cells to water ('~')
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    playerBoard[y, x] = '~';
                    playerShotBoard[y, x] = '~';
                    computerBoard[y, x] = '~';
                    computerShotBoard[y, x] = '~';
                }
            }
        }

        // Randomly places all ships on the specified board
        private void PlaceShipsRandomly(char[,] board)
        {
            foreach (var ship in ships)
            {
                bool placed = false;

                // Keep trying random positions until ship is placed
                while (!placed)
                {
                    int row = random.Next(0, boardSize);
                    int col = random.Next(0, boardSize);
                    bool isHorizontal = random.Next(0, 2) == 0; // Random orientation

                    if (CanPlaceShip(board, row, col, ship.size, isHorizontal))
                    {
                        PlaceShip(board, row, col, ship, isHorizontal);
                        placed = true;
                    }
                }
            }
        }

        // Checks if a ship can be placed at the specified location
        private bool CanPlaceShip(char[,] board, int row, int col, int length, bool isHorizontal)
        {
            if (isHorizontal)
            {
                // Check if ship would go off board
                if (col + length > boardSize) return false;

                // Check if all required cells are empty
                for (int c = col; c < col + length; c++)
                {
                    if (board[row, c] != '~') return false;
                }
            }
            else
            {
                // Vertical placement check
                if (row + length > boardSize) return false;

                for (int r = row; r < row + length; r++)
                {
                    if (board[r, col] != '~') return false;
                }
            }

            return true;
        }

        // Places a ship on the board at the specified location
        private void PlaceShip(char[,] board, int row, int col, Ship ship, bool isHorizontal)
        {
            ship.positions.Clear(); // Clear any existing positions

            if (isHorizontal)
            {
                // Place ship horizontally
                for (int c = col; c < col + ship.size; c++)
                {
                    board[row, c] = ship.identifier;
                    ship.positions.Add((row, c)); // Record ship position
                }
            }
            else
            {
                // Place ship vertically
                for (int r = row; r < row + ship.size; r++)
                {
                    board[r, col] = ship.identifier;
                    ship.positions.Add((r, col)); // Record ship position
                }
            }
        }

        // Handles player's turn
        private void PlayerTurn()
        {
            while (true)
            {
                Console.Write("Enter target coordinate (e.g., A5): ");
                var (row, col) = ParseCoordinate(Console.ReadLine());

                // Validate input
                if (row == -1 || col == -1)
                {
                    Console.WriteLine("Invalid coordinate. Format should be like A5 (letter A-J followed by number 0-9).");
                    continue;
                }

                // Check if already targeted this location
                if (playerShotBoard[row, col] != '~')
                {
                    Console.WriteLine("You've already targeted this location. Try again.");
                    continue;
                }

                // Process the shot
                if (computerBoard[row, col] == '~') // Miss
                {
                    Console.WriteLine("Miss!");
                    playerShotBoard[row, col] = 'M'; // Mark as miss
                    computerBoard[row, col] = 'M';
                }
                else if (computerBoard[row, col] == 'M' || computerBoard[row, col] == 'H') // Already shot here
                {
                    Console.WriteLine("You've already targeted this location. Try again.");
                    continue;
                }
                else // Hit
                {
                    char shipChar = computerBoard[row, col];
                    playerShotBoard[row, col] = 'H'; // Mark as hit
                    computerBoard[row, col] = 'H';

                    if (IsShipSunk(computerBoard, shipChar)) // Check if ship is sunk
                    {
                        Console.WriteLine($"Hit! You sunk the computer's {GetShipName(shipChar)}!");
                        MarkSunkShip(playerShotBoard, computerBoard, shipChar);
                    }
                    else
                    {
                        Console.WriteLine("Hit!");
                    }
                }

                break;
            }
        }

        // Parses coordinate input (e.g., "A5" to (0,4))
        private (int row, int col) ParseCoordinate(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
                return (-1, -1);

            char rowChar = char.ToUpper(input[0]);
            if (rowChar < 'A' || rowChar > 'J') // Validate row
                return (-1, -1);

            if (!int.TryParse(input.Substring(1), out int col) || col < 1 || col > boardSize) // Validate column
                return (-1, -1);

            int row = rowChar - 'A';
            return (row, col - 1);  // Convert to 0-based index
        }

        // Formats coordinate for display (e.g., (0,4) to "A5")
        private string FormatCoordinate(int row, int col)
        {
            char rowChar = (char)('A' + row);
            return $"{rowChar}{col + 1}";  // Convert back to 1-based for display
        }

        // Handles computer's turn
        private void ComputerTurn()
        {
            int row, col;
            var potentialTargets = FindPotentialTargets(); // Look for adjacent cells to existing hits

            // If there are potential targets near hits, choose one randomly
            if (potentialTargets.Count > 0)
            {
                var target = potentialTargets[random.Next(0, potentialTargets.Count)];
                row = target.Item1;
                col = target.Item2;
            }
            else
            {
                // Otherwise choose a completely random cell
                do
                {
                    row = random.Next(0, boardSize);
                    col = random.Next(0, boardSize);
                } while (computerShotBoard[row, col] != '~'); // Ensure not already shot
            }

            Console.WriteLine($"Computer targets: {FormatCoordinate(row, col)}");

            // Process the shot
            if (playerBoard[row, col] == '~') // Miss
            {
                Console.WriteLine("Computer missed!");
                computerShotBoard[row, col] = 'M';
                playerBoard[row, col] = 'M';
            }
            else // Hit
            {
                char shipChar = playerBoard[row, col];
                computerShotBoard[row, col] = 'H';
                playerBoard[row, col] = 'H';

                if (IsShipSunk(playerBoard, shipChar)) // Check if ship is sunk
                {
                    Console.WriteLine($"Computer hit your {GetShipName(shipChar)} and sunk it!");
                    MarkSunkShip(computerShotBoard, playerBoard, shipChar);
                }
                else
                {
                    Console.WriteLine("Computer hit your ship!");
                }
            }
        }

        // Finds potential targets adjacent to existing hits (for smarter computer play)
        private List<(int, int)> FindPotentialTargets()
        {
            var targets = new List<(int, int)>();

            // Scan board for hits
            for (int r = 0; r < boardSize; r++)
            {
                for (int c = 0; c < boardSize; c++)
                {
                    if (computerShotBoard[r, c] == 'H')
                    {
                        // Add adjacent cells that haven't been shot at yet
                        if (r > 0 && computerShotBoard[r - 1, c] == '~') targets.Add((r - 1, c));
                        if (r < boardSize - 1 && computerShotBoard[r + 1, c] == '~') targets.Add((r + 1, c));
                        if (c > 0 && computerShotBoard[r, c - 1] == '~') targets.Add((r, c - 1));
                        if (c < boardSize - 1 && computerShotBoard[r, c + 1] == '~') targets.Add((r, c + 1));
                    }
                }
            }

            return targets.Distinct().ToList(); // Remove duplicates
        }

        // Checks if a ship is completely sunk
        private bool IsShipSunk(char[,] board, char shipIdentifier)
        {
            // Find the ship with this identifier
            var ship = ships.FirstOrDefault(s => s.identifier == shipIdentifier);
            if (ship == null) return false;

            // Check all positions of this ship
            foreach (var (row, col) in ship.positions)
            {
                // If any position still shows the ship identifier (not hit), it's not sunk
                if (board[row, col] == shipIdentifier)
                {
                    return false;
                }
            }

            return true;
        }

        // Marks all positions of a sunk ship with 'S'
        private void MarkSunkShip(char[,] targetBoard, char[,] sourceBoard, char shipChar)
        {
            for (int r = 0; r < boardSize; r++)
            {
                for (int c = 0; c < boardSize; c++)
                {
                    if (sourceBoard[r, c] == shipChar || sourceBoard[r, c] == 'H')
                    {
                        targetBoard[r, c] = 'S';
                        sourceBoard[r, c] = 'S';
                    }
                }
            }
        }

        // Checks if all ships on a board are sunk (game win condition)
        private bool CheckWin(char[,] board)
        {
            for (int r = 0; r < boardSize; r++)
            {
                for (int c = 0; c < boardSize; c++)
                {
                    char cell = board[r, c];
                    // If any cell contains a ship identifier that's not marked as hit or sunk
                    if (cell != '~' && cell != 'M' && cell != 'H' && cell != 'S')
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // Gets the name of a ship from its identifier character
        private string GetShipName(char shipChar)
        {
            // Get the base type (without the unique identifier)
            char baseType = shipChar;
            if (shipChar >= 'D' && shipChar <= 'D' + 9) baseType = 'D';  // For destroyers
            if (shipChar >= 'B' && shipChar <= 'B' + 9) baseType = 'B';  // For battleships

            return ((ShipType)baseType).ToString();
        }

        // Displays a game board with proper formatting
        private void DisplayBoard(char[,] board, string title)
        {
            Console.WriteLine($"\n{title}:");
            Console.Write("  ");

            // Display column headers (1-10)
            for (int c = 1; c <= boardSize; c++)
            {
                Console.Write($"{c} ");
            }

            Console.WriteLine();

            // Display each row
            for (int r = 0; r < boardSize; r++)
            {
                Console.Write($"{(char)('A' + r)} "); // Row label (A-J)

                for (int c = 0; c < boardSize; c++)
                {
                    char displayChar = board[r, c];

                    // Hide ship positions that haven't been hit yet
                    if (board == playerBoard && title.Contains("Board") && !title.Contains("Revealed") ||
                        board == computerBoard && title.Contains("Target"))
                    {
                        if (displayChar != 'M' && displayChar != 'H' && displayChar != 'S')
                        {
                            displayChar = '~';
                        }
                    }

                    // Set appropriate color for the cell
                    Console.ForegroundColor = GetColorForCell(displayChar);
                    Console.Write($"{displayChar} ");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }
        }

        // Returns the appropriate console color for a cell
        private ConsoleColor GetColorForCell(char cell)
        {
            return cell switch
            {
                '~' => ConsoleColor.Blue,    // Water
                'M' => ConsoleColor.White,   // Miss
                'H' => ConsoleColor.Red,     // Hit
                'S' => ConsoleColor.DarkRed, // Sunk
                _ => ConsoleColor.Green      // Ships (only visible when revealed)
            };
        }
    }
}