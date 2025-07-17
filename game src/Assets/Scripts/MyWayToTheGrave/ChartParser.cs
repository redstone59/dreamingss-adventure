using UnityEngine;

namespace MyWayToTheGrave.Parsing
{
    public static class ChartParser
    {
        public static Song ParseChart(string fileText)
        {
            string[] lines = fileText.Replace("\r\n", "\n").Trim('\n').Split('\n');
            Song parsedSong = new()
            {
                notes = new(),
                events = new()
            };
            int currentLine = 0;

            foreach (string line in lines)
            {
                currentLine++;
                string[] splitLine = line.Split(' ');
                
                if (splitLine.Length < 2)
                {
                    Debug.Log($"Invalid line at line {currentLine} (too few arguments). ('{line}')");
                    continue;
                }
                
                if (!int.TryParse(splitLine[0], out int tick))
                {
                    Debug.Log($"Invalid tick value ({splitLine[0]}) at line {currentLine}.");
                    continue;
                }

                string eventType = splitLine[1].ToLower();
                switch (eventType)
                {
                    case "note":
                        if (!ParseNoteEvent(parsedSong, splitLine, tick, currentLine, eventType, line)) continue;
                        break;
                    case "tempo":
                        if (!ParseTempoEvent(parsedSong, splitLine, tick, currentLine, eventType, line)) continue;
                        break;
                    case "time_sig":
                        if (!ParseTimeSigEvent(parsedSong, splitLine, tick, currentLine, eventType, line)) continue;
                        break;
                    default:
                        Debug.Log($"Invalid event '{eventType}' at line {currentLine}.");
                        continue;
                }
            }

            return parsedSong;
        }

        private static bool ParseNoteEvent(Song parsedSong, string[] splitLine, int tick, int currentLine, string eventType, string line)
        {
            if (splitLine.Length < 5)
            {
                Debug.Log($"Invalid line at line {currentLine} (too few arguments for type '{eventType}'). ('{line}')");
                return false;
            }
            
            bool validLength = int.TryParse(splitLine[2], out int length);
            bool validPitch = float.TryParse(splitLine[3], out float pitch);
            char noteChar = splitLine[4][0];

            if (!validLength || !validPitch)
            {
                string invalidArgument = validLength ? "pitch" : "length";
                Debug.Log($"Invalid {invalidArgument} for event type '{eventType}' at line {currentLine}. ('{line}')");
                return false;
            }

            parsedSong.notes.Add(
                new()
                {
                    position = tick,
                    pitch = Mathf.Clamp01(pitch),
                    character = noteChar,
                    sustainLength = length == 0 ? null : length,
                    drawn = false,
                    gameObject = null
                }
            );

            return true;
        }

        // Event parse functions return `false` if they errored.

        private static bool ParseTempoEvent(Song parsedSong, string[] splitLine, int tick, int currentLine, string eventType, string line)
        {
            if (splitLine.Length < 3)
            {
                Debug.Log($"Invalid line at line {currentLine} (too few arguments for type '{eventType}'). ('{line}')");
                return false;
            }

            if (!float.TryParse(splitLine[2], out float bpm))
            {
                Debug.Log($"Invalid argument at line {currentLine} (cannot be parsed as float). ('{line}')");
                return false;
            }

            float[] values = { bpm };
            parsedSong.events.Add(
                new()
                {
                    tick = tick,
                    eventType = EventTypes.BPM,
                    values = values
                }
            );

            return true;
        }

        private static bool ParseTimeSigEvent(Song parsedSong, string[] splitLine, int tick, int currentLine, string eventType, string line)
        {
            if (splitLine.Length < 3)
            {
                Debug.Log($"Invalid line at line {currentLine} (too few arguments for type '{eventType}'). ('{line}')");
                return false;
            }

            if (!float.TryParse(splitLine[2], out float beatsPerBar))
            {
                Debug.Log($"Invalid argument at line {currentLine} (cannot be parsed as int). ('{line}')");
                return false;
            }

            float[] values = { beatsPerBar };
            parsedSong.events.Add(
                new()
                {
                    tick = tick,
                    eventType = EventTypes.TimeSignature,
                    values = values
                }
            );

            return true;
        }
    }
}