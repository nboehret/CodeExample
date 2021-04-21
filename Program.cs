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

            PlayLoop();
        }

        void PlayLoop()
        {
            bool exit = false;
            while (exit == false)
            {
                if (_currentRoom == null || _map == null)
                {
                    Console.WriteLine("Error with room or map");
                    return;
                }

                Console.WriteLine(_currentRoom.RoomDesc);

                if (_currentRoom.EndingRoom == true)
                {
                    Console.WriteLine("!!! YOU WIN !!!");
                    return;
                }

                Console.WriteLine(DoorChoices() + "\n");
                Console.Write("What number door will you take: ");
                string input = Console.ReadLine();
                if (input.ToLower() == "e" || input.ToLower() == "exit")
                {
                    exit = true;
                }
                else
                {
                    int doorChoice = ValidateDoorChoice(input);
                    if (doorChoice != -1)
                    {
                        doorChoice--; // to move it to the index of the door not the number displayed to the user
                        Door selectedDoor = _currentRoom.Doors[doorChoice];
                        Console.WriteLine($"You go through the {selectedDoor.DoorColor} door.");
                        _currentRoom = _map.Rooms.FirstOrDefault(x => x.RoomId == selectedDoor.ConnectsToRoomId);
                    }
                }
            }
        }

        string DoorChoices()
        {
            string ret = "You see ";

            if (_currentRoom.Doors.Count > 1)
            {
                ret += "some doors. There is ";
            }
            else
            {
                ret += "a door. It is ";
            }

            int doorNum = 0;
            foreach (Door door in _currentRoom.Doors)
            {
                if (doorNum > 0)
                {
                    ret += " and ";
                }
                ret += $"a {door.DoorColor} [{++doorNum}] door";
            }
            ret += ".";

            return ret;
        }

        // Validates that the user inputs a number and that the number is of a door in the room
        // Returns the door number inputted by the user
        // Will return -1 if not a valid door
        int ValidateDoorChoice(string userInput)
        {
            int ret = -1;
            bool isNumber = int.TryParse(userInput, out ret);
            if (isNumber == false)
            {
                Console.WriteLine("Please input a number of a valid door.");
            }
            else
            {
                if (ret > 0 && ret <= _currentRoom.Doors.Count)
                {
                    // good to go
                }
                else
                {
                    ret = -1;
                    Console.WriteLine("Please input a number of a valid door.");
                }
            }

            return ret;
        }

        // Loads the Map from the mapFile location
        // Returns true if the map loaded successfully
        bool LoadMap(string mapFile)
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
        bool ValidateMap()
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
            List<int> visitedRooms = new List<int>();
            bool canExit = ValidateExitRecursive(_currentRoom, ref visitedRooms, 0);

            if (canExit == false)
            {
                Console.WriteLine("Cannot get from start to the exit, quitting...");
                return false;
            }
            
            return true;
        }

        bool ValidateExitRecursive(Room room, ref List<int> visitedRooms, int depth)
        {
            bool foundExit = false;

            visitedRooms.Add(room.RoomId);

            if (room.EndingRoom == true)
            {
                foundExit = true;
            }
            else
            {
                foreach (Door door in room.Doors)
                {
                    // see if we have already been there, if not continue
                    if (visitedRooms.Contains(door.ConnectsToRoomId) == false)
                    {
                        // this checks if a room exists and then passes that connecting room for the next level of search
                        Room nextRoom = _map.Rooms.FirstOrDefault(x => x.RoomId == door.ConnectsToRoomId);
                        if (nextRoom != null)
                        {
                            foundExit = ValidateExitRecursive(nextRoom, ref visitedRooms, depth + 1);
                        }

                        if (foundExit == true)
                        {
                            return foundExit;
                        }
                    }
                }
            }

            return foundExit;
        }

        void CreateBasicMap()
        {
            Door d1 = new Door() { DoorColor = "faded brown", ConnectsToRoomId = 2 };
            Room r1 = new Room() { RoomId = 1, StartingRoom = true, EndingRoom = false, RoomDesc = "You are in a poorly lit room with walls of crude brick.", Doors = new List<Door> { d1 } };
            Room r2 = new Room() { RoomId = 2, StartingRoom = false, EndingRoom = true, RoomDesc = "You step out of the cave and into the light. You made it out." };

            Map m = new Map() {Rooms = new List<Room> {r1, r2}};

            File.WriteAllText("map.json", JsonSerializer.Serialize<Map>(m));
        }

        void CreateMap()
        {
            Door d_r1_r2 = new Door() { DoorColor = "faded brown", ConnectsToRoomId = 2 };
            Door d_r2_r1 = new Door() { DoorColor = "faded brown", ConnectsToRoomId = 1 };
            Door d_r1_r3 = new Door() { DoorColor = "rusty iron", ConnectsToRoomId = 3 };
            Door d_r3_r1 = new Door() { DoorColor = "rusty iron", ConnectsToRoomId = 1 };
            Door d_r3_r4 = new Door() { DoorColor = "cracked wooden", ConnectsToRoomId = 4};
            Room r1 = new Room() { RoomId = 1, StartingRoom = true, EndingRoom = false, RoomDesc = "You are in a poorly lit room with walls of crude brick.", Doors = new List<Door> { d_r1_r2, d_r1_r3 } };
            Room r2 = new Room() { RoomId = 2, StartingRoom = false, EndingRoom = false, RoomDesc = "You are in a carved out room. A campfire is begining to go out in the middle of the room.", Doors = new List<Door> { d_r2_r1 } };
            Room r3 = new Room() { RoomId = 3, StartingRoom = false, EndingRoom = false, RoomDesc = "You are in a room with planks of wood lining the walls keeping back the dirt and rock that is pushing its way in.", Doors = new List<Door> { d_r3_r1, d_r3_r4 } };
            Room r4 = new Room() { RoomId = 4, StartingRoom = false, EndingRoom = true, RoomDesc = "You step out of the cave and into the light. You made it out." };

            Map m = new Map() {Rooms = new List<Room> {r1, r2, r3, r4}};

            File.WriteAllText("map.json", JsonSerializer.Serialize<Map>(m));
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
