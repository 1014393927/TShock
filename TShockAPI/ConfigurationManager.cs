﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace TShockAPI
{
    /// <summary>
    /// Provides all the stupid little variables a home away from home.
    /// </summary>
    class ConfigurationManager
    {
        public static int invasionMultiplier = 1;
        public static int defaultMaxSpawns = 4;
        public static int defaultSpawnRate = 700;
        public static int serverPort = 7777;
        public static bool enableWhitelist = false;
        public static bool infiniteInvasion = false;
        public static bool permaPvp = false;
        public static int killCount = 0;
        public static bool startedInvasion = false;
        public static bool kickCheater = true;
        public static bool banCheater = true;
        public static bool banTnt = false;
        public static bool kickTnt = false;
        public enum NPCList : int
        {
            WORLD_EATER = 0,
            EYE = 1,
            SKELETRON = 2
        }

        public static void ReadJsonConfiguration()
        {
            TextReader tr = new StreamReader(FileTools.SaveDir + "config.json");
            ConfigFile cfg = JsonConvert.DeserializeObject<ConfigFile>(tr.ReadToEnd());
            tr.Close();
            
            invasionMultiplier = cfg.InvasionMultiplier;
            defaultMaxSpawns = cfg.DefaultMaximumSpawns;
            defaultSpawnRate = cfg.DefaultSpawnRate;
            serverPort = cfg.ServerPort;
            enableWhitelist = cfg.EnableWhitelist;
            infiniteInvasion = cfg.InfiniteInvasion;
            permaPvp = cfg.AlwaysPvP;
            kickCheater = cfg.KickSaveEditors;
            banCheater = cfg.BanSaveEditors;
            banTnt = cfg.BanKillTileAbusers;
            kickTnt = cfg.KickKillTileAbusers;
        }

        public static void WriteJsonConfiguration()
        {
            if (System.IO.File.Exists(FileTools.SaveDir + "config.json"))
            {
                return;
            }
            else
            {
                FileTools.CreateFile(FileTools.SaveDir + "config.json");
            }

            ConfigFile cfg = new ConfigFile();
            cfg.InvasionMultiplier = 50;
            cfg.DefaultMaximumSpawns = 4;
            cfg.DefaultSpawnRate = 700;
            cfg.ServerPort = 7777;
            cfg.EnableWhitelist = false;
            cfg.InfiniteInvasion = false;
            cfg.AlwaysPvP = false;
            cfg.KickSaveEditors = false;
            cfg.BanSaveEditors = false;
            cfg.BanKillTileAbusers = true;
            cfg.KickKillTileAbusers = true;

            string json = JsonConvert.SerializeObject(cfg, Formatting.Indented);
            TextWriter tr = new StreamWriter(FileTools.SaveDir + "config.json");
            tr.Write(json);
            tr.Close();
        }
    }
}
