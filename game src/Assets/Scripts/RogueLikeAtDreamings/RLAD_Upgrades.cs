using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RogueLikeAtDreamings.Rooms;

public enum PermanentUpgradeTypes
{
    Door,
    Power,
    Time,
    Plushies,
    Internet,
}

public enum RLAD_PurchasablePermanentUpgrades
{
    LuringSystem,
//  HomeTheatre,
//  SolarSystem,
    FiberOpticConnection, // slows internet usage
    NintendoEntertainmentSystem, // stops gaster from noticing internet outages
    ShopMembership // extra rerolls
}

[Serializable]
public struct RLADPermanentUpgrades
{
    public RLADPermanentUpgrades(bool filler = true)
    {
        doorStrength = 0;
        powerDrainage = 0;
        timeSpeed = 1;
        numPlushies = 0;
        internetSpeed = 0;
        hasLuringSystem = false;
        hasHomeTheatre = false;
        solarSystem = false;
        fiberConnection = false;
        nes = false;
        membership = false;
    }

    public int doorStrength;
    public int powerDrainage;
    public int timeSpeed;      // TODO: find lore reason as to why you can just make time go faster
    public int numPlushies;
    public int internetSpeed;
    public bool hasLuringSystem;
    public bool hasHomeTheatre;
    public bool solarSystem;
    public bool fiberConnection;
    public bool nes;
    public bool membership;
}

public enum TemporaryUpgradeTypes
{
    MultiplayerSteamGame,
    ClickteamOneDay,
    AdobeOneDay,
//  BananaPeel,
    PrescriptionMedication,
//  BatteryPack,
    SleepingPills,
    PrepaidSimCard,
    MaintenanceGuy
}

public enum RLAD_Items
{
    SingleBluray,
    CakePremix,
    BurnerPhone,
    Fertiliser,
    SpaghettiDelivery,
}

[Serializable]
public struct RLADTemporaryUpgrades
{
    public RLADTemporaryUpgrades(bool filler = true)
    {
        multiplayerSteamGame = 0;
        clickteamOneDay = false;
        adobeOneDay = false;
        bananaPeel = null;
        prescriptionMedication = false;
        sleepingPills = false;
        extraData = false;
        maintenanceGuy = false;
    }

    public int multiplayerSteamGame; // lasts 3 days
    public bool clickteamOneDay;
    public bool adobeOneDay;
    public Room? bananaPeel;
    public bool prescriptionMedication;
    public bool sleepingPills;
    public bool extraData;
    public bool maintenanceGuy;
}