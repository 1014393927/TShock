﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TShockAPI
{
    public class TShock : TerrariaPlugin
    {
        private uint[] tileThreshold = new uint[Main.maxPlayers];

        public static string saveDir = "./tshock/";

        public static bool killGuide = true;
        public static int invasionMultiplier = 1;
        public static int defaultMaxSpawns = 4;
        public static int defaultSpawnRate = 700;
        public static bool kickCheater = true;
        public static bool banCheater = true;
        public static int serverPort = 7777;
        public static bool enableWhitelist = false;
        public static bool infiniteInvasion = false;
        public static bool permaPvp = false;
        public static int killCount = 0;
        public static bool startedInvasion = false;

        public static string tileWhitelist = "";
        private static bool banTnt = false;
        private static bool kickTnt = false;

        public enum NPCList : int
        {
            WORLD_EATER = 0,
            EYE = 1,
            SKELETRON = 2
        }

        public override Version Version
        {
            get { return new Version(0, 1); }
        }

        public override Version APIVersion
        {
            get { return new Version(1, 1); }
        }

        public override string Name
        {
            get { return "TShock"; }
        }

        public override string Author
        {
            get { return "The TShock Team"; }
        }

        public override string Description
        {
            get { return "The administration modification of the future."; }
        }

        public TShock(Main game) : base (game)
        {
        }

        public override void Initialize()
        {
            GameHooks.OnPreInitialize += OnPreInit;
            GameHooks.OnPostInitialize += OnPostInit;
            GameHooks.OnUpdate += new Action<Microsoft.Xna.Framework.GameTime>(OnUpdate);
            GameHooks.OnLoadContent += new Action<Microsoft.Xna.Framework.Content.ContentManager>(OnLoadContent);
            ServerHooks.OnChat += new Action<int, string, HandledEventArgs>(OnChat);
            ServerHooks.OnJoin += new Action<int, AllowEventArgs>(OnJoin);
            NetHooks.OnPreGetData += GetData;
            NetHooks.OnGreetPlayer += new NetHooks.GreetPlayerD(OnGreetPlayer);
            NpcHooks.OnStrikeNpc += new NpcHooks.StrikeNpcD(NpcHooks_OnStrikeNpc);
        }

        public override void DeInitialize()
        {
            GameHooks.OnPreInitialize -= OnPreInit;
            GameHooks.OnPostInitialize -= OnPostInit;
            GameHooks.OnUpdate -= new Action<Microsoft.Xna.Framework.GameTime>(OnUpdate);
            GameHooks.OnLoadContent -= new Action<Microsoft.Xna.Framework.Content.ContentManager>(OnLoadContent);
            ServerHooks.OnChat -= new Action<int, string, HandledEventArgs>(OnChat);
            ServerHooks.OnJoin -= new Action<int, AllowEventArgs>(OnJoin);
            NetHooks.OnPreGetData -= GetData;
            NetHooks.OnGreetPlayer -= new NetHooks.GreetPlayerD(OnGreetPlayer);
            NpcHooks.OnStrikeNpc -= new NpcHooks.StrikeNpcD(NpcHooks_OnStrikeNpc);
        }

        /*
         * Hooks:
         * */

        void NpcHooks_OnStrikeNpc(NpcStrikeEventArgs e)
        {
            if (infiniteInvasion)
            {
                IncrementKills();
                if (Main.invasionSize < 10)
                {
                    Main.invasionSize = 20000000;
                }
            }
        }

        void OnPreGetData(byte id, messageBuffer msg, int idx, int length, HandledEventArgs e)
        {
            if (Main.netMode != 2) { return; }
            if (id == 0x1e && permaPvp)
            {
                e.Handled = true;
            }
        }

        void GetData(GetDataEventArgs e)
        {
            if (Main.netMode != 2) { return; }
            if (e.MsgID == 17)
            {
                byte type;
                int x = 0;
                int y = 0;
                using (var br = new BinaryReader(new MemoryStream(e.Msg.readBuffer, e.Index, e.Length)))
                {
                    type = br.ReadByte();
                    x = br.ReadInt32();
                    y = br.ReadInt32();
                }
                if (type == 0 && Main.tileSolid[Main.tile[x, y].type] && Main.player[e.Msg.whoAmI].active)
                {
                    tileThreshold[e.Msg.whoAmI]++;
                }
                return;
            }
            if (e.MsgID == 0x1e)
            {
                Main.player[e.Msg.whoAmI].hostile = true;
                NetMessage.SendData(30, -1, -1, "", e.Msg.whoAmI);
                e.Handled = true;
            }
        }

        void OnGreetPlayer(int who, HandledEventArgs e)
        {
            if (Main.netMode != 2) { return; }
            int plr = who; //legacy support
            ShowMOTD(who);
            if (Main.player[plr].statLifeMax > 400 || Main.player[plr].statManaMax > 200 || Main.player[plr].statLife > 400 || Main.player[plr].statMana > 200)
            {
                HandleCheater(plr);
            }
            if (permaPvp)
            {
                Main.player[who].hostile = true;
                NetMessage.SendData(30, -1, -1, "", who);
            }
            if (IsAdmin(who) && infiniteInvasion && !startedInvasion)
            {
                StartInvasion();
            }
            e.Handled = true;
        }

        void OnChat(int ply, string msg, HandledEventArgs handler)
        {
            if (Main.netMode != 2) { return; }
            int x = (int) Main.player[ply].position.X;
            int y = (int) Main.player[ply].position.Y;

            if (IsAdmin(ply))
            {
                if (msg.Length > 5 && msg.Substring(0, 5) == "/kick")
                {
                    string plStr = msg.Remove(0, 5).Trim();
                    if (!(FindPlayer(plStr) == -1 || plStr == ""))
                    {
                        Kick(FindPlayer(plStr), "You were kicked.");
                        Broadcast(plStr + " was kicked by " + FindPlayer(ply));
                    }
                    handler.Handled = true;
                }

                if (msg.Length > 4 && msg.Substring(0, 4) == "/ban")
                {
                    string plStr = msg.Remove(0, 4).Trim();
                    if (!(FindPlayer(plStr) == -1 || plStr == ""))
                    {
                        WriteBan(FindPlayer(plStr));
                        Kick(FindPlayer(plStr), "You were banned.");
                    }
                    handler.Handled = true;
                }

                if (msg == "/off")
                {
                    Netplay.disconnect = true;
                    handler.Handled = true;
                }

                if (msg == "/reload")
                {
                    SetupConfig();
                    handler.Handled = true;
                }

                if (msg == "/dropmeteor")
                {
                    WorldGen.spawnMeteor = false;
                    WorldGen.dropMeteor();
                    handler.Handled = true;
                }

                if (msg == "/star")
                {
                    int penis56 = 12;
                    int penis57 = Main.rand.Next(Main.maxTilesX - 50) + 100;
                    penis57 *= 0x10;
                    int penis58 = Main.rand.Next((int)(Main.maxTilesY * 0.05)) * 0x10;
                    Microsoft.Xna.Framework.Vector2 vector = new Microsoft.Xna.Framework.Vector2((float)penis57, (float)penis58);
                    float speedX = Main.rand.Next(-100, 0x65);
                    float speedY = Main.rand.Next(200) + 100;
                    float penis61 = (float)Math.Sqrt((double)((speedX * speedX) + (speedY * speedY)));
                    penis61 = ((float)penis56) / penis61;
                    speedX *= penis61;
                    speedY *= penis61;
                    Projectile.NewProjectile(vector.X, vector.Y, speedX, speedY, 12, 0x3e8, 10f, Main.myPlayer);
                    handler.Handled = true;
                }
                if (msg == "/bloodmoon")
                {
                    Broadcast(FindPlayer(ply) + " turned on blood moon.");
                    Main.bloodMoon = true;
                    Main.time = 0;
                    Main.dayTime = false;
                    //Main.UpdateT();
                    NetMessage.SendData(18, -1, -1, "", 0, 0, Main.sunModY, Main.moonModY);
                    NetMessage.syncPlayers();
                    handler.Handled = true;                        
                }
                if (msg == "/eater")
                {
                    NewNPC((int)NPCList.WORLD_EATER, x, y, ply);
                    Broadcast(FindPlayer(ply) + " has spawned an eater of worlds!");
                    handler.Handled = true;
                }
                if (msg == "/eye")
                {
                    NewNPC((int)NPCList.EYE, x, y, ply);
                    Broadcast(FindPlayer(ply) + " has spawned an eye!");
                    handler.Handled = true;
                }
                if (msg == "/skeletron")
                {
                    NewNPC((int)NPCList.SKELETRON, x, y, ply);
                    Broadcast(FindPlayer(ply) + " has spawned skeletron!");
                    handler.Handled = true;
                }
                if (msg == "/hardcore")
                {
                    for (int i = 0; i <= 2; i++)
                    {
                        NewNPC(i, x, y, ply);
                    }
                    Broadcast(FindPlayer(ply) + " has spawned all 3 bosses!");
                    handler.Handled = true;
                }
                if (msg == "/invade")
                {
                    Broadcast(Main.player[ply].name + " started an invasion.");
                    StartInvasion();
                    handler.Handled = true;
                }
                if (msg.Length > 9 && msg.Substring(0, 9) == "/password")
                {
                    string passwd = msg.Remove(0, 9).Trim();
                    Netplay.password = passwd;
                    SendMessage(ply, "Server password changed to: " + passwd);
                    handler.Handled = true;
                }
                if (msg == "/save")
                {
                    WorldGen.saveWorld();
                    SendMessage(ply, "World saved.");
                    handler.Handled = true;
                }
                if (msg == "/spawn")
                {
                    Teleport(ply, Main.player[ply].SpawnX * 16, Main.player[ply].SpawnY * 16);
                    SendMessage(ply, "Teleported to your spawnpoint.");
                    handler.Handled = true;
                }
                if (msg.Length > 3 && msg.Substring(0, 3) == "/tp")
                {
                    string player = msg.Remove(0, 3).Trim();
                    if (!(FindPlayer(player) == -1) && !(player == ""))
                    {
                        Teleport(ply, Main.player[FindPlayer(player)].position.X, Main.player[FindPlayer(player)].position.Y);
                        SendMessage(ply, "Teleported to " + player);
                        handler.Handled = true;
                    }
                }
                if (msg.Length > 7 && msg.Substring(0, 7) == "/tphere")
                {
                    string player = msg.Remove(0, 7).Trim();
                    if (!(FindPlayer(player) == -1) && !(player == ""))
                    {
                        Teleport(FindPlayer(player), Main.player[ply].position.X, Main.player[ply].position.Y);
                        SendMessage(FindPlayer(player), "You were teleported to " + FindPlayer(ply) + ".");
                        SendMessage(ply, "You brought " + player + " here.");
                        handler.Handled = true;
                    }
                }
            }
            if (msg == "/help")
            {
                SendMessage(ply, "TShock Commands:");
                SendMessage(ply, "/kick, /ban, /reload, /off, /dropmeteor, /invade");
                SendMessage(ply, "/star, /skeletron, /eye, /eater, /hardcore");
                SendMessage(ply, "Terraria commands:");
                SendMessage(ply, "/playing, /p, /me");
                handler.Handled = true;
            }
        }


        void OnJoin(int ply, AllowEventArgs handler)
        {
            if (Main.netMode != 2) { return; }
            string ip = GetRealIP((Convert.ToString(Netplay.serverSock[ply].tcpClient.Client.RemoteEndPoint)));
            if (CheckBanned(ip) || CheckCheat(ip) || CheckGreif(ip))
            {
                Kick(ply, "You are banned.");
            }
            if (!OnWhitelist(ip))
            {
                Kick(ply, "Not on whitelist.");
            }
        }

        void OnLoadContent(Microsoft.Xna.Framework.Content.ContentManager obj)
        {
            
        }

        void OnPreInit()
        {
            SetupConfig();
        }

        void OnPostInit()
        {
        }

        void OnUpdate(GameTime time)
        {
            if (Main.netMode != 2) { return; }
            for (uint i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active == false) { continue; }
                if (tileThreshold[i] >= 5)
                {
                    if (Main.player[i] != null)
                    {
                        WriteGrief((int)i);
                        Kick((int)i, "Kill tile abuse detected.");
                    }
                    tileThreshold[i] = 0;
                }
                else if (tileThreshold[i] > 0)
                {
                    tileThreshold[i] = 0;
                }
            }
        }

        /*
         * Useful stuff:
         * */

        public static void Teleport(int ply, int x, int y)
        {
            Main.player[ply].velocity = new Vector2(0, 0);
            NetMessage.SendData(0x0d, -1, -1, "", ply);
            Main.player[ply].position.X = x;
            Main.player[ply].position.Y = y - 0x2a;
            NetMessage.SendData(0x0d, -1, -1, "", ply);
        }


        public static void Teleport(int ply, float x, float y)
        {
            Main.player[ply].position.X = x;
            Main.player[ply].position.Y = y - 0x2a;
            NetMessage.SendData(0x0d, -1, -1, "", ply);
        }

        public static void StartInvasion()
        {
            Main.invasionType = 1;
            if (infiniteInvasion)
            {
                Main.invasionSize = 20000000;
            }
            else
            {
                Main.invasionSize = 100 + (invasionMultiplier * activePlayers());
            }

            Main.invasionWarn = 0;
            if (new Random().Next(2) == 0)
            {
                Main.invasionX = 0.0;
            }
            else
            {
                Main.invasionX = Main.maxTilesX;
            }
        }

        public static void IncrementKills()
        {
            killCount++;
            Random r = new Random();
            int random = r.Next(5);
            if (killCount % 100 == 0)
            {
                switch (random)
                {
                    case 0:
                        Broadcast("You call that a lot? " + killCount + " goblins killed!");
                        break;
                    case 1:
                        Broadcast("Fatality! " + killCount + " goblins killed!");
                        break;
                    case 2:
                        Broadcast("Number of 'noobs' killed to date: " + killCount);
                        break;
                    case 3:
                        Broadcast("Duke Nukem would be proud. " + killCount + " goblins killed.");
                        break;
                    case 4:
                        Broadcast("You call that a lot? " + killCount + " goblins killed!");
                        break;
                    case 5:
                        Broadcast(killCount + " copies of Call of Duty smashed.");
                        break;
                }

            }
        }

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

        public static bool OnWhitelist(string ip)
        {
            if (!enableWhitelist) { return true; }
            if (!System.IO.File.Exists(saveDir + "whitelist.txt")) { CreateFile(saveDir + "whitelist.txt"); TextWriter tw = new StreamWriter(saveDir + "whitelist.txt"); tw.WriteLine("127.0.0.1"); tw.Close(); }
            TextReader tr = new StreamReader(saveDir + "whitelist.txt");
            string whitelist = tr.ReadToEnd();
            ip = GetRealIP(ip);
            if (whitelist.Contains(ip)) { return true; } else { return false; }
        }

        public static bool CheckGreif(String ip)
        {
            ip = GetRealIP(ip);
            if (!banTnt) { return false; }
            TextReader tr = new StreamReader(saveDir + "grief.txt");
            string list = tr.ReadToEnd();
            tr.Close();

            return list.Contains(ip);
        }

        public static bool CheckCheat(String ip)
        {
            ip = GetRealIP(ip);
            if (!banCheater) { return false; }
            TextReader tr = new StreamReader(saveDir + "cheaters.txt");
            string trr = tr.ReadToEnd();
            tr.Close();
            if (trr.Contains(ip))
            {
                return true;
            }
            return false;
        }

        public static bool CheckBanned(String p)
        {
            String ip = p.Split(':')[0];
            TextReader tr = new StreamReader(saveDir + "bans.txt");
            string banlist = tr.ReadToEnd();
            tr.Close();
            banlist = banlist.Trim();
            if (banlist.Contains(ip))
                return true;
            return false;
        }

        private static void KeepTilesUpToDate()
        {
            TextReader tr = new StreamReader(saveDir + "tiles.txt");
            string file = tr.ReadToEnd();
            tr.Close();
            if (!file.Contains("0x3d"))
            {
                System.IO.File.Delete(saveDir + "tiles.txt");
                CreateFile(saveDir + "tiles.txt");
                TextWriter tw = new StreamWriter(saveDir + "tiles.txt");
                tw.Write("0x03, 0x05, 0x14, 0x25, 0x18, 0x18, 0x20, 0x1b, 0x34, 0x48, 0x33, 0x3d, 0x47, 0x49, 0x4a, 0x35, 0x3d, 0x3e, 0x45, 0x47, 0x49, 0x4a,");
                tw.Close();
            }
        }

        public static void SetupConfig()
        {
            if (!System.IO.Directory.Exists(saveDir)) { System.IO.Directory.CreateDirectory(saveDir); }
            if (!System.IO.File.Exists(saveDir + "tiles.txt"))
            {
                CreateFile(saveDir + "tiles.txt");
                TextWriter tw = new StreamWriter(saveDir + "tiles.txt");
                tw.Write("0x03, 0x05, 0x14, 0x25, 0x18, 0x18, 0x20, 0x1b, 0x34, 0x48, 0x33, 0x3d, 0x47, 0x49, 0x4a, 0x35, 0x3d, 0x3e, 0x45, 0x47, 0x49, 0x4a,");
                tw.Close();
            }
            if (!System.IO.File.Exists(saveDir + "motd.txt"))
            {
                CreateFile(saveDir + "motd.txt");
                TextWriter tw = new StreamWriter(saveDir + "motd.txt");
                tw.WriteLine("This server is running TShock. Type /help for a list of commands.");
                tw.WriteLine("%255,000,000%Current map: %map%");
                tw.WriteLine("Current players: %players%");
                tw.Close();
            }
            if (!System.IO.File.Exists(saveDir + "bans.txt")) { CreateFile(saveDir + "bans.txt"); }
            if (!System.IO.File.Exists(saveDir + "cheaters.txt")) { CreateFile(saveDir + "cheaters.txt"); }
            if (!System.IO.File.Exists(saveDir + "admins.txt")) { CreateFile(saveDir + "admins.txt"); }
            if (!System.IO.File.Exists(saveDir + "grief.txt")) { CreateFile(saveDir + "grief.txt"); }
            if (!System.IO.File.Exists(saveDir + "whitelist.txt")) { CreateFile(saveDir + "whitelist.txt"); }
            if (!System.IO.File.Exists(saveDir + "config.txt"))
            {
                CreateFile(saveDir + "config.txt");
                TextWriter tw = new StreamWriter(saveDir + "config.txt");
                tw.WriteLine("true,50,4,700,true,true,7777,false,false,false,false,false");
                tw.Close();
            }
            KeepTilesUpToDate();
            TextReader tr = new StreamReader(saveDir + "config.txt");
            string config = tr.ReadToEnd();
            config = config.Replace("\n", "");
            config = config.Replace("\r", "");
            config = config.Replace(" ", "");
            tr.Close();
            string[] configuration = config.Split(',');
            try
            {
                killGuide = Convert.ToBoolean(configuration[0]);
                invasionMultiplier = Convert.ToInt32(configuration[1]);
                defaultMaxSpawns = Convert.ToInt32(configuration[2]);
                defaultSpawnRate = Convert.ToInt32(configuration[3]);
                kickCheater = Convert.ToBoolean(configuration[4]);
                banCheater = Convert.ToBoolean(configuration[5]);
                serverPort = Convert.ToInt32(configuration[6]);
                enableWhitelist = Convert.ToBoolean(configuration[7]);
                infiniteInvasion = Convert.ToBoolean(configuration[8]);
                permaPvp = Convert.ToBoolean(configuration[9]);
                kickTnt = Convert.ToBoolean(configuration[10]);
                banTnt = Convert.ToBoolean(configuration[11]);
                if (infiniteInvasion)
                {
                    //Main.startInv();
                }
            }
            catch (Exception e)
            {
                WriteError(e.Message);
            }

            Netplay.serverPort = serverPort;
        }

        public static void Kick(int ply, string reason)
        {
            NetMessage.SendData(0x2, ply, -1, reason, 0x0, 0f, 0f, 0f);
            Netplay.serverSock[ply].kill = true;
            NetMessage.syncPlayers();
        }

        public static bool IsAdmin(string ply)
        {
            string remoteEndPoint = Convert.ToString((Netplay.serverSock[FindPlayer(ply)].tcpClient.Client.RemoteEndPoint));
            string[] remoteEndPointIP = remoteEndPoint.Split(':');
            TextReader tr = new StreamReader(saveDir + "admins.txt");
            string adminlist = tr.ReadToEnd();
            tr.Close();
            if (adminlist.Contains(remoteEndPointIP[0]))
            {
                return true;
            }
            return false;
        }

        public static bool IsAdmin(int ply)
        {
            string remoteEndPoint = Convert.ToString((Netplay.serverSock[ply].tcpClient.Client.RemoteEndPoint));
            string[] remoteEndPointIP = remoteEndPoint.Split(':');
            TextReader tr = new StreamReader(saveDir + "admins.txt");
            string adminlist = tr.ReadToEnd();
            tr.Close();
            if (adminlist.Contains(remoteEndPointIP[0]))
            {
                return true;
            }
            return false;
        }

        public static int FindPlayer(string ply)
        {
            int pl = -1;
            for (int i = 0; i < Main.player.Length; i++)
            {
                if ((ply.ToLower()) == Main.player[i].name.ToLower())
                {
                    pl = i;
                    break;
                }
            }
            return pl;
        }

        public static void HandleCheater(int ply)
        {
            string cheater = FindPlayer(ply);
            string ip = GetRealIP(Convert.ToString(Netplay.serverSock[ply].tcpClient.Client.RemoteEndPoint));

            WriteGrief(ply);
            WriteCheater(ply);
            if (!kickCheater) { return; }
            Netplay.serverSock[ply].kill = true;
            Netplay.serverSock[ply].Reset();
            NetMessage.syncPlayers();
            Broadcast(cheater + " was " + (banCheater ? "banned " : "kicked ") + "for cheating.");

        }

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

        public static void Broadcast(string msg)
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                SendMessage(i, msg);
            }
        }

        public static string GetRealIP(string mess)
        {
            return mess.Split(':')[0];
        }

        public static void SendMessage(int ply, string msg, float[] color)
        {
            NetMessage.SendData(0x19, ply, -1, msg, 8, color[0], color[1], color[2]);
        }

        public static void SendMessage(int ply, string message)
        {
            NetMessage.SendData(0x19, ply, -1, message, 8, 0f, 255f, 0f);
        }

        private static void WriteGrief(int ply)
        {
            TextWriter tw = new StreamWriter(saveDir + "grief.txt", true);
            tw.WriteLine("[" + Main.player[ply].name + "] [" + GetRealIP(Netplay.serverSock[ply].tcpClient.Client.RemoteEndPoint.ToString()) + "]");
            tw.Close();
        }

        private static void WriteError(string err)
        {
            if (System.IO.File.Exists(saveDir + "errors.txt"))
            {
                TextWriter tw = new StreamWriter(saveDir + "errors.txt", true);
                tw.WriteLine(err);
                tw.Close();
            }
            else
            {
                CreateFile(saveDir + "errors.txt");
                TextWriter tw = new StreamWriter(saveDir + "errors.txt", true);
                tw.WriteLine(err);
                tw.Close();
            }
        }

        public static void WriteBan(int ply)
        {
            string ip = GetRealIP(Convert.ToString(Netplay.serverSock[ply].tcpClient.Client.RemoteEndPoint));
            TextWriter tw = new StreamWriter(saveDir + "bans.txt", true);
            tw.WriteLine("[" + Main.player[ply].name + "] " + "[" + ip + "]");
            tw.Close();
        }

        private static void CreateFile(string file)
        {
            using (FileStream fs = File.Create(file)) { }
        }

        public static void ShowMOTD(int ply)
        {
            string foo = "";
            TextReader tr = new StreamReader(saveDir + "motd.txt");
            while ((foo = tr.ReadLine()) != null)
            {
                foo = foo.Replace("%map%", Main.worldName);
                foo = foo.Replace("%players%", GetPlayers());
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
                            pC[0] = Clamp(Convert.ToInt32(pCc[0]), 255, 0);
                            pC[1] = Clamp(Convert.ToInt32(pCc[1]), 255, 0);
                            pC[2] = Clamp(Convert.ToInt32(pCc[2]), 255, 0);
                            SendMessage(ply, foo, pC);
                            continue;
                        }
                        catch (Exception e)
                        {
                            WriteError(e.Message);
                        }
                    }
                }
                SendMessage(ply, foo);
            }
            tr.Close();
        }

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

        public static void WriteCheater(int ply)
        {
            string ip = GetRealIP(Convert.ToString(Netplay.serverSock[ply].tcpClient.Client.RemoteEndPoint));
            string cheaters = "";
            TextReader tr = new StreamReader(saveDir + "cheaters.txt");
            cheaters = tr.ReadToEnd();
            tr.Close();
            if (cheaters.Contains(Main.player[ply].name) && cheaters.Contains(ip)) { return; }
            TextWriter sw = new StreamWriter(saveDir + "cheaters.txt", true);
            sw.WriteLine("[" + Main.player[ply].name + "] " + "[" + ip + "]");
            sw.Close();
        }

        private static string GetPlayers()
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

        public static bool TileOnWhitelist(byte tile)
        {
            int _tile = (int)tile;
            TextReader tr2 = new StreamReader(saveDir + "tiles.txt");
            tileWhitelist = tr2.ReadToEnd(); tr2.Close();
            string hexValue = _tile.ToString("X");
            if (hexValue == "0")
            {
                return false;
            }
            Console.WriteLine(hexValue);
            return tileWhitelist.Contains(hexValue);
        }
    }
}