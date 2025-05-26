using battleship.Class;

namespace battleship
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Grid grid = new Grid();

            //grid.CreateGridDefault();
            //grid.DrawGrid();

            Console.WriteLine("Welcome to Battleship!");
            Console.WriteLine("---------------------");

            var game = new BattleshipLoop();
            game.Play();
        }
    }
}
