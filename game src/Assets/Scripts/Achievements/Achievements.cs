using System;
using UnityEngine;

public static class AchievementKeys
{
    public const string OutsideOfMinigame = "OutsideOfMinigameAchievements";
    public const string SayThatAnswer = "SayThatAnswerAchievements";
    public const string Simon = "SimonAchievements";
    public const string WorldsHardestGolf = "WorldsHardestGolfAchievements";
    public const string MuffinCredits = "MuffinCreditsAchievements";
    public const string RogueLikeAtDreamings = "RogueLikeAtDreamingsAchievements";
    public const string FuckingDreamingAndGasterTennis = "FuckingDreamingAndGasterTennisAchievements";
    public const string MyWayToTheGrave = "MyWayToTheGraveAchievements";
}

[Serializable]
public struct Achievement
{
    public string name;
    public string description;
    public string key;                              // Key for the PlayerPrefs int.
    public bool hidden;                             // Don't show the achievement if it isn't unlocked.
    public int bitPosition;                         // Each achievement is signified by one bit in an int. This is the bit position that corresponds to the achievement.
    public Texture image;
    public Func<bool> condition;                    // Condition to show the achievement on the Achievement Menu. (e.g. you can't see achievements for Simon before you've played Simon)
}

namespace AchievementEnums
{
    public enum OutsideOfMinigame
    {
        AfraidQuestionMark,                             // Stay on the title screen so long it boots you out.
        BadAtMath,                                      // Fail to delete your save data at the math question.
        Adventurer,                                     // Start the game.
        Adventured,                                     // Finish the game.
        Retired,                                        // Finish the game at Hard difficulty.
        NullOrPlaceholder = 31                          // Could use this to check for cheating, too.
    }

    public enum SayThatAnswer
    {
        Studious,                                       // Complete the game.
        Baller,                                         // Complete the game without any wrong answers.
        FilledCircularBaller,                           // Complete the game without any wrong answers and with all early buzzes.
        HuhQuestionMark,                                // Get the rare Other Dreaming (hidden).
        SoClose,                                        // Fail on the last question.
        AwayFromKeyboard                                // Get called out for taking too long.
    }

    public enum Simon
    {
        Dementia,                                       // Score 0
        YouMadeAnEffort,                                // Score >= 1
        YouCanRemember,                                 // Score >= 10
        IUhDontThinkYouRealiseHowLittleYouScore,        // Score >= 30
        TheWorldsMostInsubstantialMaxout,               // Score >= 100
        Boo,                                            // Complete the game.
        Notetaker,                                      // Click off the game.
        SomeoneDidntGetTheMemo,                         // Score >= 50 on one Simon.
        Jesus,                                          // Score >= 100 on one Simon.
        YourePrettyGoodAtThis                           // Score >= 30 in one Simon.
    }

    public enum WorldsHardestGolf
    {
        Par,                                            // Complete the game in <= 400 strokes.
        Birdie,                                         // Complete the game in <= 300 strokes.
        Eagle,                                          // Complete the game in <= 250 strokes.
        Albatross,                                      // Complete the game in <= 200 strokes.
        Putter,                                         // Die <= 25 times.
        Iron,                                           // Die <= 10 times.
        Wood,                                           // Die <= 5 times.
        Driver,                                         // Die <= 0 times.
        Bogey                                           // Score 0.
    }

    public enum MuffinCredits
    {
        Hiker,                                          // Reach the top of the credits.
        AllForNaught                                    // Die halfway up the credits.
    }

    public enum MyWayToTheGraveAchievements
    {
        FullCombo,                                      // Hit all noteheads without overtapping
        Overcough,                                      // Hit all noteheads with overtaps
        AllThatWork,                                    // Fail after the last note
        SuperSinging,                                   // SS rank
        FamiliarName,                                   // S rank
        Amazing,                                        // A rank
        BetterThanAverage,                              // B rank
        Competent,                                      // C rank
        DidWell,                                        // D rank
        Shot                                            // owie
    }

    public enum RogueLikeAtDreamings
    {
        TheFirstNight,                                  // Beat the first night
        IsNeverThatBadInAnyOfTheGames,                  // Die on the first night
        BuzzerBeater,                                   // Complete a night after a power outage
        YouDidIt,                                       // Beat the fifth night
        Investor,                                       // Purchase a permanent upgrade
        Thousandaire,                                   // Reach $1000
        Upgrader,                                       // Make a utility hit level 15.
        OneHundredAndTenPercent,                        // Complete a challenge
        Overachiever                                    // Beat the seventh night
    }

    public enum FuckingDreamingAndGasterTennis
    {
        ImagineTheNerves,                               // Reach a score of 4-4
        NervesOvercame,                                 // Win a game with score of 4-4
        NervesImagined,                                 // Lose a game with a score of 4-4
        Sweep,                                          // Win a game 5-0
        Swept,                                          // Lose a game 0-5
        Zugzwang,                                       // Have Gaster score an own goal
        BareMinimumKing,                                // Touch the puck once and still score.
        SafetyHazard                                    // Have the puck reach a velocity of 40u/s
    }
}

namespace Achievements
{
    public static class AchievementUtils
    {
        public static bool HasUnlocked(Achievement achievement)
        {
            return (PlayerPrefs.GetInt(achievement.key, 0) & (1 << achievement.bitPosition)) != 0; 
        }
    }
}