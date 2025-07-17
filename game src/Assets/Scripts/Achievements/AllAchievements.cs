using UnityEngine;
using Achievements;
using AchievementEnums;
using System;

public class AllAchievements
{
    private static Func<bool> HasSeenLevel(string sceneName)
    {
        if (PlayerPrefs.GetInt("NumberOfVictories", 0) != 0) return () => true;
        return () => PlayerPrefs.GetInt("HighestSavedLevel", 0) >= LevelOrder.GetLevelIndex(sceneName);
    }

    // Misc.

    public static Achievement Adventurer = new()
    {
        name = "Adventurer",
        description = "Become dreaming.",
        image = Resources.Load<Texture>("dreaming"),
        key = AchievementKeys.OutsideOfMinigame,
        bitPosition = (int)OutsideOfMinigame.Adventurer,
        condition = () => true
    };

    public static Achievement Adventured = new()
    {
        name = "Adventured.",
        description = "Complete dreamings's adventure: a game.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/OutsideOfMinigame/Adventured"),
        key = AchievementKeys.OutsideOfMinigame,
        hidden = false,
        bitPosition = (int)OutsideOfMinigame.Adventured,
        condition = () => AchievementUtils.HasUnlocked(Adventurer)
    };

    public static Achievement Retired = new()
    {
        name = "Retired.",
        description = "Complete dreamings's adventure: a game on Hard.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/OutsideOfMinigame/Retired"),
        key = AchievementKeys.OutsideOfMinigame,
        hidden = false,
        bitPosition = (int)OutsideOfMinigame.Retired,
        condition = () => AchievementUtils.HasUnlocked(Adventured)
    };

    public static Achievement AfraidQuestionMark = new()
    {
        name = "Afraid?",
        description = "Get kicked out of the main menu for waiting too long.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/OutsideOfMinigame/AfraidQuestionMark"),
        key = AchievementKeys.OutsideOfMinigame,
        hidden = true,
        bitPosition = (int)OutsideOfMinigame.AfraidQuestionMark,
        condition = () => true
    };

    public static Achievement BadAtMath = new()
    {
        name = "Bad at Math",
        description = "Fail to delete your save because of math.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/OutsideOfMinigame/BadAtMath"),
        key = AchievementKeys.OutsideOfMinigame,
        hidden = true,
        bitPosition = (int)OutsideOfMinigame.BadAtMath,
        condition = () => true
    };

    // Fucking Dreaming And Gaster Tennis

    public static Achievement Sweep = new()
    {
        name = "Sweep!",
        description = "Win a game of FDAGT with a score of 5-0.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/FuckingDreamingAndGasterTennis/Sweep"),
        key = AchievementKeys.FuckingDreamingAndGasterTennis,
        hidden = false,
        bitPosition = (int)FuckingDreamingAndGasterTennis.Sweep,
        condition = HasSeenLevel("FuckingDreamingAndGasterTennis")
    };

    public static Achievement Swept = new()
    {
        name = "Swept.",
        description = "Lose a game of FDAGT with a score of 0-5.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/FuckingDreamingAndGasterTennis/Swept"),
        key = AchievementKeys.FuckingDreamingAndGasterTennis,
        hidden = false,
        bitPosition = (int)FuckingDreamingAndGasterTennis.Swept,
        condition = HasSeenLevel("FuckingDreamingAndGasterTennis")
    };

    public static Achievement Zugzwang = new()
    {
        name = "Zugzwang",
        description = "Get Gaster to score an own goal in FDAGT.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/FuckingDreamingAndGasterTennis/Zugzwang"),
        key = AchievementKeys.FuckingDreamingAndGasterTennis,
        hidden = false,
        bitPosition = (int)FuckingDreamingAndGasterTennis.Zugzwang,
        condition = HasSeenLevel("FuckingDreamingAndGasterTennis")
    };

    public static Achievement BareMinimumKing = new()
    {
        name = "Bare Minimum King",
        description = "Hit the puck in FDAGT once and still score.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/FuckingDreamingAndGasterTennis/BareMinimumKing"),
        key = AchievementKeys.FuckingDreamingAndGasterTennis,
        hidden = false,
        bitPosition = (int)FuckingDreamingAndGasterTennis.BareMinimumKing,
        condition = HasSeenLevel("FuckingDreamingAndGasterTennis")
    };

    public static Achievement SafetyHazard = new()
    {
        name = "Safety Hazard",
        description = "Have the puck reach a velocity of 40u/s or higher.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/FuckingDreamingAndGasterTennis/SafetyHazard"),
        key = AchievementKeys.FuckingDreamingAndGasterTennis,
        hidden = false,
        bitPosition = (int)FuckingDreamingAndGasterTennis.SafetyHazard,
        condition = HasSeenLevel("FuckingDreamingAndGasterTennis")
    };

    public static Achievement ImagineTheNerves = new()
    {
        name = "Imagine the Nerves",
        description = "Get to game point in FDAGT.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/FuckingDreamingAndGasterTennis/ImagineTheNerves"),
        key = AchievementKeys.FuckingDreamingAndGasterTennis,
        hidden = false,
        bitPosition = (int)FuckingDreamingAndGasterTennis.ImagineTheNerves,
        condition = HasSeenLevel("FuckingDreamingAndGasterTennis")
    };
    
    public static Achievement NervesOvercame = new()
    {
        name = "Nerves: Overcame",
        description = "Win the game point in FDAGT.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/FuckingDreamingAndGasterTennis/NervesOvercame"),
        key = AchievementKeys.FuckingDreamingAndGasterTennis,
        condition = () => AchievementUtils.HasUnlocked(ImagineTheNerves),
        bitPosition = (int)FuckingDreamingAndGasterTennis.NervesOvercame
    };
    
    public static Achievement NervesImagined = new()
    {
        name = "Nerves: Imagined",
        description = "Lose the game point in FDAGT.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/FuckingDreamingAndGasterTennis/NervesImagined"),
        key = AchievementKeys.FuckingDreamingAndGasterTennis,
        condition = () => AchievementUtils.HasUnlocked(ImagineTheNerves),
        bitPosition = (int)FuckingDreamingAndGasterTennis.NervesImagined
    };

    // World's Hardest Golf

    public static Achievement Par = new()
    {
        name = "Par",
        description = "Complete World's Hardest Golf in less than 400 strokes.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/WorldsHardestGolf"),
        key = AchievementKeys.WorldsHardestGolf,
        bitPosition = (int)WorldsHardestGolf.Par,
        condition = HasSeenLevel("WorldsHardestGolf")
    };

    public static Achievement Birdie = new()
    {
        name = "Birdie",
        description = "Complete World's Hardest Golf in less than 300 strokes.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/WorldsHardestGolf"),
        key = AchievementKeys.WorldsHardestGolf,
        bitPosition = (int)WorldsHardestGolf.Birdie,
        condition = HasSeenLevel("WorldsHardestGolf")
    };

    public static Achievement Eagle = new()
    {
        name = "Eagle",
        description = "Complete World's Hardest Golf in less than 250 strokes.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/WorldsHardestGolf"),
        key = AchievementKeys.WorldsHardestGolf,
        bitPosition = (int)WorldsHardestGolf.Eagle,
        condition = HasSeenLevel("WorldsHardestGolf")
    };

    public static Achievement Albatross = new()
    {
        name = "Albatross",
        description = "Complete World's Hardest Golf in less than 200 strokes.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/WorldsHardestGolf"),
        key = AchievementKeys.WorldsHardestGolf,
        bitPosition = (int)WorldsHardestGolf.Albatross,
        condition = HasSeenLevel("WorldsHardestGolf")
    };

    public static Achievement Putter = new()
    {
        name = "Putter",
        description = "Complete World's Hardest Golf with less than 25 deaths.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/WorldsHardestGolf"),
        key = AchievementKeys.WorldsHardestGolf,
        bitPosition = (int)WorldsHardestGolf.Putter,
        condition = HasSeenLevel("WorldsHardestGolf")
    };

    public static Achievement Iron = new()
    {
        name = "Iron",
        description = "Complete World's Hardest Golf with less than 10 deaths.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/WorldsHardestGolf"),
        key = AchievementKeys.WorldsHardestGolf,
        bitPosition = (int)WorldsHardestGolf.Iron,
        condition = HasSeenLevel("WorldsHardestGolf")
    };

    public static Achievement Wood = new()
    {
        name = "Wood",
        description = "Complete World's Hardest Golf with less than 5 deaths.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/WorldsHardestGolf"),
        key = AchievementKeys.WorldsHardestGolf,
        bitPosition = (int)WorldsHardestGolf.Wood,
        condition = HasSeenLevel("WorldsHardestGolf")
    };

    public static Achievement Driver = new()
    {
        name = "Driver",
        description = "Complete World's Hardest Golf without dying.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/WorldsHardestGolf"),
        key = AchievementKeys.WorldsHardestGolf,
        bitPosition = (int)WorldsHardestGolf.Driver,
        condition = HasSeenLevel("WorldsHardestGolf")
    };

    public static Achievement Bogey = new()
    {
        name = "Bogey",
        description = "Score 20,000 or less in World's Hardest Golf.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/WorldsHardestGolf"),
        key = AchievementKeys.WorldsHardestGolf,
        hidden = true,
        bitPosition = (int)WorldsHardestGolf.Bogey,
        condition = HasSeenLevel("WorldsHardestGolf")
    };

    // Rogue Like At Dreamings

    public static Achievement TheFirstNight = new()
    {
        name = "The first night...",
        description = "Beat the first night of RLAD.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/RogueLikeAtDreamings/TheFirstNight"),
        key = AchievementKeys.RogueLikeAtDreamings,
        bitPosition = (int)AchievementEnums.RogueLikeAtDreamings.TheFirstNight,
        condition = HasSeenLevel("RogueLikeAtDreamings")
    };
    
    public static Achievement IsNeverUsuallyThatBadInAnyOfTheGames = new()
    {
        name = "...is never usually that bad in any of the games.",
        description = "Die on the first night of RLAD.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/RogueLikeAtDreamings/IsNeverUsuallyThatBadInAnyOfTheGames"),
        key = AchievementKeys.RogueLikeAtDreamings,
        bitPosition = (int)AchievementEnums.RogueLikeAtDreamings.IsNeverThatBadInAnyOfTheGames,
        condition = () => AchievementUtils.HasUnlocked(TheFirstNight)
    };

    public static Achievement Greenrunner = new()
    {
        name = "Greenrunner",
        description = "Finish a night without using the flash beacon.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/RogueLikeAtDreamings/Greenrunner"),
        key = AchievementKeys.RogueLikeAtDreamings,
        bitPosition = (int)AchievementEnums.RogueLikeAtDreamings.BuzzerBeater,
        condition = HasSeenLevel("RogueLikeAtDreamings")
    };

    public static Achievement YouDidIt = new()
    {
        name = "You did it!",
        description = "Beat the fifth night in RLAD.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/RogueLikeAtDreamings/YouDidIt"),
        key = AchievementKeys.RogueLikeAtDreamings,
        bitPosition = (int)AchievementEnums.RogueLikeAtDreamings.YouDidIt,
        condition = HasSeenLevel("RogueLikeAtDreamings")
    };

    public static Achievement Overachiever = new()
    {
        name = "Overachiever",
        description = "Beat the seventh night in RLAD.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/RogueLikeAtDreamings/Overachiever"),
        key = AchievementKeys.RogueLikeAtDreamings,
        bitPosition = (int)AchievementEnums.RogueLikeAtDreamings.Overachiever,
        condition = () => AchievementUtils.HasUnlocked(YouDidIt)
    };
    
    public static Achievement OneHundredAndTenPercent = new()
    {
        name = "110%",
        description = "Finish a night with a challenge enabled in RLAD.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/RogueLikeAtDreamings/OneHundredAndTenPercent"),
        key = AchievementKeys.RogueLikeAtDreamings,
        bitPosition = (int)AchievementEnums.RogueLikeAtDreamings.OneHundredAndTenPercent,
        condition = HasSeenLevel("RogueLikeAtDreamings")
    };

    public static Achievement Investor = new()
    {
        name = "Investor",
        description = "Purchase a permanent upgrade in RLAD.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/RogueLikeAtDreamings/Investor"),
        key = AchievementKeys.RogueLikeAtDreamings,
        bitPosition = (int)AchievementEnums.RogueLikeAtDreamings.Investor,
        condition = HasSeenLevel("RogueLikeAtDreamings")
    };

    public static Achievement Thousandaire = new()
    {
        name = "Thousandaire",
        description = "Reach $1000 in RLAD.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/RogueLikeAtDreamings/Thousandaire"),
        key = AchievementKeys.RogueLikeAtDreamings,
        bitPosition = (int)AchievementEnums.RogueLikeAtDreamings.Thousandaire,
        condition = HasSeenLevel("RogueLikeAtDreamings")
    };

    // Simon Scream

    public static Achievement Boo = new()
    {
        name = "Boo!",
        description = "Did I get you? Probably not.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        bitPosition = (int)Simon.Boo,
        condition = HasSeenLevel("SimonScream")
    };

    public static Achievement Dementia = new()
    {
        name = "Dementia",
        description = "Score 0 in Simon.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        hidden = true,
        bitPosition = (int)Simon.Dementia,
        condition = HasSeenLevel("SimonScream")
    };

    public static Achievement YouMadeAnEffort = new()
    {
        name = "You made an effort!",
        description = "Score at least 1 in Simon.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        bitPosition = (int)Simon.YouMadeAnEffort,
        condition = HasSeenLevel("SimonScream")
    };

    public static Achievement YouCanRemember = new()
    {
        name = "You can remember!",
        description = "Score at least 10 in Simon.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        bitPosition = (int)Simon.YouCanRemember,
        condition = HasSeenLevel("SimonScream")
    };

    public static Achievement IUhDontThinkYouRealiseHowLittleYouScore = new()
    {
        name = "I, uh, don't think you realise how little you score.",
        description = "Score at least 30 in Simon",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        bitPosition = (int)Simon.IUhDontThinkYouRealiseHowLittleYouScore,
        condition = HasSeenLevel("SimonScream")
    };

    public static Achievement TheWorldsMostInsubstantialMaxout = new()
    {
        name = "The World's Most Insubstantial Maxout",
        description = "Max out the score in Simon, netting you 100 points. Good job.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        hidden = true,
        bitPosition = (int)Simon.TheWorldsMostInsubstantialMaxout,
        condition = HasSeenLevel("SimonScream")
    };

    public static Achievement YourePrettyGoodAtThis = new()
    {
        name = "You're pretty good at this!",
        description = "Score 30 in one Simon.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        bitPosition = (int)Simon.YourePrettyGoodAtThis,
        condition = () => AchievementUtils.HasUnlocked(IUhDontThinkYouRealiseHowLittleYouScore)
    };

    public static Achievement SomeoneDidntGetTheMemo = new()
    {
        name = "Someone didn't get the memo.",
        description = "Score 50 in one Simon.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        bitPosition = (int)Simon.SomeoneDidntGetTheMemo,
        condition = () => AchievementUtils.HasUnlocked(YourePrettyGoodAtThis)
    };

    public static Achievement Jesus = new()
    {
        name = "Jesus.",
        description = "Max out the score in one Simon.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        bitPosition = (int)Simon.Jesus,
        condition = () => AchievementUtils.HasUnlocked(SomeoneDidntGetTheMemo)
    };

    public static Achievement Notetaker = new()
    {
        name = "Notetaker",
        description = "Click off the game in Simon.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/Simon"),
        key = AchievementKeys.Simon,
        hidden = true,
        bitPosition = (int)Simon.Notetaker,
        condition = HasSeenLevel("SimonScream")
    };

    // My Way To The Grave

    public static Achievement FullCombo = new()
    {
        name = "Full Combo!!",
        description = "Hit all notes without overtapping in MWTTG.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/FullCombo"),
        key = AchievementKeys.MyWayToTheGrave,
        hidden = false,
        bitPosition = (int)MyWayToTheGraveAchievements.FullCombo,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    public static Achievement Overcough = new()
    {
        name = "100% Overcough",
        description = "Hit every note in MWTTG, but overtap.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/Overcough"),
        key = AchievementKeys.MyWayToTheGrave,
        hidden = false,
        bitPosition = (int)MyWayToTheGraveAchievements.Overcough,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    public static Achievement AllThatWork = new()
    {
        name = "All that work.",
        description = "Gone to waste.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/AllThatWork"),
        key = AchievementKeys.MyWayToTheGrave,
        bitPosition = (int)MyWayToTheGraveAchievements.AllThatWork,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    public static Achievement SuperSinging = new()
    {
        name = "Super Singing!",
        description = "Achieve the SS rank in any song in MWTTG.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/SuperSinging"),
        key = AchievementKeys.MyWayToTheGrave,
        hidden = false,
        bitPosition = (int)MyWayToTheGraveAchievements.SuperSinging,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    public static Achievement FamiliarName = new()
    {
        name = "Familiar name.",
        description = "Achieve an S rank in any song in MWTTG.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/FamiliarName"),
        key = AchievementKeys.MyWayToTheGrave,
        hidden = false,
        bitPosition = (int)MyWayToTheGraveAchievements.FamiliarName,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    public static Achievement Amazing = new()
    {
        name = "Amazing!",
        description = "Achieve an A rank in any song in MWTTG.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/Amazing"),
        key = AchievementKeys.MyWayToTheGrave,
        hidden = false,
        bitPosition = (int)MyWayToTheGraveAchievements.Amazing,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    public static Achievement BetterThanAverage = new()
    {
        name = "Better Than Average",
        description = "Achieve a B rank in any song in MWTTG.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/BetterThanAverage"),
        key = AchievementKeys.MyWayToTheGrave,
        hidden = false,
        bitPosition = (int)MyWayToTheGraveAchievements.BetterThanAverage,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    public static Achievement Competent = new()
    {
        name = "Competent",
        description = "Achieve a C rank in any song in MWTTG.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/Competent"),
        key = AchievementKeys.MyWayToTheGrave,
        hidden = false,
        bitPosition = (int)MyWayToTheGraveAchievements.Competent,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    public static Achievement DidWell = new()
    {
        name = "Did well.",
        description = "Enough.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/DidWell"),
        key = AchievementKeys.MyWayToTheGrave,
        hidden = false,
        bitPosition = (int)MyWayToTheGraveAchievements.DidWell,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    public static Achievement Shot = new()
    {
        name = "Shot.",
        description = "It's like I'm really in the Philippines!",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MyWayToTheGrave/Shot"),
        key = AchievementKeys.MyWayToTheGrave,
        hidden = false,
        bitPosition = (int)MyWayToTheGraveAchievements.Shot,
        condition = HasSeenLevel("MyWayToTheGrave")
    };

    // "Say" That Answer!

    public static Achievement Studious = new()
    {
        name = "Studious",
        description = "Beat \"Say\" That Answer!",
        image = Resources.Load<Texture>("Achievements/Achievement Images/SayThatAnswer/Generic"),
        key = AchievementKeys.SayThatAnswer,
        hidden = false,
        bitPosition = (int)SayThatAnswer.Studious,
        condition = HasSeenLevel("SayThatAnswer")
    };

    public static Achievement Baller = new()
    {
        name = "Baller",
        description = "Beat \"Say\" That Answer! without any wrong answers.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/SayThatAnswer/Baller"),
        key = AchievementKeys.SayThatAnswer,
        hidden = false,
        bitPosition = (int)SayThatAnswer.Baller,
        condition = HasSeenLevel("SayThatAnswer")
    };
    
    public static Achievement CircularBaller = new()
    {
        name = "Circular Baller",
        description = "Beat \"Say\" That Answer! without wrong answers and using only early buzzes.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/SayThatAnswer/CircularBaller"),
        key = AchievementKeys.SayThatAnswer,
        hidden = false,
        bitPosition = (int)SayThatAnswer.FilledCircularBaller,
        condition = () => AchievementUtils.HasUnlocked(Baller)
    };
    
    public static Achievement FilledCircularBaller = new()
    {
        name = "F.C. Baller",
        description = "Beat \"Say\" That Answer! without letting your opponent score a point and only using early buzzes.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/SayThatAnswer/FilledCircularBaller"),
        key = AchievementKeys.SayThatAnswer,
        hidden = false,
        bitPosition = (int)SayThatAnswer.FilledCircularBaller,
        condition = () => AchievementUtils.HasUnlocked(CircularBaller)
    };

    public static Achievement HuhQuestionMark = new()
    {
        name = "Huh?",
        description = "I thought this would wipe out time. Forward and back.",
        image = Resources.Load<Texture>("dreaming"),
        key = AchievementKeys.SayThatAnswer,
        hidden = true,
        bitPosition = (int)SayThatAnswer.HuhQuestionMark,
    };

    public static Achievement SoClose = new()
    {
        name = "So close!",
        description = "Lose \"Say\" That Answer! on the final question",
        image = Resources.Load<Texture>("Achievements/Achievement Images/SayThatAnswer/Generic"),
        key = AchievementKeys.SayThatAnswer,
        hidden = false,
        bitPosition = (int)SayThatAnswer.SoClose,
        condition = HasSeenLevel("SayThatAnswer")
    };

    public static Achievement AwayFromKeyboard = new()
    {
        name = "A.F.K.",
        description = "Cheating bastard.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/SayThatAnswer/Generic"),
        key = AchievementKeys.SayThatAnswer,
        hidden = true,
        bitPosition = (int)SayThatAnswer.AwayFromKeyboard,
    };

    // Muffin Credits

    public static Achievement AllForNaught = new()
    {
        name = "All for Naught",
        description = "Die halfway up the credits.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MuffinCredits/AllForNaught"),
        key = AchievementKeys.MuffinCredits,
        hidden = false,
        bitPosition = (int)AchievementEnums.MuffinCredits.AllForNaught,
        condition = HasSeenLevel("MuffinCredits")
    };

    public static Achievement Hiker = new()
    {
        name = "Hiker",
        description = "Reach the top of the credits alive.",
        image = Resources.Load<Texture>("Achievements/Achievement Images/MuffinCredits/Hiker"),
        key = AchievementKeys.MuffinCredits,
        hidden = false,
        bitPosition = (int)AchievementEnums.MuffinCredits.Hiker,
        condition = HasSeenLevel("MuffinCredits")
    };

    // Dating Simulator
}