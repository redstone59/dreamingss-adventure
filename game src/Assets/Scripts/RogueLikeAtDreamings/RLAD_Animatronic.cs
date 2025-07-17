using System;
using System.Collections;
using System.Collections.Generic;
using RogueLikeAtDreamings;
using RogueLikeAtDreamings.Elevator;
using RogueLikeAtDreamings.Rooms;
using UnityEngine;

public class RLADAnimatronic : MonoBehaviour
{
    private Animatronic animatronic;

    public int startAILevel;
    /// <summary>
    /// The number of Fixed Updates that must pass before a movement opportunity.
    /// </summary>
    public int opportunitySpeed;
    /// <summary>
    /// The variation in `opportunitySpeed`
    /// </summary>
    public int opportunityWiggle;

    private int nextMovementOpportunity;
    private int fixedUpdateCount = 0;
    public Func<Room, (bool, bool)> moveFunction;

    public bool InBasement;
    private bool _wasFrozen;
    public float Patience;
    public float PatienceMultiplier;

    public AudioSource stepSource;
    public AudioClip closeStepsClip;
    public float closeStepsVolume;
    public float closeStepsPan;
    public AudioClip farStepsClip;
    public float farStepsVolume;
    public float farStepsPan;

    public int AILevel
    { 
        get { return animatronic._aiLevel; } 
        set 
        {
            animatronic._aiLevel = value;
        }
    }
    public Room CurrentRoom
    {
        get { return animatronic._currentRoom; }
        set
        {
            animatronic._currentRoom = value;
        }
    }
    public float aggression
    {
        get { return animatronic._aggression; }
        set
        {
            animatronic._aggression = value;
        }
    }
    public Room[] NonAttackTargets
    {
        get { return animatronic._nonAttackTargets; }
        set
        {
            animatronic._nonAttackTargets = value;
        }
    }
    public Room Target
    {
        get { return animatronic._targetRoom; }
        set { animatronic._targetRoom = value; }
    }

    public List<Room> TargetQueue
    {
        get { return animatronic.TargetQueue; }
        set { animatronic.TargetQueue = value; }
    }

    public bool PreferElevator
    {
        get { return animatronic._preferElevator; }
        set { animatronic._preferElevator = value; }
    }

    public void NewMovementOpportunity()
    {
        int wiggle = UnityEngine.Random.Range(-opportunityWiggle, opportunityWiggle);
        nextMovementOpportunity = opportunitySpeed + wiggle;
    }

    void Awake()
    {
        stepSource = GetComponent<AudioSource>();
        animatronic = new(startAILevel)
        {
            Stun = Stun,
            Step = PlaySteps
        };
        _wasFrozen = false;
        InBasement = false;
        Patience = 0;
    }

    public void UpdatePatience()
    {
        if (!InBasement)
        {
            animatronic._outOfAction = _wasFrozen;
            return;
        }
        if (CurrentRoom == Room.Basement_Dead) return;
        animatronic._outOfAction = true;
        Patience += Time.deltaTime;
        
        if (Patience >= 5 * PatienceMultiplier)
        {
            Room nextRoom = RoomUtilities.GetClosestRooms(animatronic._manager, CurrentRoom, Room.Basement_Dead)[0];
            if (moveFunction(nextRoom).Item1)
            {
                if (nextRoom != Room.Basement_Dead)
                    PlaySteps(nextRoom != Room.Basement_Far);

                CurrentRoom = nextRoom;
                Patience = 0;
            }
        }
        if (Patience < -1f)
        {
            Room previousRoom = RoomUtilities.GetClosestRooms(animatronic._manager, CurrentRoom, Room.Stairwell_Bottom)[0];
            if (CurrentRoom == Room.Basement_PointOfNoReturn) return; // haha thats funny no but really this just stops the insta-kill on flash
            if (moveFunction(previousRoom).Item1)
            {
                PlaySteps(previousRoom == Room.Basement_Close);
                if (previousRoom == Room.Stairwell_Bottom)
                {
                    InBasement = false;
                    previousRoom = NonAttackTargets[UnityEngine.Random.Range(0, NonAttackTargets.Length)];
                    Target = NonAttackTargets[UnityEngine.Random.Range(0, NonAttackTargets.Length)];
                    animatronic._outOfAction = _wasFrozen;
                    Stun(UnityEngine.Random.Range(2, 5) * 20);
                }
                CurrentRoom = previousRoom;
            }
        }
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if (fixedUpdateCount++ < nextMovementOpportunity)
            return;
        
        fixedUpdateCount = 0;
        NewMovementOpportunity();
        animatronic.MovementOpportunity(moveFunction);
        Debug.Log($"{gameObject.name} movement opportunity with freeze set to {animatronic._outOfAction}");
    }

    public void ForceTargetQueue()
    {
        if (TargetQueue.Count > 0)
        {
            Target = TargetQueue[0];
            TargetQueue.RemoveAt(0);
            if (RoomUtilities.GetFloor(CurrentRoom, animatronic._manager) == Floor.Basement)
            {
                CurrentRoom = Room.Stairwell_Bottom;
            }
        }
    }

