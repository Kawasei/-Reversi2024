using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reversi2024
{
    public static class Utility
    {
        public static Vector2Int ConvertPosition(ulong data)
        {
            for (int y = 0; y < 8; y++)
            {
                if ((data & 0b11111111) != 0)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if ((data & (ulong)(1 << x)) != 0)
                        {
                            return new Vector2Int(x, y);
                        }
                    }
                }

                data >>= 8;
            }
            
            return new Vector2Int(-1, -1);
        }
        
        public static ulong ConvertPosition(Vector2Int pos)
        {
            return ConvertPosition(pos.x, pos.y);
        }

        public static ulong ConvertPosition(int x, int y)
        {
            return (ulong)1 << (x + y * 8);
        }
    }
}
