using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
//using Enumerate = UnityEngine.InputSystem.InputControlExtensions.Enumerate;

#nullable enable
public struct InputEvent
{
    public double time;
    public bool isKeyEvent;
    public KeyControl[]? keyControls;
    public Vector2? mouseYPosition;

    public override string ToString()
    {
        return $"Input({time:N2}, {(isKeyEvent ? "key" : "other")}, '{(keyControls != null ? string.Join("", keyControls.Select(i => i.wasPressedThisFrame ? i.displayName: "")) : "")}', {mouseYPosition})";
    }
}
#nullable restore

public static class InputManager
{
    //public const Enumerate DEFAULT_CONTROL_ENUMERATION_FLAGS =
    //    Enumerate.IgnoreControlsInCurrentState; //  

    private static List<InputEvent> inputQueue;

    private static double beforeUpdateTime;
    private static double afterUpdateTime;
    private static double latestInputTime;
    
    public static double InitaliseTime;
    public static double InputUpdateTime { get; private set; }
    public static double CurrentInputTime => InputState.currentTime;

    private static Vector2Control previousMousePosition = null;

    public static InputEvent[] InputQueue
    {
        get
        {
            InputEvent[] queuedInputs = inputQueue.ToArray();
            inputQueue.Clear();
            return queuedInputs;
        }
    }
    public static bool Initialised
    {
        get
        {
            return InitaliseTime != 0;
        }
    }

    public static void Initialise()
    {
        inputQueue = new(64);

        InputSystem.pollingFrequency = 500;
        InputSystem.onEvent += OnEvent;
        InputSystem.onBeforeUpdate += OnBeforeUpdate;
        InputSystem.onAfterUpdate += OnAfterUpdate;

        InitaliseTime = CurrentInputTime;
    }

    public static void Destroy()
    {
        InputSystem.onEvent -= OnEvent;
        InputSystem.onBeforeUpdate -= OnBeforeUpdate;
        InputSystem.onAfterUpdate -= OnAfterUpdate;
        inputQueue.Clear();

        InitaliseTime = 0;
    }

    private static void OnBeforeUpdate()
    {
        if (!Initialised) return;
        beforeUpdateTime = CurrentInputTime;
    }

    private static void OnAfterUpdate()
    {
        if (!Initialised) return;
        afterUpdateTime = CurrentInputTime;
        InputUpdateTime = Math.Max(beforeUpdateTime, latestInputTime);
    }

    private static void OnEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!Initialised) return;
        double currentTime = CurrentInputTime;

        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        latestInputTime = Math.Max(latestInputTime, eventPtr.time);

        if (device is Keyboard keyboard)
        {
            KeyControl[] allKeyControls = keyboard.allKeys.ToArray();

            inputQueue.Add(
                new()
                {
                    time = currentTime,
                    isKeyEvent = true,
                    keyControls = allKeyControls,
                    mouseYPosition = null
                }
            );
        }
        else if (device is Mouse mouse)
        {
            if (previousMousePosition != null && mouse.position == previousMousePosition)
                return;

            inputQueue.Add(
                new()
                {
                    time = currentTime,
                    isKeyEvent = false,
                    keyControls = null,
                    mouseYPosition = mouse.position.ReadValue()
                }
            );
        }
    }
}