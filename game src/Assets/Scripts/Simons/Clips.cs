using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Clips : MonoBehaviour
{
    public VideoPlayer player;
    public AudioSource targetAudioSource;
    public VideoClip[] videoClips;
    public int index;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<VideoPlayer>();
        targetAudioSource = GetComponent<AudioSource>();
        player.SetTargetAudioSource(0, targetAudioSource);
    }

    // Update is called once per frame
    void Update()
    {
    }

    [ContextMenu("Play Next Clip")]
    public void PlayNextClip()
    {
        player.clip = videoClips[index++];
        player.Play();
        StartCoroutine(HideClipOnEnd());
    }

    private IEnumerator HideClipOnEnd()
    {
        yield return new WaitForSeconds(1f);
        while (player.isPlaying) yield return null;
        player.clip = null;
    }
}
