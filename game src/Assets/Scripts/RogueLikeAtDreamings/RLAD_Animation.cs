using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable
namespace RogueLikeAtDreamings
{
    public class Animation
    {
        private Image _image;
        public IEnumerable<Sprite> _frames;
        public float secondsPerFrame;
        public bool isPlaying;

        public Animation(Image image, IEnumerable<Sprite> frames, float secondsPerFrame)
        {
            _image = image;
            _frames = frames;
            this.secondsPerFrame = secondsPerFrame;
            isPlaying = false;
        }

        public IEnumerator Play(Color color, Action? atEndOfAnimation = null)
        {
            if (isPlaying) yield break;

            _image.color = color;
            isPlaying = true;
            foreach (Sprite frame in _frames)
            {
                if (!isPlaying) break;
                _image.sprite = frame;
                yield return new WaitForSeconds(secondsPerFrame);
            }
            isPlaying = false;
            _image.color = Color.clear;
            atEndOfAnimation?.Invoke();
        }

        public IEnumerator PlayLoop(Color color)
        {
            if (isPlaying) yield break;

            _image.color = color;
            isPlaying = true;
            while (isPlaying)
            {
                foreach (Sprite frame in _frames)
                {
                    if (!isPlaying) break;
                    _image.sprite = frame;
                    yield return new WaitForSeconds(secondsPerFrame);
                }
            }
            _image.color = Color.clear;
        }
    }
}
