using System.Collections.Generic;
using System.IO;
using MyWayToTheGrave.SongLoading;
using TMPro;
using UnityEngine;

namespace MyWayToTheGrave.Menus
{
    public class SongMenu : MonoBehaviour
    {
        public RhythmGame game;

        public SongMenuItem[] items;
        public List<SongFolder> allSongs;
        public int scroll = 0;
        public TextMeshProUGUI statusText;

        public SongFolder currentSong;
        public TextMeshProUGUI playText;

        public void ScanDirectory(string directory)
        {
            Scroll(0);
            string[] subdirectories = Directory.GetDirectories(directory);

            foreach (string subdirectory in subdirectories)
            {
                StartCoroutine(
                    SongLoader.GetSong(
                        subdirectory,
                        (song) => {
                            if (song is SongFolder folder)
                                UpdateAllSongs(folder);
                        },
                        statusText
                    )
                );
            }
        }

        public void UpdateAllSongs(SongFolder song)
        {
            // This will be one hell of an overhead lmao
            allSongs.Add(song);
            allSongs.Sort(
                (a, b) => a.metadata.name.Length - b.metadata.name.Length
            );
            Scroll(0);
        }

        public void Scroll(int delta)
        {
            int max = Mathf.Max(0, allSongs.Count - items.Length);
            scroll += delta;
            scroll = Mathf.Clamp(scroll, 0, max);

            for (int i = 0; i < items.Length; i++)
            {
                SongFolder? currentSong = scroll + i < allSongs.Count ? allSongs[scroll + i] : null;
                items[i].Display(currentSong);
            }
        }

        public void Select(int offset)
        {
            if (scroll + offset >= allSongs.Count) return;

            currentSong = allSongs[scroll + offset];
            playText.text = "Play custom";
            statusText.text = $"Selected \"{currentSong.metadata.name}\" by {currentSong.metadata.composer}.";
        }

        public void PlaySelectedSong()
        {
            game.OnChartLoaded(currentSong);
            gameObject.SetActive(false);
        }
    }
}