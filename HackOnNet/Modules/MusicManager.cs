using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace HackOnNet.Modules {
    public static class MusicManager {
        [DllImport("winmm.dll")]
        private static extern uint mciSendString(
            string command,
            StringBuilder returnValue,
            int returnLength,
            IntPtr winHandle);

        private static SoundPlayer player = new SoundPlayer();

        private static bool hacknetMusicDisabled = false;

        private static List<string> songsInQueue = new List<string>();

        private static Stopwatch sw = new Stopwatch();
        private static int songLength = 0;

        public static void Shuffle(List<string> songFiles) {
            Stop();
            songsInQueue = songFiles;
            songsInQueue.Shuffle();
        }

        public static void Play(string song) {
            Stop();
            songsInQueue.Clear();
            songsInQueue.Add(song);
        }

        public static void Stop() {
            hacknetMusicDisabled = true;
            sw.Stop();
            sw.Reset();
            songLength = 0;
            player.Stop();
        }

        public static void Check() {
            if(hacknetMusicDisabled) {
                Hacknet.MusicManager.stop();
            }
            if (!sw.IsRunning) {
                if(songsInQueue.Count > 0) {
                    songLength = GetSongLength(songsInQueue[0]);
                    player.Stream = File.OpenRead(songsInQueue[0]);
                    player.Play();
                    sw.Start();
                    songsInQueue.RemoveAt(0);
                }
            }
            if (sw.ElapsedMilliseconds > songLength) {
                sw.Stop();
                sw.Reset();
            }
        }

        private static int GetSongLength(string songLocation) {
            StringBuilder lengthBuf = new StringBuilder(32);

            mciSendString(string.Format($"open \"{songLocation}\" type waveaudio alias wave"), null, 0, IntPtr.Zero);
            mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
            mciSendString("close wave", null, 0, IntPtr.Zero);

            int length = 0;
            int.TryParse(lengthBuf.ToString(), out length);

            return length;
        }
    }


    static class ThreadSafeRandom {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
    static class MyExtensions {
        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
