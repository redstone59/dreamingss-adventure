using System.Collections.Generic;
using UnityEngine;

namespace MyWayToTheGrave.Scoring
{
    public struct SongStats
    {
        public SongStats(int noteheads = 0)
        {
            noteHeadsHit = 0;
            totalNoteHeads = noteheads;
            overtaps = 0;

            summedPitchAccuracy = 0;
        }

        readonly float AveragePitchAccuracy
        {
            get { return summedPitchAccuracy / totalNoteHeads; }
        }

        public int noteHeadsHit;
        public int totalNoteHeads;
        public int overtaps;

        public float summedPitchAccuracy;
    }

    public class MW_ScoreManager : MonoBehaviour
    {
        public MW_CrowdManager crowdManager;

        public float flatHeadScore;
        public float sustainScorePerTick;
        private int score = 0;
        private float sustainScore = 0;

        public int Score
        {
            get { return score; }
        }
        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            
        }

        public void AddNoteHead(NoteHit noteHit)
        {
            float hitScore = noteHit.headHit ? flatHeadScore * noteHit.pitchAccuracy : 0;
            score += Mathf.FloorToInt(hitScore) * (crowdManager.InBonus ? 2 : 1);
        }

        public void AddSustain(NoteHit noteHit)
        {
            score += CalculateSustainScore(noteHit) * (crowdManager.InBonus ? 2 : 1);
        }

        public void AddPartialSustain(float accuracy, int ticksSustained)
        {
            sustainScore += ticksSustained * accuracy * sustainScorePerTick * (crowdManager.InBonus ? 2 : 1);
            if (sustainScore >= 1)
            {
                int truncated = (int)sustainScore;
                sustainScore -= truncated;
                score += truncated;
            }
        }

        private int CalculateSustainScore(NoteHit noteHit)
        {
            return (int)(noteHit.sustainAccuracy * noteHit.ticksSustained * sustainScorePerTick);
        }
    }
}