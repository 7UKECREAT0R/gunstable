using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Worldgen
{
    public class World : MonoBehaviour
    {
        private int worldWidth;
        private int worldHeight;

        private int tilesWidth;
        private int tilesHeight;
        private bool[,] tiles;
        
        private List<Room> rooms = new();

        public WorldgenSettings settings;
        public Tilemap tilemap;
        
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
                if (room.Right > maxX)
                    maxX = room.Right;
                if (room.Bottom > maxY)
                    maxY = room.Bottom;
            }
            
            return new Room(maxX - minX, maxY - minY, minX, minY);
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
        /// <param name="room"></param>
        private void ApplyRoom(Room room)
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

            foreach (Room possibleBorderRoom in this.rooms)
            {
                if (possibleBorderRoom.Right == roomRightRequirement)
                {
                    // they border vertical sides, get shared range
                    int sharedMax = Mathf.Min(possibleBorderRoom.Bottom, room.Bottom);
                    int sharedMin = Mathf.Max(possibleBorderRoom.Top, room.Top);
                    int hallwayY = Random.Range(sharedMin, sharedMax + 1);
                    this.tiles[room.Left - 1, hallwayY] = true;
                }
                if (possibleBorderRoom.Left == roomLeftRequirement)
                {
                    // they border vertical sides, get shared range
                    int sharedMax = Mathf.Min(possibleBorderRoom.Bottom, room.Bottom);
                    int sharedMin = Mathf.Max(possibleBorderRoom.Top, room.Top);
                    int hallwayY = Random.Range(sharedMin, sharedMax + 1);
                    this.tiles[room.Right + 1, hallwayY] = true;
                }
                if (possibleBorderRoom.Top == roomTopRequirement)
                {
                    // they border horizontal sides, get shared range
                    int sharedMax = Mathf.Min(possibleBorderRoom.Right, room.Right);
                    int sharedMin = Mathf.Max(possibleBorderRoom.Left, room.Left);
                    int hallwayX = Random.Range(sharedMin, sharedMax + 1);
                    this.tiles[hallwayX, room.Bottom + 1] = true;
                }
                if (possibleBorderRoom.Bottom == roomBottomRequirement)
                {
                    // they border horizontal sides, get shared range
                    int sharedMax = Mathf.Min(possibleBorderRoom.Right, room.Right);
                    int sharedMin = Mathf.Max(possibleBorderRoom.Left, room.Left);
                    int hallwayX = Random.Range(sharedMin, sharedMax + 1);
                    this.tiles[hallwayX, room.Top - 1] = true;
                }
            }
        }
        private Room GenerateBranchRoom(Room roomBase)
        {
            RoomSide side = (RoomSide) Random.Range(0, 4);
            Vector2Int point = roomBase.GetRandomPointOnWall(side, out Vector2Int forward);
            point += forward;

            int width = Random.Range(this.settings.roomMinSize, this.settings.roomMaxSize + 1);
            int height = Random.Range(this.settings.roomMinSize, this.settings.roomMaxSize + 1);

            bool horizontal = side is RoomSide.Top or RoomSide.Bottom;
            int slide = horizontal ?
                Random.Range(-width + 1, roomBase.width + width) :
                Random.Range(-height + 1, roomBase.height + height);

            return side switch
            {
                RoomSide.Left => new Room(width, height, point.x, point.y + slide),
                RoomSide.Right => new Room(width, height, point.x, point.y + slide),
                RoomSide.Top => new Room(width, height, point.x + slide, point.y),
                RoomSide.Bottom => new Room(width, height, point.x + slide, point.y),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        /// <summary>
        /// Regenerate the map with the current <see cref="settings"/>.
        /// </summary>
        private void Regenerate()
        {
            this.rooms.Clear();
            this.tilemap.ClearAllTiles();
            
            Room baseRoom = GenerateRoom(0, 0);
            this.rooms.Add(baseRoom);

            Room currentRoom = baseRoom;
            for (int i = 0; i < this.settings.roomCount; i++)
            {
                Room attachedRoom = GenerateBranchRoom(currentRoom);
                this.rooms.Add(attachedRoom);
                currentRoom = attachedRoom;
            }

            Room containing = GetContainingRoom();
            ShiftAllRooms(-containing.x, -containing.y);
            
            this.tiles = new bool[containing.width, containing.height];
            this.tilesWidth = containing.width;
            this.tilesHeight = containing.height;
            
            for (int y = 0; y < containing.width; y++)
                for (int x = 0; x < containing.height; x++)
                    this.tiles[x, y] = false;
            
            
        }

        private void Start()
        {
            this.tilemap = GetComponent<Tilemap>();
            Regenerate();
        }
    }

    public struct WorldgenSettings
    {
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
    }
    public enum RoomSide
    {
        Left, Right, Top, Bottom
    }
    public struct Room
    {
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
        }

        public int Left => this.x;
        public int Right => this.x + this.width;
        public int Top => this.y;
        public int Bottom => this.y + this.height;

        /// <summary>
        /// Returns a random point that touches the wall of this room.
        /// </summary>
        /// <returns></returns>
        public Vector2Int GetRandomPointOnWall(RoomSide side, out Vector2Int direction)
        {
            int x, y;
            switch (side)
            {
                case RoomSide.Left:
                    x = this.Left - 1;
                    y = Random.Range(this.Top, this.Bottom + 1);
                    direction = new Vector2Int(-1, 0);
                    break;
                case RoomSide.Right:
                    x = this.Right + 1;
                    y = Random.Range(this.Top, this.Bottom + 1);
                    direction = new Vector2Int(1, 0);
                    break;
                case RoomSide.Top:
                    x = Random.Range(this.Left, this.Right + 1);
                    direction = new Vector2Int(0, -1);
                    y = this.Top - 1;
                    break;
                case RoomSide.Bottom:
                    x = Random.Range(this.Left, this.Right + 1);
                    direction = new Vector2Int(0, 1);
                    y = this.Bottom + 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            return new Vector2Int(x, y);
        }
    }
}