﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using osu_database_reader.BinaryFiles;
using osu_database_reader.Components.Player;
using Xunit;

namespace UnitTestProject
{
    public class TestsBinary
    {
        public TestsBinary()
        {
            SharedCode.PreTestCheck();
        }

        [Fact]
        public void ReadOsuDb()
        {
            OsuDb db = OsuDb.Read(SharedCode.GetRelativeFile("osu!.db"));
            Debug.WriteLine("Version: " + db.OsuVersion);
            Debug.WriteLine("Amount of beatmaps: " + db.Beatmaps.Count);
            Debug.WriteLine($"Account name: \"{db.AccountName}\" (account {(db.AccountUnlocked ? "not locked" : "locked, unlocked at " + db.AccountUnlockDate)})");
            Debug.WriteLine("Account rank: " + db.AccountRank);
            for (int i = 0; i < Math.Min(10, db.Beatmaps.Count); i++) {   //print 10 at most
                var b = db.Beatmaps[i];
                Debug.WriteLine($"{b.Artist} - {b.Title} [{b.Version}]");
            }
        }

        [Fact]
        public void ReadCollectionDb()
        {
            CollectionDb db = CollectionDb.Read(SharedCode.GetRelativeFile("collection.db"));
            Debug.WriteLine("Version: " + db.OsuVersion);
            Debug.WriteLine("Amount of collections: " + db.Collections.Count);
            foreach (var c in db.Collections) {
                Debug.WriteLine($" - Collection {c.Name} has {c.BeatmapHashes.Count} item" + (c.BeatmapHashes.Count == 1 ? "" : "s"));
            }
        }

        [Fact]
        public void ReadScoresDb()
        {
            ScoresDb db = ScoresDb.Read(SharedCode.GetRelativeFile("scores.db"));
            Debug.WriteLine("Version: " + db.OsuVersion);
            Debug.WriteLine("Amount of beatmaps: " + db.Beatmaps.Count);
            Debug.WriteLine("Amount of scores: " + db.Scores.Count());

            string[] keys = db.Beatmaps.Keys.ToArray();
            for (int i = 0; i < Math.Min(25, keys.Length); i++) {   //print 25 at most
                string md5 = keys[i];
                List<Replay> replays = db.Beatmaps[md5];

                Debug.WriteLine($"Beatmap with md5 {md5} has replays:");
                for (int j = 0; j < Math.Min(5, replays.Count); j++) {      //again, 5 at most
                    var r = replays[j];
                    Debug.WriteLine($"\tReplay by {r.PlayerName}, for {r.Score} score with {r.Combo}x combo. Played at {r.TimePlayed}");
                }
            }
        }

        [Fact]
        public void ReadPresenceDb()
        {
            var db = PresenceDb.Read(SharedCode.GetRelativeFile("presence.db"));
            Debug.WriteLine("Version: " + db.OsuVersion);
            Debug.WriteLine("Amount of scores: " + db.Players.Count);

            for (int i = 0; i < Math.Min(db.Players.Count, 10); i++) {    //10 at most
                var p = db.Players[i];
                Debug.WriteLine($"Player {p.PlayerName} lives at long {p.Longitude} and lat {p.Latitude}. Some DateTime: {p.Unknown1}. Rank: {p.PlayerRank}. {p.GameMode}, #{p.GlobalRank}, id {p.PlayerId}");
            }
        }

        [Fact]
        public void ReadReplay()
        {
            //get random file
            string path = SharedCode.GetRelativeDirectory("Replays");
            string[] files = Directory.GetFiles(path);

            Skip.If(files.Length == 0, "No replays in your replay folder!");

            for (int i = 0; i < Math.Min(files.Length, 10); i++) {  //10 at most
                var r = Replay.Read(files[i]);
                Debug.WriteLine("Version: " + r.OsuVersion);
                Assert.True(r.OsuVersion >= 20070000, "osu! version is too low, is the replay object empty?");
                Debug.WriteLine("Beatmap hash: " + r.BeatmapHash);
                Assert.Equal(32, r.BeatmapHash.Length); //invalid beatmap hash
                Debug.WriteLine($"Replay by {r.PlayerName}, for {r.Score} score with {r.Combo}x combo. Played at {r.TimePlayed}");
                Debug.WriteLine($"Amount of replay frames: {r.ReplayFrames.Length}");
                for (int j = 0; j < Math.Min(r.ReplayFrames.Length, 10); j++)
                    Debug.WriteLine(r.ReplayFrames[j]);
                Debug.WriteLine("");
            }
        }
    }
}
