/* Created by Nick Boehret
 * 
 * This is a code example to show my coding ability.
 * This code uses .net core 3.1 and demonstartes JSON deserialization and recursion
 * 
 * The main flow of the program is read and deserialize map.json, validate the created objects,
 * and display them using a game like interface.
 *
 * Please see detailed comments above functions and classes for more descriptions.
 */

using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CodeExample
{
    // Main entry point of the code
    class Program
    {
        static void Main(string[] args)
        {
            Game g = new Game();
        }
    }

    class Game
    {
        // stores the loaded in map for use across the class
        Map _map;
        // stores the current room for displaying room information to the user
        Room _currentRoom;

        //
        // the main area of the code where loading, validation, and display (playLoop) occurs
        //
        public Game()
        {
            bool mapLoaded = LoadMap("map.json");
            if (mapLoaded == false) { return; }

            bool mapValid = ValidateMap();
            if (mapValid == false) { return; }

            Console.WriteLine("Ready. Type E or Exit at anytime to quit.");

            PlayLoop();
        }

        // Loads the Map from the mapFile location
        // Returns true if the map loaded successfully
        bool LoadMap(string mapFile)
        {
            // checks to see if the file exisits
            if (File.Exists(mapFile) == true)
            {
                string mapData = File.ReadAllText(mapFile);
                // passes read file data to the deserializer
                _map = JsonSerializer.Deserialize<Map>(mapData);
                return true;
            }
            else
            {
                Console.WriteLine($"Cannot find {mapFile}, quitting...");
                return false;
            }
        }

        // Validates the loaded map, it must have one entrance and at least one exit and there is a path to from the start to end
        // Returns true if map validation was successful
        bool ValidateMap()
        {
            // checks just in case we got here somehow and the map wasn't loaded
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
                    // presets the current room so an additional loop isn't needed
                    _currentRoom = room;
                }
                if (room.EndingRoom == true)
                {
                    ends++;
                }
            }

            // checks to see if the number of Starting points and ending points are allowed
            // a positive check of the logic here allows for easier code reading
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
            // we don't need to check if _currentRoom is not null because it will be set if the above validated
            List<int> visitedRooms = new List<int>();
            bool canExit = ValidateExitRecursive(_currentRoom, ref visitedRooms, 0);

            if (canExit == false)
            {
                Console.WriteLine("Cannot get from start to the exit, quitting...");
                return false;
            }
            
            return true;
        }

        // Starts off in the _currentRoom which was noted as the starting point
        // Will go through each door that links room to room until the ending point is found
        //
        // It notates which room it has been in to prevent infinite loops for doors that go back to the room
        // they came from.
        bool ValidateExitRecursive(Room room, ref List<int> visitedRooms, int depth)
        {
            bool foundExit = false;

            // adds room id to list so we don't repeat room searches
            visitedRooms.Add(room.RoomId);

            if (room.EndingRoom == true)
            {
                foundExit = true;
            }
            else
            {   
                // loops through all doors in the room
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

                        // allows early validation because we only need to be able to get to one exit to finish
                        if (foundExit == true)
                        {
                            return foundExit;
                        }
                    }
                }
            }

            return foundExit;
        }

        // The main display loop
        // Shows the user which room they are in and gives options to proceed
        // to the next room by selecting the door by number
        void PlayLoop()
        {
            bool exit = false;
            while (exit == false)
            {
                // just incase something happened to the current room or map we check to make sure they are still there
                if (_currentRoom == null || _map == null)
                {
                    Console.WriteLine("Error with room or map");
                    return;
                }

                // prints out the room description to identify where the user is
                Console.WriteLine(_currentRoom.RoomDesc);

                // once at the ending room it finishes the display
                if (_currentRoom.EndingRoom == true)
                {
                    Console.WriteLine("!!! YOU WIN !!!");
                    return;
                }

                // lists door choices and validates that the choice given is okay
                Console.WriteLine(DoorChoices() + "\n");
                Console.Write("What number door will you take: ");
                string input = Console.ReadLine();
                // accepts e or exit to quit at any time
                if (input.ToLower() == "e" || input.ToLower() == "exit")
                {
                    exit = true;
                }
                else
                {
                    // sends the input to the valiator to determine if it is a valid input
                    int doorChoice = ValidateDoorChoice(input);
                    if (doorChoice != -1)
                    {
                        doorChoice--; // to move it to the index of the door not the number displayed to the user
                        Door selectedDoor = _currentRoom.Doors[doorChoice];
                        Console.WriteLine($"You go through the {selectedDoor.DoorColor} door.");
                        // sets the current room by finding the first room in the list with that ID
                        _currentRoom = _map.Rooms.FirstOrDefault(x => x.RoomId == selectedDoor.ConnectsToRoomId);
                    }
                }
            }
        }

        // creates a string to display what doors are availble to choose from for the user
        // a good example of output from this is: "You see a door. It is a brown [1] door."
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

        // A utility function used to create a JSON map file to load
        void CreateBasicMap()
        {
            Door d1 = new Door() { DoorColor = "faded brown", ConnectsToRoomId = 2 };
            Room r1 = new Room() { RoomId = 1, StartingRoom = true, EndingRoom = false, RoomDesc = "You are in a poorly lit room with walls of crude brick.", Doors = new List<Door> { d1 } };
            Room r2 = new Room() { RoomId = 2, StartingRoom = false, EndingRoom = true, RoomDesc = "You step out of the cave and into the light. You made it out." };

            Map m = new Map() {Rooms = new List<Room> {r1, r2}};

            File.WriteAllText("map.json", JsonSerializer.Serialize<Map>(m));
        }

        // A utility function used to create a JSON map file to load
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

    // For deserializing JSON to a Map Object
    public class Map
    {
        public IList<Room> Rooms { get; set; }
    }

    // For deserializing JSON to a Room object
    public class Room
    {
        public int RoomId { get; set; }
        public bool StartingRoom { get; set; }
        public bool EndingRoom { get; set; }
        public IList<Door> Doors { get; set; }
        public string RoomDesc { get; set; }
    }

    // For deserializing JSON to a Door object
    public class Door
    {
        public string DoorColor { get; set; }
        public int ConnectsToRoomId { get; set; }
    }
}
