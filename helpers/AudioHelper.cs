using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    public static class AudioHelper
    {
        public struct AudioState
        {
            public int sourceNum;
            public string clipName;
            public float position;
            public bool isPlaying;
            public float volume;
        }

        public static List<AudioState> PauseAllLoops()
        {
            Traverse controller = Traverse.Create(AudioController.Instance);
            List<AudioSource> sources = controller.Field("loopSources").GetValue<List<AudioSource>>();

            List<AudioState> retval = new List<AudioState>();
            for (int i = 0; i < sources.Count; i++)
            {
                AudioSource source = sources[i];

                if (source == null || source.clip == null)
                {
                    retval.Add(new AudioState {
                        sourceNum = i,
                        position = 0f,
                        clipName = default(string),
                        isPlaying = false
                    });    
                    continue;
                }

                retval.Add(new AudioState {
                    sourceNum = i,
                    position = source.isPlaying ? source.time / source.clip.length : 0f,
                    clipName = source.clip.name,
                    isPlaying = source.isPlaying,
                    volume = source.volume
                });
            }

            AudioController.Instance.StopAllLoops();
            return retval;
        }

        public static void ResumeAllLoops(List<AudioState> states)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].isPlaying)
                {
                    AudioController.Instance.SetLoopAndPlay(states[i].clipName, i, true, true);
                    AudioController.Instance.SetLoopVolumeImmediate(0f, i);
                    AudioController.Instance.SetLoopTimeNormalized(states[i].position, i);
                    AudioController.Instance.FadeInLoop(1f, states[i].volume, new int[] { i });
                } else {
                    AudioController.Instance.StopLoop(i);
                }
            }
        }
    }
}