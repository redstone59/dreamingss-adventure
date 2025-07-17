using MyWayToTheGrave.SongLoading;
using UnityEngine;

namespace MyWayToTheGrave.DefaultSong
{
    public class DefaultSong : MonoBehaviour
    {
        [TextArea]
        public string normalModeString;
        [TextArea]
        public string hardModeString;

        public SongMetadata metadata;
        public SongAudio songAudio;

        public SongFolder GetDefaultSong(bool hardMode)
        {
            SongFolder song = new()
            {
                metadata = metadata,
                audio = songAudio,
                chart = hardMode ? hardModeString : normalModeString
            };

            return song;
        }
    }
}