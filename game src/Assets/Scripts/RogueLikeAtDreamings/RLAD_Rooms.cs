using System.Collections.Generic;
using RogueLikeAtDreamings.Elevator;
using UnityEngine;

namespace RogueLikeAtDreamings.Rooms
{
    public enum Room
    {
        /*
        DreamingsRoom,
        Ensuite,
        LivingRoom,
        Theatre,
        DiningRoom,
        Kitchen,
        Bathroom,
        Toilet,
        Hallway,
        StorageRoom,
        FrontDoor,
        Office
        */

        LivingRoom,
        Laundry,
        Bathroom,
        DiningRoom,
        Kitchen,
        FrontDoor,
        Veranda,

        Hallway,
        BlurayCollection,
        GastersRoom,
        DreamingsRoom,
        Ensuite,

        Stairwell_Top,
        Stairwell_Middle,
        Stairwell_Bottom,
        Elevator,

        Basement_Dead,
        Basement_PointOfNoReturn,
        Basement_Close,
        Basement_Far
    }

    public class RoomUtilities
    {
        public static Room[] GetConnectedRooms(Room room, RLAD_GameManager manager)
        {
            return room switch
            {
                Room.Stairwell_Top    => new Room[] { Room.Stairwell_Middle, Room.Hallway },
                Room.Stairwell_Middle => new Room[] { Room.Stairwell_Top, Room.Stairwell_Bottom, Room.LivingRoom, Room.FrontDoor },
                Room.Stairwell_Bottom => new Room[] { Room.Stairwell_Middle, Room.Basement_Far },

                Room.Elevator => manager.elevator.floor switch
                {
                    Floor.Upper    => new Room[] { Room.Hallway },
                    Floor.Ground   => new Room[] { Room.FrontDoor },
                    Floor.Basement => new Room[] { Room.Basement_Far },
                    _ => throw new KeyNotFoundException("yeah idk")
                },

                // Basement

                Room.Basement_Far     => manager.elevator.floor == Floor.Basement ?
                                         new Room[] { Room.Stairwell_Bottom, Room.Basement_Close, Room.Elevator } :
                                         new Room[] { Room.Stairwell_Bottom, Room.Basement_Close },
                Room.Basement_Close   => new Room[] { Room.Basement_Far, Room.Basement_PointOfNoReturn },
                Room.Basement_PointOfNoReturn => new Room[] { Room.Basement_Dead },
                Room.Basement_Dead => new Room[] { },

                // Ground

                Room.FrontDoor        => manager.elevator.floor == Floor.Ground ?
                                         new Room[] { Room.LivingRoom, Room.Stairwell_Middle, Room.Elevator} :
                                         new Room[] { Room.LivingRoom, Room.Stairwell_Middle },
                Room.LivingRoom       => new Room[] { Room.Laundry, Room.FrontDoor, Room.DiningRoom},
                Room.Laundry          => new Room[] { Room.LivingRoom, Room.Bathroom },
                Room.Bathroom         => new Room[] { Room.Laundry },
                Room.DiningRoom       => new Room[] { Room.LivingRoom, Room.Veranda, Room.Kitchen },
                Room.Veranda          => new Room[] { Room.DiningRoom },
                Room.Kitchen          => new Room[] { Room.DiningRoom },

                // Upper

                Room.Hallway          => manager.elevator.floor == Floor.Upper ?
                                         new Room[] { Room.Stairwell_Top, Room.BlurayCollection, Room.DreamingsRoom, Room.GastersRoom, Room.Elevator } :
                                         new Room[] { Room.Stairwell_Top, Room.BlurayCollection, Room.DreamingsRoom, Room.GastersRoom },
                Room.BlurayCollection => new Room[] { Room.Hallway },
                Room.GastersRoom      => new Room[] { Room.Hallway },
                Room.DreamingsRoom    => new Room[] { Room.Hallway, Room.Ensuite },
                Room.Ensuite          => new Room[] { Room.DreamingsRoom },

                // Catch-all (shouldn't hit)
                _                               => throw new KeyNotFoundException("what the fuck")
            };
        }

        public static Floor GetFloor(Room room, RLAD_GameManager manager)
        {
            if (room == Room.Elevator) return manager.elevator.floor;

            List<Room> basementRooms = new()
            {
                Room.Stairwell_Bottom,
                Room.Basement_Close,
                Room.Basement_Far,
                Room.Basement_PointOfNoReturn,
                Room.Basement_Dead
            };
            if (basementRooms.Contains(room)) return Floor.Basement;

            List<Room> groundFloorRooms = new()
            {
                Room.Stairwell_Middle,
                Room.FrontDoor,
                Room.LivingRoom,
                Room.Laundry,
                Room.Bathroom,
                Room.DiningRoom,
                Room.Veranda,
                Room.Kitchen
            };
            if (groundFloorRooms.Contains(room)) return Floor.Ground;

            return Floor.Upper;
        }

        static List<Room> GetShortestPathBetween(RLAD_GameManager manager, List<Room> currentPath, Room end)
        {
            if (currentPath.Contains(end)) return currentPath;
            List<Room> workingPath = new(currentPath);
            List<Room> shortestPath = new(currentPath);
            for (int i = 0; i <= 10; i++) shortestPath.Add(Room.DreamingsRoom);

            foreach (Room neighbour in GetConnectedRooms(currentPath[^1], manager))
            {
                if (workingPath.Contains(neighbour)) continue;
                workingPath.Add(neighbour);
                if (neighbour == end) return workingPath;

                List<Room> newPath = GetShortestPathBetween(manager, workingPath, end);
                if (newPath.Count < shortestPath.Count)
                    shortestPath = newPath;
                
                workingPath.RemoveAt(workingPath.Count - 1);
            }

            return shortestPath;
        }

        public static int GetDistanceBetween(RLAD_GameManager manager, Room start, Room end)
        {
            return GetShortestPathBetween(manager, new() { start }, end).Count - 1;
        }

        public static List<Room> GetClosestRooms(RLAD_GameManager manager, Room currentRoom, Room target)
        {
            int shortestDistance = int.MaxValue;
            List<Room> closest = new();

            foreach (Room room in GetConnectedRooms(currentRoom, manager))
            {
                int distance = GetDistanceBetween(manager, room, target);
                if (distance > shortestDistance) continue;
                if (distance == shortestDistance)
                {
                    closest.Add(room);
                    continue;
                }
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closest.Clear();
                    closest.Add(room);
                }
            }

            return closest;
        }
    }
}