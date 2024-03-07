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
        /// Returns TileType.Floor if selfIsFloor. Otherwise, uses the input matrix to determine which
        /// tile to return.
        /// </summary>
        /// <param name="selfIsFloor">If this rile is a floor (returns TileType.Floor)</param>
        /// <param name="left">If the tile to the left is a wall.</param>
        /// <param name="top">If the tile above is a wall.</param>
        /// <param name="right">If the tile to the right is a wall.</param>
        /// <param name="bottom">If the tile below is a wall.</param>
        /// <param name="bottomLeft">If the bottom-left tile is a wall.</param>
        /// <param name="bottomRight">If the bottom-right tile is a wall.</param>
        /// <param name="topLeft">If the top-left tile is a wall.</param>
        /// <param name="topRight">If the top-right tile is a wall.</param>
        /// <returns>The sprite type to use in the given configuration.</returns>
        public static TileType GetTileType(bool selfIsFloor,
            bool left, bool top, bool right, bool bottom,
            bool bottomLeft, bool bottomRight, bool topLeft, bool topRight)
        {
            if (selfIsFloor)
                return TileType.Floor;
            if (top && right && !topRight)
                return TileType.CornerTopRight;
            if (top && left && !topLeft)
                return TileType.CornerTopLeft;
            if (bottom && right && !bottomRight)
                return TileType.CornerBottomRight;
            if (bottom && left && !bottomLeft)
                return TileType.CornerBottomLeft;
            if (!bottom && !left && !bottomLeft)
                return TileType.StubBottomLeft;
            if (!bottom && !right && !bottomRight)
                return TileType.StubBottomRight;
            if (!top && !left && !topLeft)
                return TileType.StubTopLeft;
            if (!top && !right && !topRight)
                return TileType.StubTopRight;
            if (!top)
                return TileType.EdgeTop;
            if (!bottom)
                return TileType.EdgeBottom;
            if (!left)
                return TileType.EdgeLeft;
            if (!right)
                return TileType.EdgeRight;
            return TileType.Ceiling;
        }
    }
}