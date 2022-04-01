using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using BepInEx.Logging;
using UnityEngine.Networking;
using System;

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

        public static string FindResourceName(string key, string type, Assembly target)
        {
            string lowerKey = $".{key.ToLowerInvariant()}.{type.ToLowerInvariant()}";
            foreach (string resourceName in target.GetManifestResourceNames())
                if (resourceName.ToLowerInvariant().EndsWith(lowerKey))
                    return resourceName;

            return default(string);
        }

        private static byte[] GetResourceBytes(string key, string type, Assembly target)
        {
            string resourceName = FindResourceName(key, type, target);

            if (string.IsNullOrEmpty(resourceName))
            {
                string errorHelp = "";
                foreach (string testName in target.GetManifestResourceNames())
                    errorHelp += "," + testName;
                throw new InvalidDataException($"Could not find resource matching {key}. This is what I have: {errorHelp}");
            }

            using (Stream resourceStream = target.GetManifestResourceStream(resourceName))
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    resourceStream.CopyTo(memStream);
                    return memStream.ToArray();
                }
            }
        }

        private static string WriteWavToFile(string wavname)
        {
            byte[] wavBytes = GetResourceBytes(wavname, "wav", Assembly.GetExecutingAssembly());
            string tempPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{wavname}.wav");
            File.WriteAllBytes(tempPath, wavBytes);
            return tempPath;
        }

        public static void LoadAudioClip(string clipname, ManualLogSource log = null, string group = "Loops")
        {
            // Is this a hack?
            // Hell yes, this is a hack.

            Traverse audioController = Traverse.Create(AudioController.Instance);
            List<AudioClip> clips = audioController.Field(group).GetValue<List<AudioClip>>();

            if (clips.Find(clip => clip.name.Equals(clipname)) != null)
                return;

            string manualPath = WriteWavToFile(clipname);

            try
            {
                if (log != null)
                    log.LogInfo($"About to get audio clip at file://{manualPath}");

                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip($"file://{manualPath}", AudioType.WAV))
                {
                    request.SendWebRequest();
                    while (request.IsExecuting()); // Wait for this thing to finish

                    if (request.isHttpError)
                    {
                        throw new InvalidOperationException($"Bad request getting audio clip {request.error}");
                    }
                    else
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                        clip.name = clipname;
                        
                        clips.Add(clip);
                    }
                }
            } finally {
                if (File.Exists(manualPath))
                    File.Delete(manualPath);
            }
        }
    }
}