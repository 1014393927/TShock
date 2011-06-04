﻿using System;
using System.IO;
using Terraria;
using System.Collections.Generic;

namespace TShockAPI
{
    class Tools
    {
        private static List<Group> groups = new List<Group>();

        /// <summary>
        /// Provides the real IP address from a RemoteEndPoint string that contains a port and an IP
        /// </summary>
        /// <param name="mess">A string IPv4 address in IP:PORT form.</param>
        /// <returns>A string IPv4 address.</returns>
        public static string GetRealIP(string mess)
        {
            return mess.Split(':')[0];
        }

        /// <summary>
        /// Used for some places where a list of players might be used.
        /// </summary>
        /// <returns>String of players seperated by commas.</returns>
        public static string GetPlayers()
        {
            string str = "";
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    if (str == "")
                    {
                        str = str + Main.player[i].name;
                    }
                    else
                    {
                        str = str + ", " + Main.player[i].name;
                    }
                }
            }
            return str;
        }

        /// <summary>
        /// It's a clamp function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to clamp</param>
        /// <param name="max">Maximum bounds of the clamp</param>
        /// <param name="min">Minimum bounds of the clamp</param>
        /// <returns></returns>
        public static T Clamp<T>(T value, T max, T min)
    where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }

        /// <summary>
        /// Broadcasts a message to all players
        /// </summary>
        /// <param name="msg">string message</param>
        public static void Broadcast(string msg)
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                SendMessage(i, msg);
            }
            Log.Info("Broadcast: " + msg);
        }

        /// <summary>
        /// Sends a message out to a single player
        /// </summary>
        /// <param name="ply">int socket thingy for the player from the server socket</param>
        /// <param name="msg">String message</param>
        /// <param name="color">Float containing red, blue, and green color values</param>
        public static void SendMessage(int ply, string msg, float[] color)
        {
            NetMessage.SendData(0x19, ply, -1, msg, 255, color[0], color[1], color[2]);
            Log.Info("Said: " + msg + " - To: " + Tools.FindPlayer(ply));
        }

        /// <summary>
        /// Sends a green message to a player
        /// </summary>
        /// <param name="ply">int socket thingy for the player from the server socket</param>
        /// <param name="message">string message</param>
        public static void SendMessage(int ply, string message)
        {
            NetMessage.SendData(0x19, ply, -1, message, 255, 0f, 255f, 0f);
            Log.Info("Said: " + message + " - To: " + Tools.FindPlayer(ply));
        }

        /// <summary>
        /// The number of active players on the server.
        /// </summary>
        /// <returns>int playerCount</returns>
        public static int activePlayers()
        {
            int num = 0;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active)
                {
                    num++;
                }
            }
            return num;
        }

        /// <summary>
        /// Finds the name of the player of the int given
        /// </summary>
        /// <param name="ply">string player name</param>
        /// <returns>int player</returns>
        public static int FindPlayer(string ply)
        {
            /*int pl = -1;
            for (int i = 0; i < Main.player.Length; i++)
            {
                if ((ply.ToLower()) == Main.player[i].name.ToLower())
                {
                    pl = i;
                    break;
                }
            }
            return pl;*/
            List<int> found = new List<int>();
            for (int i = 0; i < Main.player.Length; i++)
                if (Main.player[i].name.ToLower().Contains(ply.ToLower()))
                    found.Add(i);
            if (found.Count == 1)
                return found[0];
            else if (found.Count > 1)
                return -2;
            else
                return -1;
        }

        /// <summary>
        /// Gets the given player's name
        /// </summary>
        /// <param name="ply">int player</param>
        /// <returns>string name</returns>
        public static string FindPlayer(int ply)
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                if (i == ply)
                {
                    return Main.player[i].name;
                }
            }
            return "null";
        }

        /// <summary>
        /// Creates an NPC
        /// </summary>
        /// <param name="type">Type is defined in the enum NPC list</param>
        /// <param name="x">X coord of the desired npc</param>
        /// <param name="y">Y coord of the desired npc</param>
        /// <param name="target">int player that the npc targets</param>
        public static void NewNPC(int type, int x, int y, int target)
        {
            switch (type)
            {
                case 0: //World Eater
                    WorldGen.shadowOrbSmashed = true;
                    WorldGen.shadowOrbCount = 3;
                    int w = NPC.NewNPC(x, y, 13, 1);
                    Main.npc[w].target = target;
                    break;
                case 1: //Eye
                    Main.time = 4861;
                    Main.dayTime = false;
                    WorldGen.spawnEye = true;
                    break;
                case 2: //Skeletron
                    int enpeecee = NPC.NewNPC(x, y, 0x23, 0);
                    Main.npc[enpeecee].netUpdate = true;
                    break;
            }
        }

        /// <summary>
        /// Kicks a player from the server.
        /// </summary>
        /// <param name="ply">int player</param>
        /// <param name="reason">string reason</param>
        public static void Kick(int ply, string reason)
        {
            NetMessage.SendData(0x2, ply, -1, reason, 0x0, 0f, 0f, 0f);
            Log.Info("Kicked " + Tools.FindPlayer(ply) + " for : " + reason);
        }

        /// <summary>
        /// Adds someone to cheaters.txt
        /// </summary>
        /// <param name="ply">int player</param>
        public static void HandleCheater(int ply)
        {
            if (!TShock.players[ply].group.HasPermission("ignorecheatdetection"))
            {
                string cheater = Tools.FindPlayer(ply);
                string ip = Tools.GetRealIP(Convert.ToString(Netplay.serverSock[ply].tcpClient.Client.RemoteEndPoint));

                FileTools.WriteCheater(ply);
                if (!ConfigurationManager.kickCheater) { return; }
                Netplay.serverSock[ply].kill = true;
                Netplay.serverSock[ply].Reset();
                NetMessage.syncPlayers();
                Tools.Broadcast(cheater + " was " + (ConfigurationManager.banCheater ? "banned " : "kicked ") + "for cheating.");
            }
        }

        /// <summary>
        /// Adds someone to griefers.txt
        /// </summary>
        /// <param name="ply">int player</param>
        public static void HandleGriefer(int ply)
        {
            if (!TShock.players[ply].group.HasPermission("ignoregriefdetection"))
            {
                string cheater = Tools.FindPlayer(ply);
                string ip = Tools.GetRealIP(Convert.ToString(Netplay.serverSock[ply].tcpClient.Client.RemoteEndPoint));

                FileTools.WriteGrief(ply);
                if (!ConfigurationManager.kickGriefer) { return; }
                Netplay.serverSock[ply].kill = true;
                Netplay.serverSock[ply].Reset();
                NetMessage.syncPlayers();
                Tools.Broadcast(cheater + " was " + (ConfigurationManager.banGriefer ? "banned " : "kicked ") + "for griefing.");
            }
        }

        /// <summary>
        /// Shows a MOTD to the player
        /// </summary>
        /// <param name="ply">int player</param>
        public static void ShowMOTD(int ply)
        {
            string foo = "";
            TextReader tr = new StreamReader(FileTools.SaveDir + "motd.txt");
            while ((foo = tr.ReadLine()) != null)
            {
                foo = foo.Replace("%map%", Main.worldName);
                foo = foo.Replace("%players%", Tools.GetPlayers());
                if (foo.Substring(0, 1) == "%" && foo.Substring(12, 1) == "%") //Look for a beginning color code.
                {
                    string possibleColor = foo.Substring(0, 13);
                    foo = foo.Remove(0, 13);
                    float[] pC = { 0, 0, 0 };
                    possibleColor = possibleColor.Replace("%", "");
                    string[] pCc = possibleColor.Split(',');
                    if (pCc.Length == 3)
                    {
                        try
                        {
                            pC[0] = Tools.Clamp(Convert.ToInt32(pCc[0]), 255, 0);
                            pC[1] = Tools.Clamp(Convert.ToInt32(pCc[1]), 255, 0);
                            pC[2] = Tools.Clamp(Convert.ToInt32(pCc[2]), 255, 0);
                            Tools.SendMessage(ply, foo, pC);
                            continue;
                        }
                        catch (Exception e)
                        {
                            FileTools.WriteError(e.Message);
                        }
                    }
                }
                Tools.SendMessage(ply, foo);
            }
            tr.Close();
        }

        public static void LoadGroups()
        {
            groups = new List<Group>();
            groups.Add(new SuperAdminGroup("superadmin"));

            StreamReader sr = new StreamReader(FileTools.SaveDir + "groups.txt");
            string data = sr.ReadToEnd();
            data = data.Replace("\r", "");
            string[] lines = data.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#"))
                {
                    continue;
                }
                string[] args = lines[i].Split(' ');
                if (args.Length < 2)
                {
                    continue;
                }
                string name = args[0];
                string parent = args[1];
                Group group = null;
                if (parent.Equals("null"))
                {
                    group = new Group(name);
                }
                else
                {
                    for (int j = 0; j < groups.Count; j++)
                    {
                        if (groups[j].GetName().Equals(parent))
                        {
                            group = new Group(name, groups[j]);
                            break;
                        }
                    }
                }
                if (group == null)
                {
                    throw new System.Exception("Something in the groups.txt is fucked up");
                }
                else
                {
                    for (int j = 2; j < args.Length; j++)
                    {
                        string permission = args[j];
                        if (permission.StartsWith("!"))
                        {
                            group.NegatePermission(args[j].Replace("!", ""));
                        }
                        else
                        {
                            group.AddPermission(args[j]);
                        }
                    }
                }
                groups.Add(group);
            }
            sr.Close();
        }

        /// <summary>
        /// Returns a Group from the name of the group
        /// </summary>
        /// <param name="ply">string groupName</param>
        public static Group GetGroup(string groupName)
        {
            //first attempt on cached groups
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].GetName().Equals(groupName))
                {
                    return groups[i];
                }
            }
            //shit, it didnt work, reload and try again
            LoadGroups();

            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].GetName().Equals(groupName))
                {
                    return groups[i];
                }
            }

            //sigh :(, ok, you fucked up the config files, congrats.
            return null;
        }

        /// <summary>
        /// Returns a Group for a ip from users.txt
        /// </summary>
        /// <param name="ply">string ip</param>
        public static Group GetGroupForIP(string ip)
        {
            ip = GetRealIP(ip);

            StreamReader sr = new StreamReader(FileTools.SaveDir + "users.txt");
            string data = sr.ReadToEnd();
            data = data.Replace("\r", "");
            string[] lines = data.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string[] args = lines[i].Split(' ');
                if (args.Length < 2)
                {
                    continue;
                }
                if (lines[i].StartsWith("#"))
                {
                    continue;
                }
                if (args[0].Equals(ip))
                {
                    return GetGroup(args[1]);
                }
            }
            sr.Close();
            return GetGroup("default");
        }

        public Tools() { }
    }
}