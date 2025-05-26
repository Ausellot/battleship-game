using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battleship.Class
{
    public class Ship
    {
        public ShipType type {  get; set; }
        public int size { get; set; }
        public char identifier { get; set; }
        public List<(int row, int col)> positions { get; set; } = new List<(int row, int col)>();
    }
    public enum ShipType
    {
        Battleship = 'B',
        Destroyer = 'D'
    }
}
