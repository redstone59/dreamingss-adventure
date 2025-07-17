using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AchievementMenu : MonoBehaviour
{
    List<Achievement?> displayedAchievements;

    public AchievementBox[] boxes = new AchievementBox[6];
    public TextMeshProUGUI pageText;
    public int currentPage;

    void Awake()
    {
        displayedAchievements = PadAchievements(CreateSeperatedPages(
            GetEveryAchievement(),
            new Achievement[]
            {
                AllAchievements.Sweep,
                AllAchievements.Par,
                AllAchievements.TheFirstNight,
                AllAchievements.Boo,
                AllAchievements.FullCombo,
                AllAchievements.Studious,
                AllAchievements.AllForNaught
            }
        ));
    }

    List<Achievement> GetEveryAchievement()
    {
        AllAchievements allAchievements = new();
        var achievements = allAchievements.GetType()
                                          .GetFields()
                                          .Select(field => field.GetValue(allAchievements))
                                          .ToArray();

        List<Achievement> everyAchievement = new();

        foreach (var field in achievements)
        {
            if (field is not Achievement)
                continue;
            
            Achievement achievement = (field as Achievement?).GetValueOrDefault(AllAchievements.Boo);
            everyAchievement.Add(achievement);
        }

        return everyAchievement;
    }

    List<Achievement[]> CreateSeperatedPages(List<Achievement> everyAchievement, Achievement[] seperators)
    {
        List<Achievement[]> pages = new();
        List<Achievement> currentPage = new();
        
        foreach (Achievement achievement in everyAchievement)
        {
            if (pages.Count < seperators.Length && achievement.key == seperators[pages.Count].key)
            {
                pages.Add(currentPage.ToArray());
                currentPage.Clear();
            }
            currentPage.Add(achievement);
        }
        if (currentPage.Count != 0) pages.Add(currentPage.ToArray());

        return pages;
    }

    List<Achievement?> PadAchievements(List<Achievement[]> pages)
    {
        List<Achievement?> paddedAchievements = new();

        foreach (Achievement[] page in pages)
        {
            foreach (Achievement achievement in page)
            {
                paddedAchievements.Add(achievement);
            }
            
            int nullsToAdd = 6 - paddedAchievements.Count % 6;
            for (int i = 0; i < nullsToAdd; i++) paddedAchievements.Add(null);
        }

        return paddedAchievements;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialise()
    {
        currentPage = 0;
        ChangePage(0);
    }

    public void ChangePage(int change)
    {
        int newPage = currentPage + change;
        int maxPage = Mathf.CeilToInt(displayedAchievements.Count / (float)boxes.Length);
        if (!(0 <= newPage && newPage < maxPage))
        {
            return;
        }
        
        currentPage = newPage;
        pageText.text = $"Page {currentPage + 1}/{maxPage}";
        UpdateBoxes();
    }

    public void UpdateBoxes()
    {
        int startIndex = currentPage * boxes.Length;

        for (int i = 0; i < boxes.Length; i++)
        {
            Achievement? updateTo;

            try
            {
                updateTo = displayedAchievements[startIndex + i];
            }
            catch (ArgumentOutOfRangeException)
            {
                updateTo = null;
            }

            boxes[i].UpdateBox(updateTo);
        }
    }
}