    public void ForceMovementOpportunity()
    {
        fixedUpdateCount = nextMovementOpportunity + 1;
    }

    public void Stun(int fixedUpdates)
    {
        nextMovementOpportunity += fixedUpdates;
    }

    public void Freeze(bool isFrozen)
    {
        _wasFrozen = isFrozen;
        animatronic._outOfAction = isFrozen;
    }

    public void SetManager(RLAD_GameManager _manager)
    {
        animatronic._manager = _manager;
    }

    public void PlaySteps(bool close)
    {
        stepSource.clip = close ? closeStepsClip : farStepsClip;
        stepSource.volume = close ? closeStepsVolume : farStepsVolume;
        stepSource.panStereo = close ? closeStepsPan : farStepsPan;

        stepSource.pitch = 1 + UnityEngine.Random.Range(-0.2f, 0.2f);
        stepSource.Play();
    }
}

class Animatronic
{
    /// <summary>
    /// Ranges from 0 to 100, determines movement chance.
    /// </summary>
    public int _aiLevel;
    public float _aggression;
    public Room _targetRoom;
    public List<Room> TargetQueue;
    public Room[] _nonAttackTargets;
    public Room _currentRoom;
    public bool _outOfAction;
    public RLAD_GameManager _manager;
    public bool _preferElevator = true;
    public int TaskTime;

    public Action<int> Stun;
    public Action<bool> Step;

    public Animatronic(int startLevel, bool inAction = false, Room startRoom = Room.DreamingsRoom)
    {
        _aiLevel = startLevel;
        _currentRoom = startRoom;
        _outOfAction = inAction;
        TargetQueue = new();
    }

    private void DecideNextTargetRoom()
    {
        if (TargetQueue.Count > 0)
        {
            _targetRoom = TargetQueue[0];
            TargetQueue.RemoveAt(0);
            return;
        }

        if (UnityEngine.Random.Range(0, 1f) <= _aggression)
        {
            _targetRoom = Room.Basement_Dead;
            return;
        }

        Room previousTarget = _targetRoom;
        do
        {
            _targetRoom = _nonAttackTargets[UnityEngine.Random.Range(0, _nonAttackTargets.Length)];
        }
        while (_targetRoom == previousTarget);
    }

    private Room GetIntermediate()
    {
        Floor currentFloor = RoomUtilities.GetFloor(_currentRoom, _manager);

        bool transitioningBetweenFloors = new List<Room>()
        {
            Room.Elevator,
            Room.Stairwell_Bottom,
            Room.Stairwell_Middle,
            Room.Stairwell_Top 
        }.Contains(_currentRoom);

        if (!transitioningBetweenFloors && RoomUtilities.GetFloor(_targetRoom, _manager) != currentFloor)
        {
            bool goToElevator = _preferElevator && (RoomUtilities.GetFloor(_currentRoom, _manager) == _manager.elevator.floor);
            return goToElevator ? Room.Elevator : _targetRoom;
        }
        else
        {
            return _targetRoom;
        }
    }

    public (bool succeeded, Room? movedTo) MovementOpportunity(Func<Room, (bool canMove, bool reroll)> callback)
    {
        // FNAF AI rolls a random number, and if that number matches or exceeds the AI level, then it can move
        // The check below is the inverse of that.
        // Random(20) < AI and Random(20) + 1 <= AI are used in FNAF1 and 2
        if (_outOfAction || (UnityEngine.Random.Range(0, 100) >= _aiLevel))
            return (false, null);

        bool reroll;
        Room? movedTo;

        do
        {
            Room intermediate = GetIntermediate();

            Room[] connected = RoomUtilities.GetConnectedRooms(_currentRoom, _manager);
            Room[] closestRooms = RoomUtilities.GetClosestRooms(_manager, _currentRoom, intermediate).ToArray();

            Floor currentFloor = RoomUtilities.GetFloor(_currentRoom, _manager);
            bool chanceToBeDistracted = connected.Length == 0
                                            ? false
                                            : UnityEngine.Random.Range(0, 1f) <= 0.2f;
            bool distracted = currentFloor != Floor.Basement && chanceToBeDistracted;
            Debug.Log(distracted);
            movedTo = distracted ? 
                        connected[UnityEngine.Random.Range(0, connected.Length)] :
                        closestRooms[UnityEngine.Random.Range(0, closestRooms.Length)];
            

            bool canMove;
            (canMove, reroll) = callback((Room)movedTo);
            if (!canMove)
            {
                if (reroll) continue;
                return (false, null);
            }
        }
        while (reroll);

        _currentRoom = (Room)movedTo;
        Debug.Log($"Moved to {_currentRoom}");
        if (_currentRoom == _targetRoom)
        {
            DecideNextTargetRoom();
            TaskTime = _manager.player.currentChallenge?.positive == Patches.TheChallenge
                           ? (int)(1 / Time.fixedDeltaTime)
                           : UnityEngine.Random.Range(
                             (int)(3 / Time.fixedDeltaTime),
                             (int)(12 / Time.fixedDeltaTime)
                            );
            Stun(TaskTime);
        }
        if (_currentRoom == Room.Basement_Far)
        {
            Step(false);
        }
        return (true, movedTo);
    }
}