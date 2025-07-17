using System;
using UnityEngine;

public class MW_CrowdManager : MonoBehaviour
{
    public int crowdScore;
    public int maxCrowdScore;
    public int minCrowdScore;
    public int hitReward;
    public int missPunishment;
    public int oversingPunishment;
    public float sustainMultiplier;

    private float partialSustainScore;

    public GameObject arrow;
    public Vector3 leftBound;
    public Vector3 rightBound;

    private bool hasEnteredBonus = false;
    private bool hasLeftBonus = false;
    public bool wentIntoRed = false;
    public bool wentBelowHalf = false;

    public bool Failed
    {
        get { return crowdScore <= minCrowdScore; }
    }

    public bool InBonus
    {
        get { return crowdScore >= 0.8 * (maxCrowdScore - minCrowdScore); }
    }

    public bool HasLeftBonus
    {
        get { return hasEnteredBonus && hasLeftBonus; }
    }

    public bool HasEnteredBonus
    {
        get {return hasEnteredBonus; }
    }

    void Start()
    {
        Initialise();
        MoveArrow();
    }

    void Update()
    {
        if (!hasEnteredBonus && InBonus) hasEnteredBonus = true;
        if (!hasLeftBonus && hasEnteredBonus && !InBonus) hasLeftBonus = true;

        if (!wentIntoRed && crowdScore <= 0.2 * (maxCrowdScore - minCrowdScore)) wentIntoRed = true;
        if (!wentBelowHalf && crowdScore < 0.4 * (maxCrowdScore - minCrowdScore)) wentBelowHalf = true;
    }

    public bool AdjustCrowdScore(bool hit)
    {
        crowdScore += hit ? hitReward : missPunishment;
        crowdScore = Math.Clamp(crowdScore, minCrowdScore, maxCrowdScore);
        MoveArrow();
        return Failed;
    }

    public bool Oversung()
    {
        crowdScore += oversingPunishment;
        MoveArrow();
        return Failed;
    }

    public bool SustainBonus(int ticksInSustain, float pitchAccuracy)
    {
        partialSustainScore += ticksInSustain * sustainMultiplier * pitchAccuracy;
        if (partialSustainScore >= 1)
        {
            int truncated = (int)partialSustainScore;
            partialSustainScore -= truncated;
            crowdScore += truncated;
        }
        MoveArrow();
        return Failed;
    }

    public void Initialise()
    {
        crowdScore = (maxCrowdScore + minCrowdScore) / 2;
    }

    public void MoveArrow()
    {
        arrow.transform.position = Vector3.Lerp(leftBound, rightBound, crowdScore / (float)(maxCrowdScore - minCrowdScore));
    }
}