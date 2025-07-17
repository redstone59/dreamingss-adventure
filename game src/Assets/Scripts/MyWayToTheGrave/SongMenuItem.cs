using System;
using MyWayToTheGrave.Parsing;
using MyWayToTheGrave.SongLoading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyWayToTheGrave.Menus
{
    public class SongMenuItem : MonoBehaviour
    {
        public RawImage albumArt;
        public TextMeshProUGUI songTitle;
        public TextMeshProUGUI allOtherInformation;

        public void Display(SongFolder? potentialFolder)
        {
            if (potentialFolder == null)
            {
                albumArt.color = Color.clear;
                songTitle.text = "";
                allOtherInformation.text = "";
                return;
            }

            SongFolder folder = (SongFolder)potentialFolder;
            albumArt.texture = folder.metadata.album;
            albumArt.color = Color.white;
            songTitle.text = string.IsNullOrWhiteSpace(folder.metadata.longName)
                                 ? folder.metadata.name
                                 : folder.metadata.longName;
            allOtherInformation.text = $"by {folder.metadata.composer}";
            if (folder.metadata.coverers != null)
                allOtherInformation.text += $" (covered by {folder.metadata.coverers})";
            allOtherInformation.text += "\n";
            allOtherInformation.text += $"charted by {folder.metadata.charter}";
            allOtherInformation.text += "\n";

            float songLength = folder.audio.song.length;

            string displayedSongLength = TimeSpan.FromSeconds(Convert.ToDouble(songLength)).ToString("%m\\:ss");

            allOtherInformation.text += $"{displayedSongLength}, "; 
            allOtherInformation.text += $"{ChartParser.ParseChart(folder.chart).notes.Count:N0} notes";
            // allOtherInformation.text += "\n";
        }
    }
}