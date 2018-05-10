using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace HackOnNet.Modules {
    public static class MusicManager {
        private static SoundPlayer player = new SoundPlayer();

        public static void Shuffle(List<string> songFiles) {
            Stop();
            List<string> songFilesShuffled = songFiles;
            songFilesShuffled.Shuffle();
            var t = Task.Run(() => ShuffleAsync(songFilesShuffled));
            t.Wait();
        }
        private static void ShuffleAsync(List<string> songs) {
            foreach (string song in songs) {
                player.Stream = File.OpenRead(song);
                player.PlaySync();
            }
        }

        public static void Play(string song) {
            Stop();
            var t = Task.Run(() => PlayAsync(song));
            t.Wait();
        }
        private static void PlayAsync(string song) {
            player.Stream = File.OpenRead(song);
            player.PlaySync();
        }

        public static void Stop() {
            Hacknet.MusicManager.stop();
            player.Stop();
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
