namespace Worldgen
{
    /// <summary>
    /// Represents a type of tile in a standard tile-set in this game.
    /// </summary>
    public enum TileType
    {
        Ceiling,
        Floor,
        CornerTopLeft,
        CornerTopRight,
        CornerBottomLeft,
        CornerBottomRight,
        EdgeLeft,
        EdgeRight,
        EdgeBottom,
        EdgeTop,
        StubTopLeft,
        StubTopRight,
        StubBottomLeft,
        StubBottomRight
    }

    public static class TileTypeUtils
    {
        /// <summary>
        /// Uses the input matrix to determine which tile to return.
        /// </summary>
        /// <param name="middleLeft">If the tile to the left is a wall.</param>
        /// <param name="topMiddle">If the tile above is a wall.</param>
        /// <param name="middleRight">If the tile to the right is a wall.</param>
        /// <param name="bottomMiddle">If the tile below is a wall.</param>
        /// <param name="bottomLeft">If the bottom-left tile is a wall.</param>
        /// <param name="bottomRight">If the bottom-right tile is a wall.</param>
        /// <param name="topLeft">If the top-left tile is a wall.</param>
        /// <param name="topRight">If the top-right tile is a wall.</param>
        /// <returns>The sprite type to use in the given configuration.</returns>
        public static TileType GetTileType(bool middleLeft, bool topMiddle, bool middleRight, bool bottomMiddle,
            bool bottomLeft, bool bottomRight, bool topLeft, bool topRight)
        {
            // lonesome edges
            if (!middleLeft && !middleRight)
                return TileType.Ceiling;
            if (!topMiddle && !bottomMiddle)
                return TileType.Ceiling;
            
            switch (topMiddle)
            {
                case true when bottomMiddle && middleLeft && !middleRight:
                    return TileType.EdgeLeft;
                case true when bottomMiddle && !middleLeft:
                    return TileType.EdgeRight;
                case true when !bottomMiddle && middleLeft && middleRight:
                    return TileType.EdgeTop;
                case false when middleLeft && middleRight:
                    return TileType.EdgeBottom;
            }

            int coveredCorners = (bottomRight ? 1 : 0) + (bottomLeft ? 1 : 0) + (topLeft ? 1 : 0) + (topRight ? 1 : 0);

            if (coveredCorners == 1)
            {
                if(bottomLeft)
                    return TileType.StubBottomLeft;
                if(bottomRight)
                    return TileType.StubBottomRight;
                if(topRight)
                    return TileType.StubTopRight;
                if(topLeft)
                    return TileType.StubTopLeft;
            }

            if (!middleLeft || !middleRight)
                return TileType.Ceiling;

            if (coveredCorners != 3)
                return TileType.Ceiling;
            if (!bottomRight)
                return TileType.CornerTopLeft;
            if (!bottomLeft)
                return TileType.CornerTopRight;
            if (!topRight)
                return TileType.CornerBottomLeft;
            if (!topLeft)
                return TileType.CornerBottomRight;

            return TileType.Ceiling;
        }
    }
}