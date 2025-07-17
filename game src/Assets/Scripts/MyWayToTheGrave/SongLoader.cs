using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace MyWayToTheGrave.SongLoading
{
    [Serializable]
    public struct SongMetadata
    {
        public string name;
        public string composer;
        public string charter;
        public Texture album;
        public Sprite background;
        public bool dreamingable_vocals;
        public bool dreamingable_warmupdeath;

        public string? longName;
        public string? coverers;
    }

    [Serializable]
    public struct SongAudio
    {
        public AudioClip song;
        public AudioClip vocalA;
        public AudioClip vocalB;
        public AudioClip warmup;
        public AudioClip death;
    }

    public struct LoadRequirements
    {
        public LoadRequirements(bool filler = true)
        {
            song = false;
            vocalA = true;
            vocalB = true;
            warmup = true;
            death = true;
            metadata = false;
            notes = false;
            album = true;
            background = true;
        }

        public bool song;
        public bool vocalA;
        public bool vocalB;
        public bool warmup;
        public bool death;
        public bool metadata;
        
        public bool notes;
        public bool album;
        public bool background;

        public readonly bool Completed()
        {
            return
            song     &&
            vocalA   &&
            vocalB   &&
            warmup   &&
            death    &&
            metadata &&
            notes    &&
            album    &&
            background;
        }

        public readonly void WhatsHoldingThisUp()
        {
            string holdUp = "Held up by: ";
            if (!song) holdUp += "song audio, ";
            if (!vocalA) holdUp += "primary vocal audio, ";
            if (!vocalB) holdUp += "secondary vocal audio, ";
            if (!warmup) holdUp += "warmup audio, ";
            if (!death) holdUp += "death audio, ";
            if (!metadata) holdUp += "metadata, ";
            if (!notes) holdUp += "notes, ";
            if (!album) holdUp += "album art, ";
            if (!background) holdUp += "background image";
            Debug.Log(holdUp);
        }
    }

    [Serializable]
    public struct SongFolder
    {
        public SongMetadata metadata;
        public SongAudio audio;
        public string chart;
    }

    public static class SongLoader
    {
        public const string FILE_STARTER = "file://";

        public static bool IsValid(string[] filepaths)
        {
            bool songPresent = false;
            bool notesPresent = false;
            bool metadataPresent = false;

            foreach (string filepath in filepaths)
            {
                string filename = Path.GetFileNameWithoutExtension(filepath).ToLower().Trim();
                songPresent |= filename == "song";
                notesPresent |= filename == "notes";
                metadataPresent |= filename == "metadata";
            }

            return songPresent && notesPresent && metadataPresent;
        }

        public static IEnumerator GetSong(string path, Action<SongFolder?> callback, TextMeshProUGUI loadingText)
        {
            SongFolder folder = new();
            string[] filepaths = Directory.GetFiles(path);

            if (!IsValid(filepaths))
            {
                callback.Invoke(null);
                yield break;
            }

            LoadRequirements loaded = new(true);
            Debug.Log($"{loaded.vocalA}, {loaded.vocalB}");
            float startTime = Time.time;
            foreach (string filepath in filepaths)
            {
                string extension = Path.GetExtension(filepath);
                if (extension == ".meta") continue;

                string filename = Path.GetFileNameWithoutExtension(filepath).ToLower().Trim();
                string pathNoExtension = Path.Combine("MyWayToTheGrave", "");

                switch (filename)
                {
                    case "song":
                        loadingText.text = "Loading song audio...";
                        yield return GetFileAudioClip(filepath, (audio) =>
                        {
                            Debug.Log($"Loaded song audio in {Time.time - startTime:N2}s.");
                            folder.audio.song = audio;
                            loaded.song = true;
                        });
                        break;
                    case "vocal_a":
                        loaded.vocalA = false;
                        loadingText.text = "Loading primary vocal audio...";
                        yield return GetFileAudioClip(filepath, (audio) =>
                        {
                            Debug.Log($"Loaded primary vocal audio in {Time.time - startTime:N2}s.");
                            folder.audio.vocalA = audio;
                            loaded.vocalA = true;
                        });
                        break;
                    case "vocal_b":
                        loaded.vocalB = false;
                        loadingText.text = "Loading secondary vocal audio...";
                        yield return GetFileAudioClip(filepath, (audio) =>
                        {
                            Debug.Log($"Loaded secondary vocal audio in {Time.time - startTime:N2}s.");
                            folder.audio.vocalB = audio;
                            loaded.vocalB = true;
                        });
                        break;
                    case "warmup":
                        loaded.warmup = false;
                        loadingText.text = "Loading warmup audio...";
                        yield return GetFileAudioClip(filepath, (audio) =>
                        {
                            Debug.Log($"Loaded warmup audio in {Time.time - startTime:N2}s.");
                            folder.audio.warmup = audio;
                            loaded.warmup = true;
                        });
                        break;
                    case "death":
                        loaded.death = false;
                        loadingText.text = "Loading death audio...";
                        yield return GetFileAudioClip(filepath, (audio) =>
                        {
                            Debug.Log($"Loaded death audio in {Time.time - startTime:N2}s.");
                            folder.audio.death = audio;
                            loaded.death = true;
                        });
                        break;
                    case "metadata":
                        folder.metadata.coverers = null;
                        folder.metadata.longName = null;
                        loadingText.text = "Loading metadata...";
                        yield return GetFileText(filepath, (text) =>
                        {
                            Debug.Log($"Loaded metadata in {Time.time - startTime:N2}s.");
                            ProcessMetadataFile(ref folder, text);
                            loaded.metadata = true;
                        });
                        break;
                    case "album":
                        loaded.album = false;
                        loadingText.text = "Loading album art...";
                        yield return GetFileTexture(filepath, (texture) =>
                        {
                            Debug.Log($"Loaded album art in {Time.time - startTime:N2}s.");
                            folder.metadata.album = texture;
                            loaded.album = true;
                        });
                        break;
                    case "background":
                        loaded.background = false;
                        loadingText.text = "Loading background image...";
                        yield return GetFileTexture(filepath, (texture) =>
                        {
                            Debug.Log($"Loaded background image in {Time.time - startTime:N2}s.");
                            folder.metadata.background = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f));
                            Debug.Log(folder.metadata.background);
                            Debug.Log($"{texture.width}, {texture.height}");
                            loaded.background = true;
                        });
                        break;
                    case "notes":
                        loadingText.text = "Loading chart...";
                        yield return GetFileText(filepath, (text) =>
                        {
                            Debug.Log($"Loaded chart notes in {Time.time - startTime:N2}s.");
                            folder.chart = text;
                            loaded.notes = true;
                        });
                        break;
                    default:
                        Debug.Log($"Invalid file in song folder {Path.GetFileName(filepath)}");
                        break;
                }
            }

            while (!loaded.Completed())
            {
                loaded.WhatsHoldingThisUp();
                yield return null;
            }

            Debug.Log($"Song folder loaded in {Time.time - startTime}s.");
            loadingText.text = "Loaded!";
            callback(folder);
        }

        private static void ProcessMetadataFile(ref SongFolder folder, string text)
        {
            string[] entries = text.Replace("\r\n", "\n").Split('\n');
            foreach (string entry in entries)
            {
                string[] splitEntry = entry.Split("=", 2);
                if (splitEntry.Length == 1)
                {
                    Debug.Log($"Incorrect metadata entry '{entry}' (no equals sign)");
                    continue;
                }

                string key = splitEntry[0].Trim().ToLower();
                string value = splitEntry[1];
                switch (key)
                {
                    case "name":
                        folder.metadata.name = value;
                        break;
                    case "long_name":
                        folder.metadata.longName = value;
                        break;
                    case "composer":
                        folder.metadata.composer = value;
                        break;
                    case "coverers":
                        folder.metadata.coverers = value;
                        break;
                    case "charter":
                        folder.metadata.charter = value;
                        break;
                    case "dm_vocala":
                        folder.metadata.dreamingable_vocals = !string.IsNullOrWhiteSpace(value);
                        break;
                    case "dm_warmupdeath":
                        folder.metadata.dreamingable_warmupdeath = !string.IsNullOrWhiteSpace(value);
                        break;
                    default:
                        Debug.Log($"Invalid metadata key {key}.");
                        continue;
                }
            }
        }

        private static IEnumerator GetFileText(string filepath, Action<string>? callback = null)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(FILE_STARTER + filepath))
            {
                yield return uwr.SendWebRequest();

                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(uwr.downloadHandler.text);
                }
                else
                {
                    Debug.Log($"Error loading {filepath}: {uwr.error}");
                }
            }
        }

        private static IEnumerator GetFileTexture(string filepath, Action<Texture2D>? callback = null)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(FILE_STARTER + filepath))
            {
                yield return uwr.SendWebRequest();

                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(DownloadHandlerTexture.GetContent(uwr));
                }
                else
                {
                    Debug.Log($"Error loading {filepath}: {uwr.error}");
                }
            }
        }

        private static IEnumerator GetFileAudioClip(string filepath, Action<AudioClip>? callback = null)
        {
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(FILE_STARTER + filepath, AudioType.UNKNOWN))
            {
                yield return uwr.SendWebRequest();

                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(DownloadHandlerAudioClip.GetContent(uwr));
                }
                else
                {
                    Debug.Log($"Error loading {filepath}: {uwr.error}");
                }
            }
        }
    }
}