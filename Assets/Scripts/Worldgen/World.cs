using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Characters;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Worldgen
{
    [RequireComponent(typeof(Tilemap), typeof(TilemapCollider2D))]
    public class World : MonoBehaviour
    {
        private const int OUTER_PADDING = 3;
        private const float CELL_SIZE = 0.08F;
        
        private int worldWidth;
        private int worldHeight;

        private int tilesWidth;
        private int tilesHeight;
        private bool[,] tiles;

        private readonly Dictionary<TileType, Tile> loadedTiles = new();
        private readonly List<Room> rooms = new();
        private Tilemap tilemap;
        private new TilemapCollider2D collider;
        private WorldgenSettings settings;
        
        public float spawnPointX;
        public float spawnPointY;

        /// <summary>
        /// Finds the furthest room from the middle of the rooms' total area.
        /// </summary>
        private Room FindFurthestRoomFromMiddle(out int thatRoomsIndex)
        {
            thatRoomsIndex = -1;
            Room bestMatch = default;
            float currentDistance = 0F;
            Vector2 middleOfSpace = GetContainingRoom().Middle;

            for (int i = 0; i < this.rooms.Count; i++)
            {
                Room room = this.rooms[i];
                Vector2 position = room.Middle;
                float distance = Vector2.Distance(position, middleOfSpace);

                if (distance < currentDistance)
                    continue;

                thatRoomsIndex = i;
                currentDistance = distance;
                bestMatch = room;
            }

            return bestMatch;
        }
        /// <summary>
        /// Gets the world-space coordinates of the middle point of a room.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <returns>The coordinates of the middle point of the room in world space.</returns>
        private Vector2 GetWorldCoordinateOfRoomMiddle(Room room)
        {
            int roomMiddleX = (room.x * 2 + room.width) / 2;
            int roomMiddleY = (room.y * 2 + room.height) / 2;
            return GetWorldCoordinateOfTile(roomMiddleX, roomMiddleY);
        }
        /// <summary>
        /// Get the coordinates of a tile in world space.
        /// </summary>
        /// <param name="tileX">The x-coordinate of the tile.</param>
        /// <param name="tileY">The y-coordinate of the tile.</param>
        /// <returns>The coordinates of the tile in world space.</returns>
        private Vector2 GetWorldCoordinateOfTile(int tileX, int tileY)
        {
            return new Vector2(
                (OUTER_PADDING + tileX) * CELL_SIZE,
                (this.tilesHeight - (tileY - OUTER_PADDING)) * CELL_SIZE
            );
        }

        private void PopulateRoom(Room room, Character spawner, ref bool freeGun)
        {
            GlobalStuff stuff = GlobalStuff.SINGLETON;

            // do pickups, if any
            while (freeGun || Random.Range(0, 6) == 0)
            {
                freeGun = false;
                Vector2 position = RandomPointInRoom();
                if (Game.RollDropType())
                    spawner.SpawnGunPickup(position, Game.RollGun());
                else
                    spawner.SpawnLuckyObjectPickup(position, Game.RollLuckyObject());
            }
            
            if (!room.shouldSpawnEnemies)
                return;

            // spawn enemies
            Vector2 playerPosition = new Vector2(this.spawnPointX, this.spawnPointY);
            int spawned = 0;
            while (Random.Range(0, 8) == 0)
            {
                Vector2 position = RandomPointInRoom();
                float angle;

                if (Vector2.Distance(playerPosition, position) < 1F)
                {
                    Vector2 direction = position - playerPosition;
                    angle = Vector2.SignedAngle(Vector2.right, direction.normalized);
                }
                else
                    angle = Random.Range(-180F, 180F);

                stuff.SpawnEnemy(position, 10, angle, this.settings.enemySpriteSheet);
                spawned++;

                if (spawned > 2)
                    break;
            }
            
            return;

            Vector2 RandomPointInRoom()
            {
                Vector2 lowerBound = GetWorldCoordinateOfTile(room.Left, room.Top);
                lowerBound.x += CELL_SIZE;
                lowerBound.y += CELL_SIZE;
                
                Vector2 upperBound = GetWorldCoordinateOfTile(room.Right, room.Bottom);
                upperBound.x += CELL_SIZE;
                upperBound.y += CELL_SIZE;
                
                return new Vector2(
                    Random.Range(lowerBound.x, upperBound.x),
                    Random.Range(lowerBound.y, upperBound.y)
                );
            }
        }
        private void PopulateRooms(Character spawner)
        {
            GlobalStuff stuff = GlobalStuff.SINGLETON;
            
            while (stuff.Enemies < this.settings.avgEnemies)
            {
                foreach (Room room in this.rooms)
                {
                    bool freeGun = !room.shouldSpawnEnemies;
                    PopulateRoom(room, spawner, ref freeGun);
                }
            }
        }
        
        private void LoadTilesetData()
        {
            this.loadedTiles.Clear();
            string basePath = "Sprites/Tilesets/" + this.settings.sheet + '/';
            
            foreach(TileType tileType in Enum.GetValues(typeof(TileType)))
            {
                string tilePath = basePath + tileType;
                Tile tile = Resources.Load<Tile>(tilePath);

                if (tile == null)
                    throw new Exception("WARNING! Missing tile in project: " + tilePath);

                this.loadedTiles[tileType] = tile;
            }
        }
        /// <summary>
        /// Returns the bounds of all the rooms in the <see cref="rooms"/> list.
        /// </summary>
        /// <returns></returns>
        private Room GetContainingRoom()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = 0;
            int maxY = 0;

            foreach (Room room in this.rooms)
            {
                if (room.Left < minX)
                    minX = room.Left;
                if (room.Top < minY)
                    minY = room.Top;
                if (room.x + room.width > maxX)
                    maxX = room.x + room.width;
                if (room.y + room.height > maxY)
                    maxY = room.y + room.height;
            }

            int sizeX = maxX - minX;
            int sizeY = maxY - minY;
            return new Room(sizeX, sizeY, minX, minY);
        }
        /// <summary>
        /// Shift all registered rooms by some amount.
        /// </summary>
        /// <param name="x">The amount to shift on the X axis.</param>
        /// <param name="y">The amount to shift on the Y axis.</param>
        private void ShiftAllRooms(int x, int y)
        {
            for (int i = 0; i < this.rooms.Count; i++)
            {
                Room room = this.rooms[i];
                room.x += x;
                room.y += y;
                this.rooms[i] = room;
            }
        }
        /// <summary>
        /// Generates a random room.
        /// </summary>
        /// <returns></returns>
        private Room GenerateRoom(int x, int y)
        {
            int width = Random.Range(this.settings.roomMinSize, this.settings.roomMaxSize + 1);
            int height = Random.Range(this.settings.roomMinSize, this.settings.roomMaxSize + 1);
            return new Room(width, height, x, y);
        }

        /// <summary>
        /// Applies the given room to the <see cref="tiles"/> buffer.
        /// </summary>
        /// <param name="room">The room to apply.</param>
        /// <param name="possibleBorders">The rooms which could possibly border the specified room.</param>
        private void ApplyRoom(Room room, IEnumerable<Room> possibleBorders)
        {
            for (int _x = 0; _x < room.width; _x++)
            {
                for (int _y = 0; _y < room.height; _y++)
                {
                    int x = _x + room.x;
                    int y = _y + room.y;
                    if (x < 0 || x >= this.tilesWidth)
                        continue;
                    if (y < 0 || y >= this.tilesHeight)
                        continue;

                    this.tiles[x, y] = true;
                }
            }
            
            // find any rooms that border this one, and add a hallway between them
            int roomRightRequirement = room.Left - 2;
            int roomLeftRequirement = room.Right + 2;
            int roomTopRequirement = room.Bottom + 2;
            int roomBottomRequirement = room.Top - 2;

            foreach (Room possibleBorderRoom in possibleBorders)
            {
                bool overlapsHorizontally =
                    room.Left <= possibleBorderRoom.Right && room.Right >= possibleBorderRoom.Left;
                bool overlapsVertically =
                    room.Top <= possibleBorderRoom.Bottom && room.Bottom >= possibleBorderRoom.Top;
                int leewayRequired = this.settings.hallWidth;
                
                if (possibleBorderRoom.Right == roomRightRequirement && overlapsVertically)
                {
                    // they border vertical sides, get shared range
                    int sharedMax = Mathf.Min(possibleBorderRoom.Bottom, room.Bottom);
                    int sharedMin = Mathf.Max(possibleBorderRoom.Top, room.Top);
                    int hallwayY = Random.Range(sharedMin, sharedMax - leewayRequired + 1);
                    for(int i = 0; i < leewayRequired; i++)
                        this.tiles[room.Left - 1, i + hallwayY] = true;
                }
                if (possibleBorderRoom.Left == roomLeftRequirement && overlapsVertically)
                {
                    // they border vertical sides, get shared range
                    int sharedMax = Mathf.Min(possibleBorderRoom.Bottom, room.Bottom);
                    int sharedMin = Mathf.Max(possibleBorderRoom.Top, room.Top);
                    int hallwayY = Random.Range(sharedMin, sharedMax - leewayRequired + 1);
                    for(int i = 0; i < leewayRequired; i++)
                        this.tiles[room.Right + 1, i + hallwayY] = true;
                }
                if (possibleBorderRoom.Top == roomTopRequirement && overlapsHorizontally)
                {
                    // they border horizontal sides, get shared range
                    int sharedMax = Mathf.Min(possibleBorderRoom.Right, room.Right);
                    int sharedMin = Mathf.Max(possibleBorderRoom.Left, room.Left);
                    int hallwayX = Random.Range(sharedMin, sharedMax - leewayRequired + 1);
                    for(int i = 0; i < leewayRequired; i++)
                        this.tiles[i + hallwayX, room.Bottom + 1] = true;
                }
                // ReSharper disable once InvertIf
                if (possibleBorderRoom.Bottom == roomBottomRequirement && overlapsHorizontally)
                {
                    // they border horizontal sides, get shared range
                    int sharedMax = Mathf.Min(possibleBorderRoom.Right, room.Right);
                    int sharedMin = Mathf.Max(possibleBorderRoom.Left, room.Left);
                    int hallwayX = Random.Range(sharedMin, sharedMax - leewayRequired + 1);
                    for(int i = 0; i < leewayRequired; i++)
                        this.tiles[i + hallwayX, room.Top - 1] = true;
                }
            }
        }
        private void ApplyTilesToTilemap()
        {
            // fill the borders
            Tile ceilingTile = this.loadedTiles[TileType.Ceiling];
            this.tilemap.size = new Vector3Int(
                this.tilesWidth + OUTER_PADDING * 2,
                this.tilesHeight + OUTER_PADDING * 2
            );
            
            this.tilemap.BoxFill(Vector3Int.zero, ceilingTile,
                0,
                0,
                this.tilesWidth + OUTER_PADDING * 2,
                this.tilesHeight + OUTER_PADDING * 2
            );

            // regular apply
            Vector3Int placer = new Vector3Int(0, 0, 0);
            for (int y = 0; y < this.tilesHeight; y++)
            {
                placer.y = OUTER_PADDING + this.tilesHeight - y;
                for (int x = 0; x < this.tilesWidth; x++)
                {
                    placer.x = x + OUTER_PADDING;
                    TileType type;
                    bool middleMiddle = this.tiles[x, y];
                    
                    if (!middleMiddle)
                    {
                        bool topLeft = TryIsWall(x - 1, y - 1);
                        bool topMiddle = TryIsWall(x, y - 1);
                        bool topRight = TryIsWall(x + 1, y - 1);
                        bool middleLeft = TryIsWall(x - 1, y);
                        bool middleRight = TryIsWall(x + 1, y);
                        bool bottomLeft = TryIsWall(x - 1, y + 1);
                        bool bottomMiddle = TryIsWall(x, y + 1);
                        bool bottomRight = TryIsWall(x + 1, y + 1);
                        type = TileTypeUtils.GetTileType(
                            middleLeft, topMiddle, middleRight, bottomMiddle,
                            bottomLeft, bottomRight, topLeft, topRight);
                    }
                    else
                        type = TileType.Floor;

                    Tile tile = this.loadedTiles[type];
                    this.tilemap.SetTile(placer, tile);
                }
            }

            return;
            
            // Returns if the tile at the given coordinate is a wall.
            bool TryIsWall(int x, int y)
            {
                if (x >= this.tilesWidth || x < 0)
                    return true;
                if (y >= this.tilesHeight || y < 0)
                    return true;
                return !this.tiles[x, y];
            }
        }
        private Room GenerateBranchRoom(Room roomBase, out RoomSide side, out bool deadlocked, RoomSide? offLimits = null)
        {
            const int DEADLOCK_LIMIT = 50;
            int attempts = 0;
            
            while (true)
            {
                side = (RoomSide) Random.Range(0, 4);
                
                // there's an off limits direction
                if (offLimits.HasValue)
                {
                    while (side == offLimits.Value)
                        side = (RoomSide) Random.Range(0, 4);
                }

                int locationOnSideAxis = roomBase.GetPropagationCoordinate(side);

                int width = Random.Range(this.settings.roomMinSize, this.settings.roomMaxSize + 1);
                int height = Random.Range(this.settings.roomMinSize, this.settings.roomMaxSize + 1);

                bool horizontal = side is RoomSide.Top or RoomSide.Bottom;
                int leeway = this.settings.hallWidth;
                int slide = horizontal ?
                    Random.Range(-width + leeway, roomBase.width - leeway) :
                    Random.Range(-height + leeway, roomBase.height - leeway);

                Room toReturn = default;
                if (side == RoomSide.Left)
                    toReturn = new Room(width, height, locationOnSideAxis - width + 1, roomBase.y + slide);
                if (side == RoomSide.Right)
                    toReturn = new Room(width, height, locationOnSideAxis, roomBase.y + slide);
                if (side == RoomSide.Top)
                    toReturn = new Room(width, height, roomBase.x + slide, locationOnSideAxis - height + 1);
                if (side == RoomSide.Bottom)
                    toReturn = new Room(width, height, roomBase.x + slide, locationOnSideAxis);

                // check if the room intersects with any existing room
                if (this.rooms.Any(r => r.Intersects(toReturn)))
                {
                    attempts++;
                    if (attempts > DEADLOCK_LIMIT)
                    {
                        deadlocked = true;
                        return default;
                    }
                    continue; // don't generate it, keep trying other variants
                }

                deadlocked = false;
                return toReturn;
            }
        }
        /// <summary>
        /// Regenerate the map with the current <see cref="settings"/>.
        /// </summary>
        internal void Regenerate()
        {
            this.rooms.Clear();
            this.tilemap.ClearAllTiles();

            Room baseRoom = GenerateRoom(0, 0);
            this.rooms.Add(baseRoom);

            Stack<Room> currentRoom = new();
            RoomSide? disallowedSide = null;
            currentRoom.Push(baseRoom);
            for (int i = 0; i < this.settings.roomCount; i++)
            {
                Room attachedRoom = GenerateBranchRoom(currentRoom.Peek(), out RoomSide side, out bool deadlocked, disallowedSide);

                if (deadlocked)
                {
                    if (currentRoom.Count > 1)
                    {
                        // try previous room and make branch
                        i--;
                        currentRoom.Pop();
                        continue;
                    }
                    // give up
                    break;
                }
                
                this.rooms.Add(attachedRoom);
                currentRoom.Push(attachedRoom);
                
                // generating on the opposite side is now off limits
                disallowedSide = side switch {
                    RoomSide.Left => RoomSide.Right,
                    RoomSide.Right => RoomSide.Left,
                    RoomSide.Top => RoomSide.Bottom,
                    RoomSide.Bottom => RoomSide.Top,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            Room containing = GetContainingRoom();
            ShiftAllRooms(-containing.x, -containing.y);

            // initialization
            this.tiles = new bool[containing.width, containing.height];
            this.tilesWidth = containing.width;
            this.tilesHeight = containing.height;

            for (int y = 0; y < containing.height; y++)
                for (int x = 0; x < containing.width; x++)
                    this.tiles[x, y] = false;

            // imprints the rooms into the tiles array, and creates hallways
            var remainingRoomsToApply = new Stack<Room>(this.rooms);
            while (remainingRoomsToApply.Count > 0)
            {
                Room roomToApply = remainingRoomsToApply.Pop();
                ApplyRoom(roomToApply, remainingRoomsToApply);
            }
            
            // draws the tiles array to the unity tilemap
            ApplyTilesToTilemap();
            this.collider.ProcessTilemapChanges();
            this.transform.position = Vector3.zero;
            
            // find spawn point
            Room furthestRoom = FindFurthestRoomFromMiddle(out int indexOfFurthestRoom);
            furthestRoom.shouldSpawnEnemies = false;
            this.rooms[indexOfFurthestRoom] = furthestRoom;
            Vector2 furthestRoomStart = GetWorldCoordinateOfRoomMiddle(furthestRoom);
            this.spawnPointX = furthestRoomStart.x;
            this.spawnPointY = furthestRoomStart.y;
            
            // set the player to the spawn position.
            Player player = FindObjectOfType<Player>();
            player.transform.position = furthestRoomStart;
            
            // clear all stuff in the world
            GlobalStuff.SINGLETON.ClearWorld();
            
            // populate rooms with LOOT & ENEMIES
            PopulateRooms(player);
        }

        public void ResetAndRegenerate()
        {
            if (this.settings.sheet != Game.worldGenerationSettings.sheet)
            {
                // reload world tileset
                this.settings = Game.worldGenerationSettings;
                LoadTilesetData();
            }
            else
                this.settings = Game.worldGenerationSettings;
            
            Regenerate();
        }
        private void Start()
        {
            this.tilemap = GetComponent<Tilemap>();
            this.collider = GetComponent<TilemapCollider2D>();

            this.settings = Game.worldGenerationSettings;
            LoadTilesetData();
            
            StartCoroutine(StartLater());
        }
        private IEnumerator StartLater()
        {
            yield return new WaitForFixedUpdate();
            Regenerate();
        }
    }

    public enum WorldSpriteSheet
    {
        kitchen,
        cave,
        white_house
    }
    public struct WorldgenSettings
    {
        /// <summary>
        /// The sprite sheet to use.
        /// </summary>
        public WorldSpriteSheet sheet;
        /// <summary>
        /// The minimum size (per axis) of a room.
        /// </summary>
        public int roomMinSize;
        /// <summary>
        /// The maximum size (per axis) of a room.
        /// </summary>
        public int roomMaxSize;
        /// <summary>
        /// The number of rooms to generate.
        /// </summary>
        public int roomCount;
        /// <summary>
        /// The width of halls in between rooms.
        /// </summary>
        public int hallWidth;
        
        /// <summary>
        /// The minimum number of enemies allowed to spawn.
        /// </summary>
        public int avgEnemies;
        /// <summary>
        /// The sprite sheet to use for the enemies in this world.
        /// </summary>
        public Enemy.SpriteSheet enemySpriteSheet;

        public WorldgenSettings(WorldSpriteSheet sheet, int roomMinSize, int roomMaxSize, int roomCount, int hallWidth, Enemy.SpriteSheet enemySpriteSheet, int avgEnemies)
        {
            Assert.IsFalse(hallWidth > roomMinSize, "Hall width cannot be larger than minimum room size.");
            Assert.IsFalse(roomMinSize > roomMaxSize, "Minimum room size cannot be larger than the maxiumu room size.");

            this.sheet = sheet;
            this.roomMinSize = roomMinSize;
            this.roomMaxSize = roomMaxSize;
            this.roomCount = roomCount;
            this.hallWidth = hallWidth;
            this.enemySpriteSheet = enemySpriteSheet;
            this.avgEnemies = avgEnemies;
        }
    }
    public enum RoomSide
    {
        Left, Right, Top, Bottom
    }
    public struct Room
    {
        public bool shouldSpawnEnemies;
        public int width;
        public int height;
        public int x;
        public int y;

        public Room(int width, int height, int x, int y)
        {
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
            this.shouldSpawnEnemies = true;
        }

        public int Left => this.x;
        public int Right => this.x + this.width - 1;
        public int Top => this.y;
        public int Bottom => this.y + this.height - 1;
        public Vector2 Middle => new(this.x + this.width / 2F, this.y + this.height / 2F);

        /// <summary>
        /// Returns the coordinate (x or y depending on input side) of the given side of the room.
        /// </summary>
        /// <returns></returns>
        [Pure]
        public int GetPropagationCoordinate(RoomSide side)
        {
            return side switch
            {
                RoomSide.Left => this.Left - 2,
                RoomSide.Right => this.Right + 2,
                RoomSide.Top => this.Top - 2,
                RoomSide.Bottom => this.Bottom + 2,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }
        public bool Intersects(Room other)
        {
            return this.Left - 1 < other.Right + 1 && this.Right + 1 > other.Left - 1 && 
                   this.Top - 1 < other.Bottom + 1 && this.Bottom + 1 > other.Top - 1;
        }
        public override string ToString()
        {
            return $"{nameof(this.width)}: {this.width}, {nameof(this.height)}: {this.height}, {nameof(this.x)}: {this.x}, {nameof(this.y)}: {this.y}, {nameof(this.Left)}: {this.Left}, {nameof(this.Right)}: {this.Right}, {nameof(this.Top)}: {this.Top}, {nameof(this.Bottom)}: {this.Bottom}";
        }
    }
}