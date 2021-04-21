using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CodeExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Game g = new Game();
        }
    }

    class Game
    {
        Map _map;
        Room _currentRoom;

        public Game()
        {
            bool mapLoaded = LoadMap("map.json");
            if (mapLoaded == false) { return; }

            bool mapValid = ValidateMap();
            if (mapValid == false) { return; }


        }

        // Loads the Map from the mapFile location
        // Returns true if the map loaded successfully
        public bool LoadMap(string mapFile)
        {
            //Console.WriteLine($"Looking for {mapFile}");
            if (File.Exists(mapFile) == true)
            {
                string mapData = File.ReadAllText(mapFile);
                _map = JsonSerializer.Deserialize<Map>(mapData);
                return true;
            }
            else
            {
                Console.WriteLine($"Cannot find {mapFile}, quitting...");
                return false;
            }
        }

        // Vakudates the loaded map, it must have one entrance and at least one exit and there is a path to from the start to end
        // Returns true if map validation was successful
        public bool ValidateMap()
        {
            if (_map == null)
            {
                Console.WriteLine("No map data to read, quitting...");
                return false;
            }

            // check for start and end(s)
            int starts = 0;
            int ends = 0;
            foreach (Room room in _map.Rooms)
            {
                if (room.StartingRoom == true)
                {
                    starts++;
                    _currentRoom = room;
                }
                if (room.EndingRoom == true)
                {
                    ends++;
                }
            }

            if (starts == 1 && ends >= 1)
            {
                // good to go
            }
            else
            {
                Console.WriteLine("Map has too few entrances or exits, quitting...");
                return false;
            }

            // check if you can get from the start to the exit
            bool canExit = ValidateExitRecursive(_currentRoom, 0);

            if (canExit == false)
            {
                Console.WriteLine("Cannot get from start to the exit, quitting...");
                return false;
            }
            
            return true;
        }

        bool ValidateExitRecursive(Room room, int depth)
        {
            bool foundExit = false;

            if (room.EndingRoom == true)
            {
                foundExit = true;
            }
            else
            {
                foreach (Door door in room.Doors)
                {
                    // this checks if a room exists and then passes that connecting room for the next level of search
                    Room nextRoom = _map.Rooms.FirstOrDefault(x => x.RoomId == door.ConnectsToRoomId);
                    if (nextRoom != null)
                    {
                        foundExit = ValidateExitRecursive(nextRoom, depth + 1);
                    }

                    if (foundExit == true)
                    {
                        return foundExit;
                    }
                }
            }

            return foundExit;
        }

        public void CreateBasicMap()
        {
            Door d1 = new Door() { DoorColor = "faded brown", ConnectsToRoomId = 2 };
            Room r1 = new Room() { RoomId = 1, StartingRoom = true, EndingRoom = false, RoomDesc = "You are in a poorly lit room with walls of crude brick.", Doors = new List<Door> { d1 } };
            Room r2 = new Room() { RoomId = 2, StartingRoom = false, EndingRoom = true, RoomDesc = "You step out of the cave and into the light. You made it out." };

            Map m = new Map() {Rooms = new List<Room> {r1, r2}};

            File.WriteAllText("map.json", JsonSerializer.Serialize<Map>(m));
        }

        public void PlayLoop()
        {
            bool exit = false;
            while (exit == false)
            {
                
            }
        }
    }

    public class Map
    {
        public IList<Room> Rooms { get; set; }
    }

    public class Room
    {
        public int RoomId { get; set; }
        public bool StartingRoom { get; set; }
        public bool EndingRoom { get; set; }
        public IList<Door> Doors { get; set; }
        public string RoomDesc { get; set; }
    }

    public class Door
    {
        public string DoorColor { get; set; }
        public int ConnectsToRoomId { get; set; }
    }
}
