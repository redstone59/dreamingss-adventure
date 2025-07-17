using Keys;
using TMPro;
using UnityEngine;

using System.Collections.Generic;
using Achievements;

public class EndCreditsText : MonoBehaviour
{
    public TextMeshPro tmp;

    void Start()
    {
        string[] levelSelectLines = new[]
        {
            "did you make it to the top this time?",
            "now go for a trophy",
            "personally i like my way to the grave. it was fun to make",
            "some say that the first half of this minigame \"sucks\". i tend to agree with them.",
            "i've been here the whole time",
            "clutterfunk",
            "bwomp. bwomp, bwa-bwomp. bwah. bwomp bwomp bwomp",
            "the semicolon doesnt get as much love as it should",
            "its not lupus",
            "there is actually a really rare line that can appear here. it's one in a thousand",
            "try pressing a number on the title screen",
            "i could write all manner of things in here and the chances people actually see it is quite minimal",
            "hidden in the depths of this game's code is a barely-baked plants vs zombies clone",
            "this game is foddian. discuss",
            "so what shows have you been watching lately",
            "my way to the grave has a no fail mode if you like singing but hate being shot",
            "the reason i chose that as the password was because it was in the jan Misali hangman video. i didn't know it was homestar runner",
            "genuinely i think ultimate custom night could have a roguelike mod because of RLAD",
            "rogue like at dreamings was a bitch to balance. theres like 4 different variables per animatronic.",
            "the rarity of challenges in rogue like at dreamings is stolen from nubby's number factory. some of the patches are balatro references. i am unoriginal.",
            "none of the achievements are based on random chance.",
            "you should make a speedrun.com page for this game",
            "if you're dead, the credits can be fast forwarded with space and skipped with control + space (but only when playing through level select)"
        };

        (Achievement, string)[] achievementsWithLine = new[]
        {
            (AllAchievements.BadAtMath, "theres a hidden achievement if you fail to delete your save. im not kidding."),
            (AllAchievements.AfraidQuestionMark, "have you tried appreciating the music on the title screen?"),
            (AllAchievements.SafetyHazard, "if you hit the puck in FDAGT <i>really</i> hard, you'll get an achievement"),
            (AllAchievements.TheWorldsMostInsubstantialMaxout, "the maximum score in simon scream is 100."),
            (AllAchievements.Jesus, "try getting the highest score within one simon in simon scream"),
            (AllAchievements.HuhQuestionMark, "theres a secret contestant in \"Say\" That Answer! they'll show up if you play it enough"),
            (AllAchievements.Notetaker, "i know when you try to cheat in simon."),
            (AllAchievements.AllThatWork, "have you ever had a coughing fit after finishing a karaoke song? it really sucks"),
            (AllAchievements.FilledCircularBaller, "the best way to seem smart is to answer before the question is said and never let your opponent speak"),
            (AllAchievements.Bogey, "in golf, the more strokes you get, the better your score, right?")
        };

        List<string> achievementLines = new(16);
        int unlocked = 0;
        foreach ((Achievement achievement, string line) in achievementsWithLine)
        {
            if (AchievementUtils.HasUnlocked(achievement))
                unlocked++;
            else
                achievementLines.Add(line);
        }

        // Increase chance to show non-achievement line with number of hidden achievements unlocked
        bool showAchievementLine = achievementLines.Count > 0 && Random.Range(0, achievementsWithLine.Length) > unlocked / 2;

        tmp = GetComponent<TextMeshPro>();
        // you ready for some sweet sweet ternary abuse
        string scoreLine = PlayerPrefs.GetInt(PlayerPrefKeys.DontSaveProgress, 0) != 0
                              ? showAchievementLine
                                    ? $"<color=blue>{achievementLines[Random.Range(0, achievementLines.Count)]}</color>"
                                    : Random.Range(0, 1000) == 0
                                          ? "this message is very rare"
                                          : levelSelectLines[Random.Range(0, levelSelectLines.Length)]
                              : $"your total score for this run was: {PlayerPrefs.GetInt(PlayerPrefKeys.TotalScore, 0):N0}";
        tmp.text = tmp.text.Replace("$score", scoreLine);

        string firstLine = PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0
                              ? "have you unlocked all the achievements yet?"
                              : "if this is your first time beating the game, you've just unlocked Hard Mode!";
        tmp.text = tmp.text.Replace("$firstline", firstLine);
    }
}