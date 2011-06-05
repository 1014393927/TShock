﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using System.Text.RegularExpressions;

namespace TShockAPI
{
    public class Commands
    {
        public delegate void CommandDelegate(CommandArgs args);

        public struct CommandArgs
        {
            public string Message;
            public int PlayerX;
            public int PlayerY;
            public int PlayerID;

            public CommandArgs(string message, int x, int y, int id)
            {
                Message = message; PlayerX = x; PlayerY = y; PlayerID = id;
            }
        }

        public static void InitCommands()
        {
            TShock.admincommandList.Add("kick", new CommandDelegate(Kick));
            TShock.admincommandList.Add("ban", new CommandDelegate(Ban));
            TShock.admincommandList.Add("off", new CommandDelegate(Off));
            TShock.admincommandList.Add("reload", new CommandDelegate(Reload));
            TShock.admincommandList.Add("dropmeteor", new CommandDelegate(DropMeteor));
            TShock.admincommandList.Add("star", new CommandDelegate(Star));
            TShock.admincommandList.Add("bloodmoon", new CommandDelegate(Bloodmoon));
            TShock.admincommandList.Add("eater", new CommandDelegate(Eater));
            TShock.admincommandList.Add("eye", new CommandDelegate(Eye));
            TShock.admincommandList.Add("skeletron", new CommandDelegate(Skeletron));
            TShock.admincommandList.Add("hardcore", new CommandDelegate(Hardcore));
            TShock.admincommandList.Add("invade", new CommandDelegate(Invade));
            TShock.admincommandList.Add("password", new CommandDelegate(Password));
            TShock.admincommandList.Add("save", new CommandDelegate(Save));
            TShock.admincommandList.Add("spawn", new CommandDelegate(Spawn));
            TShock.admincommandList.Add("tp", new CommandDelegate(TP));
            TShock.admincommandList.Add("tphere", new CommandDelegate(TPHere));
            TShock.admincommandList.Add("spawnmob", new CommandDelegate(SpawnMob));
            TShock.admincommandList.Add("butcher", new CommandDelegate(Butcher));
            TShock.admincommandList.Add("maxspawns", new CommandDelegate(MaxSpawns));
            TShock.admincommandList.Add("spawnrate", new CommandDelegate(SpawnRate));
            TShock.admincommandList.Add("time", new CommandDelegate(Time));
            TShock.admincommandList.Add("help", new CommandDelegate(Help));
            TShock.admincommandList.Add("slap", new CommandDelegate(Slap));
            TShock.admincommandList.Add("off-nosave", new CommandDelegate(OffNoSave));
            TShock.admincommandList.Add("protectspawn", new CommandDelegate(ProtectSpawn));
            TShock.admincommandList.Add("debug-config", new CommandDelegate(DebugConfiguration));
            TShock.admincommandList.Add("playing", new CommandDelegate(Playing));
            TShock.commandList.Add("help", new CommandDelegate(Help));
            TShock.commandList.Add("kill", new CommandDelegate(Kill));
            TShock.commandList.Add("playing", new CommandDelegate(Playing));
            if (ConfigurationManager.distributationAgent != "terraria-online")
            {
                TShock.admincommandList.Add("kill", new CommandDelegate(Kill));
                TShock.admincommandList.Add("item", new CommandDelegate(Item));
                TShock.admincommandList.Add("give", new CommandDelegate(Give));
                TShock.admincommandList.Add("heal", new CommandDelegate(Heal));

            }
        }

        #region Command Methods

        public static void Playing(CommandArgs args)
        {
            Tools.SendMessage(args.PlayerID, Tools.GetPlayers());
        }

        public static void DebugConfiguration(CommandArgs args)
        {
            int ply = args.PlayerID;
            var commands = TShock.commandList;
            if (TShock.players[ply].IsAdmin())
                commands = TShock.admincommandList;
            Tools.SendMessage(ply, "TShock Config:");
            string lineOne = "";
            lineOne += "KickCheater : " + ConfigurationManager.kickCheater + ", ";
            lineOne += "BanCheater : " + ConfigurationManager.banCheater + ", ";
            lineOne += "KickGriefer : " + ConfigurationManager.kickGriefer + ", ";
            lineOne += "BanGriefer : " + ConfigurationManager.banGriefer;
            Tools.SendMessage(ply, lineOne, new float[] { 255f, 255f, 0f });
            string lineTwo = "";
            lineTwo += "BanTnt : " + ConfigurationManager.banTnt + ", ";
            lineTwo += "KickTnt : " + ConfigurationManager.kickTnt + ", ";
            lineTwo += "BanBoom : " + ConfigurationManager.banBoom + ", ";
            lineTwo += "KickBoom : " + ConfigurationManager.kickBoom;
            Tools.SendMessage(ply, lineTwo, new float[] { 255f, 255f, 0f });
            string lineThree = "";
            lineThree += "InvMultiplier : " + ConfigurationManager.invasionMultiplier + ", ";
            lineThree += "ProtectS : " + ConfigurationManager.spawnProtect + ", ";
            lineThree += "ProtectR : " + ConfigurationManager.spawnProtectRadius + ", ";
            lineThree += "DMS : " + ConfigurationManager.defaultMaxSpawns + ", ";
            lineThree += "SpawnRate: " + ConfigurationManager.defaultSpawnRate + ", ";
            Tools.SendMessage(ply, lineThree, new float[] { 255f, 255f, 0f});
        }

        public static void Kick(CommandArgs args)
        {
            string plStr = args.Message.Remove(0, 5).Trim();
            int ply = args.PlayerID;
            int player = Tools.FindPlayer(plStr);
            if (!(player == -1 || player == -2 || plStr == ""))
            {
                if (!TShock.players[player].IsAdmin())
                {
                    Tools.Kick(player, "You were kicked.");
                    Tools.Broadcast(Tools.FindPlayer(player) + " was kicked by " + Tools.FindPlayer(ply));
                }
                else
                    Tools.SendMessage(ply, "You can't kick another admin!", new float[] { 255f, 0f, 0f });
            }
            else if (Tools.FindPlayer(plStr) == -2)
                Tools.SendMessage(ply, "More than one player matched!", new float[] { 255f, 0f, 0f });
            else
                Tools.SendMessage(ply, "Invalid player!", new float[] { 255f, 0f, 0f });
        }

        public static void Ban(CommandArgs args)
        {
            string plStr = args.Message.Remove(0, 4).Trim();
            int ply = args.PlayerID;
            int player = Tools.FindPlayer(plStr);
            if (!(player == -1 || player == -2 || plStr == ""))
            {
                if (!TShock.players[player].IsAdmin())
                {
                    FileTools.WriteBan(player);
                    Tools.Kick(player, "You were banned.");
                    Tools.Broadcast(Tools.FindPlayer(ply) + " banned " + Tools.FindPlayer(player) + "!");
                }
                else
                    Tools.SendMessage(ply, "You can't ban another admin!", new float[] { 255f, 0f, 0f });
            }
            else if (Tools.FindPlayer(plStr) == -2)
                Tools.SendMessage(ply, "More than one player matched!", new float[] { 255f, 0f, 0f });
            else
                Tools.SendMessage(ply, "Invalid player!", new float[] { 255f, 0f, 0f });
        }

        public static void Off(CommandArgs args)
        {
            WorldGen.saveWorld();
            Netplay.disconnect = true;
        }

        public static void OffNoSave(CommandArgs args)
        {
            Netplay.disconnect = true;
        }

        public static void Reload(CommandArgs args)
        {
            FileTools.SetupConfig();
            Tools.SendMessage(args.PlayerID, "Configuration reload complete. Some changes may require server restart.");
        }

        public static void DropMeteor(CommandArgs args)
        {
            WorldGen.spawnMeteor = false;
            WorldGen.dropMeteor();
        }

        public static void Star(CommandArgs args)
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
        }

        public static void Bloodmoon(CommandArgs args)
        {
            int ply = args.PlayerID;
            Tools.Broadcast(Tools.FindPlayer(ply) + " turned on blood moon.");
            Main.bloodMoon = true;
            Main.time = 0;
            Main.dayTime = false;
            NetMessage.SendData(18, -1, -1, "", 0, 0, Main.sunModY, Main.moonModY);
            NetMessage.syncPlayers();
        }

        public static void Eater(CommandArgs args)
        {
            int x = args.PlayerX;
            int y = args.PlayerY;
            int ply = args.PlayerID;
            Tools.NewNPC((int)ConfigurationManager.NPCList.WORLD_EATER, x, y, ply);
            Tools.Broadcast(Tools.FindPlayer(ply) + " has spawned an eater of worlds!");
        }

        public static void Eye(CommandArgs args)
        {
            int x = args.PlayerX;
            int y = args.PlayerY;
            int ply = args.PlayerID;
            Tools.NewNPC((int)ConfigurationManager.NPCList.EYE, x, y, ply);
            Tools.Broadcast(Tools.FindPlayer(ply) + " has spawned an eye!");
        }
        public static void Skeletron(CommandArgs args)
        {
            int x = args.PlayerX;
            int y = args.PlayerY;
            int ply = args.PlayerID;
            Tools.NewNPC((int)ConfigurationManager.NPCList.SKELETRON, x, y, ply);
            Tools.Broadcast(Tools.FindPlayer(ply) + " has spawned skeletron!");
        }

        public static void Hardcore(CommandArgs args)
        {
            int x = args.PlayerX;
            int y = args.PlayerY;
            int ply = args.PlayerID;
            for (int i = 0; i <= 2; i++)
            {
                Tools.NewNPC(i, x, y, ply);
            }
            Tools.Broadcast(Tools.FindPlayer(ply) + " has spawned all 3 bosses!");
        }

        public static void Invade(CommandArgs args)
        {
            int ply = args.PlayerID;
            if (Main.invasionSize <= 0)
            {
                Tools.Broadcast(Main.player[ply].name + " has started an invasion.");
                TShock.StartInvasion();
            }
            else
            {
                Tools.Broadcast(Main.player[ply].name + " has ended an invasion.");
                Main.invasionSize = 0;
            }
        }

        public static void Password(CommandArgs args)
        {
            int ply = args.PlayerID;
            string passwd = args.Message.Remove(0, 9).Trim();
            Netplay.password = passwd;
            Tools.SendMessage(ply, "Server password changed to: " + passwd);
        }

        public static void Save(CommandArgs args)
        {
            int ply = args.PlayerID;
            WorldGen.saveWorld();
            Tools.SendMessage(ply, "World saved.");
        }

        public static void Spawn(CommandArgs args)
        {
            int ply = args.PlayerID;
            TShock.Teleport(ply, Main.spawnTileX * 16 + 8 - Main.player[ply].width / 2, Main.spawnTileY * 16 - Main.player[ply].height);
            Tools.SendMessage(ply, "Teleported to your spawnpoint.");
        }

        public static void TP(CommandArgs args)
        {
            int ply = args.PlayerID;
            string player = args.Message.Remove(0, 3).Trim();
            if (Tools.FindPlayer(player) != -1 && Tools.FindPlayer(player) != -2 && player != "")
            {
                TShock.Teleport(ply, Main.player[Tools.FindPlayer(player)].position.X, Main.player[Tools.FindPlayer(player)].position.Y);
                Tools.SendMessage(ply, "Teleported to " + player);
            }
            else
                Tools.SendMessage(ply, "Invalid player!", new float[] { 255f, 0f, 0f });
        }

        public static void TPHere(CommandArgs args)
        {
            int ply = args.PlayerID;
            string player = args.Message.Remove(0, 7).Trim();
            if (Tools.FindPlayer(player) != -1 && Tools.FindPlayer(player) != -2 && player != "")
            {
                TShock.Teleport(Tools.FindPlayer(player), Main.player[ply].position.X, Main.player[ply].position.Y);
                Tools.SendMessage(Tools.FindPlayer(player), "You were teleported to " + Tools.FindPlayer(ply) + ".");
                Tools.SendMessage(ply, "You brought " + player + " here.");
            }
            else
                Tools.SendMessage(ply, "Invalid player!", new float[] { 255f, 0f, 0f });
        }

        public static void SpawnMob(CommandArgs args)
        {
            int x = args.PlayerX;
            int y = args.PlayerY;
            var msgargs = Regex.Split(args.Message, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            if (msgargs.Length >= 2 && msgargs.Length <= 3)
            {
                for (int i = 1; i < msgargs.Length; i++)
                    msgargs[i] = ((msgargs[i].TrimEnd('"')).TrimStart('"'));
                string inputtype = "";
                int amount = 1;
                int npcid = -1;
                int type = -1;
                inputtype = msgargs[1];
                if (msgargs.Length == 3)
                    int.TryParse(msgargs[2], out amount);

                if (!int.TryParse(inputtype, out type))
                    type = TShock.GetNPCID(inputtype);
                if (type >= 1 && type <= 45)
                {
                    for (int i = 0; i < amount; i++)
                        npcid = NPC.NewNPC(x, y, type, 0);
                    Tools.Broadcast(string.Format("{0} was spawned {1} time(s).", Main.npc[npcid].name, amount));;
                }
            }
            else
                Tools.SendMessage(args.PlayerID, "Invalid syntax! Proper syntax: /spawnmob <mob name/id> [amount]", new float[] { 255f, 0f, 0f });
        }

        public static void Item(CommandArgs args)
        {
            var msgargs = Regex.Split(args.Message, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")[1];
            int ply = args.PlayerID;
            bool flag = false;
            if (msgargs.Length >= 2)
            {
                msgargs = ((msgargs.TrimEnd('"')).TrimStart('"'));
                int type = 0;
                if (!int.TryParse(msgargs, out type))
                    type = TShock.GetItemID(msgargs);
                if (type >= 1 && type <= 238)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        if (!Main.player[ply].inventory[i].active)
                        {
                            //Main.player[ply].inventory[i].SetDefaults(type);
                            //Main.player[ply].inventory[i].stack = Main.player[ply].inventory[i].maxStack;
                            int id = Terraria.Item.NewItem(0, 0, 0, 0, type, 1, true);
                            Main.item[id].position.X = (float)args.PlayerX;
                            Main.item[id].position.Y = (float)args.PlayerY;
                            Main.item[id].stack = Main.item[id].maxStack;
                            //TShock.SendDataAll(21, -1, "", id);
                            NetMessage.SendData(21, -1, -1, "", id, 0f, 0f, 0f);
                            Tools.SendMessage(ply, "Got some " + Main.item[id].name + ".");
                            //TShock.UpdateInventories();
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                        Tools.SendMessage(args.PlayerID, "You don't have free slots!", new float[] { 255f, 0f, 0f });
                }
                else
                    Tools.SendMessage(args.PlayerID, "Invalid item type!", new float[] { 255f, 0f, 0f });
            }
            else
                Tools.SendMessage(args.PlayerID, "Invalid syntax! Proper syntax: /item <item name/id>", new float[] { 255f, 0f, 0f });
        }

        public static void Give(CommandArgs args)
        {
            var msgargs = Regex.Split(args.Message, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            int ply = args.PlayerID;
            bool flag = false;
            if (msgargs.Length == 3)
            {
                for (int i = 1; i < msgargs.Length; i++)
                    msgargs[i] = ((msgargs[i].TrimEnd('"')).TrimStart('"'));
                int type = 0;
                int player = -1;
                if (!int.TryParse(msgargs[1], out type))
                    type = TShock.GetItemID(msgargs[1]);
                if (type >= 1 && type <= 238)
                {
                    player = Tools.FindPlayer(msgargs[2]);
                    if (player != -1)
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            if (!Main.player[player].inventory[i].active)
                            {
                                //Main.player[player].inventory[i].SetDefaults(type);
                                //Main.player[player].inventory[i].stack = Main.player[player].inventory[i].maxStack;
                                int id = Terraria.Item.NewItem(0, 0, 0, 0, type, 1, true);
                                Main.item[id].position.X = Main.player[player].position.X;
                                Main.item[id].position.Y = Main.player[player].position.Y;
                                Main.item[id].stack = Main.item[id].maxStack;
                                //TShock.SendDataAll(21, -1, "", id);
                                NetMessage.SendData(21, -1, -1, "", id, 0f, 0f, 0f);
                                Tools.SendMessage(ply, string.Format("Gave {0} some {1}.", msgargs[2], Main.item[id].name));
                                Tools.SendMessage(player, string.Format("{0} gave you some {1}.", Tools.FindPlayer(ply), Main.item[id].name));
                                //TShock.UpdateInventories();
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                            Tools.SendMessage(args.PlayerID, "Player does not have free slots!", new float[] { 255f, 0f, 0f });
                    }
                    else
                        Tools.SendMessage(args.PlayerID, "Invalid player!", new float[] { 255f, 0f, 0f });
                }
                else
                    Tools.SendMessage(args.PlayerID, "Invalid item type!", new float[] { 255f, 0f, 0f });
            }
            else
                Tools.SendMessage(args.PlayerID, "Invalid syntax! Proper syntax: /give <item type/id> <player>", new float[] { 255f, 0f, 0f });
        }

        public static void Heal(CommandArgs args)
        {
            int ply = args.PlayerID;
            int x = args.PlayerX;
            int y = args.PlayerY;
            var msgargs = Regex.Split(args.Message, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            int player = ply;
            if (msgargs.Length == 2)
                player = Tools.FindPlayer((msgargs[1].TrimEnd('"')).TrimStart('"'));
            if (player != ply && player >= 0)
            {
                Tools.SendMessage(ply, string.Format("You just healed {0}", (msgargs[1].TrimEnd('"')).TrimStart('"')));
                Tools.SendMessage(player, string.Format("{0} just healed you!", Tools.FindPlayer(ply)));
                x = (int)Main.player[player].position.X;
                y = (int)Main.player[player].position.Y;
            }
            else
                Tools.SendMessage(ply, "You just got healed!");
            for (int i = 0; i < 20; i++)
            {
                int itemid = Terraria.Item.NewItem(1, 1, 1, 1, 58);
                Main.item[itemid].position.X = (float)x;
                Main.item[itemid].position.Y = (float)y;
                NetMessage.SendData(21, -1, -1, "", itemid, 0f, 0f, 0f);
            }
        }

        public static void Butcher(CommandArgs args)
        {
            int killcount = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].townNPC || !Main.npc[i].active)
                    continue;
                else
                {
                    Main.npc[i].StrikeNPC(99999, 90f, 1);
                    NetMessage.SendData(28, -1, -1, "", i, (float)99999, 90f, 1);
                    killcount++;
                }
            }
            Tools.Broadcast("Killed " + killcount.ToString() + " NPCs.");
        }

        public static void MaxSpawns(CommandArgs args)
        {
            int ply = args.PlayerID;
            int amount = Convert.ToInt32(args.Message.Remove(0, 10));
            int.TryParse(args.Message.Remove(0, 10), out amount);
            NPC.defaultSpawnRate = amount;
            Tools.Broadcast(Tools.FindPlayer(ply) + " changed the maximum spawns to: " + amount);
        }

        public static void SpawnRate(CommandArgs args)
        {
            int ply = args.PlayerID;
            int amount = Convert.ToInt32(args.Message.Remove(0, 10));
            int.TryParse(args.Message.Remove(0, 10), out amount);
            NPC.defaultSpawnRate = amount;
            Tools.Broadcast(Tools.FindPlayer(ply) + " changed the spawn rate to: " + amount);
        }

        public static void Help(CommandArgs args)
        {
            int ply = args.PlayerID;
            var commands = TShock.commandList;
            if (TShock.players[ply].IsAdmin())
                commands = TShock.admincommandList;
            Tools.SendMessage(ply, "TShock Commands:");
            int h = 1;
            int i = 0;
            string tempstring = "";
            int page = 1;
            if (args.Message.Split(' ').Length == 2)
                int.TryParse(args.Message.Split(' ')[1], out page);
            if (commands.Count > (15 * (page - 1)))
            {
                for (int j = (15 * (page - 1)); j < commands.Count; j++)
                {
                    if (i == 3) break;
                    if (j == commands.Count - 1)
                    {
                        tempstring += "/" + commands.Keys.ElementAt(j) + ", ";
                        Tools.SendMessage(ply, tempstring.TrimEnd(new char[] { ' ', ',' }), new float[] { 255f, 255f, 0f });
                    }
                    if ((h - 1) % 5 == 0 && (h - 1) != 0)
                    {
                        Tools.SendMessage(ply, tempstring.TrimEnd(new char[] { ' ', ',' }), new float[] { 255f, 255f, 0f });
                        tempstring = "/" + commands.Keys.ElementAt(j) + ", ";
                        i++;
                        h++;
                    }
                    else
                    {
                        tempstring += "/" + commands.Keys.ElementAt(j) + ", ";
                        h++;
                    }
                }
            }
            if (commands.Count > (15 * page))
            { Tools.SendMessage(ply, "Type /help " + (page + 1).ToString() + " for more commands.", new float[] { 255f, 0f, 255f }); }
            Tools.SendMessage(ply, "Terraria commands:");
            Tools.SendMessage(ply, "/playing, /p, /me", new float[] { 255f, 255f, 0f });
        }

        public static void Time(CommandArgs args)
        {
            var arg = args.Message.Split(' ');
            if (arg.Length == 2)
            {
                if (arg[1] == "day")
                {
                    Main.time = 0;
                    Main.dayTime = true;
                    NetMessage.SendData(18, -1, -1, "", 0, 0, Main.sunModY, Main.moonModY);
                    NetMessage.syncPlayers();
                    Tools.Broadcast(Tools.FindPlayer(args.PlayerID) + " set time to day.");
                }
                else if (arg[1] == "night")
                {
                    Main.time = 0;
                    Main.dayTime = false;
                    NetMessage.SendData(18, -1, -1, "", 0, 0, Main.sunModY, Main.moonModY);
                    NetMessage.syncPlayers();
                    Tools.Broadcast(Tools.FindPlayer(args.PlayerID) + " set time to night.");
                }
                else
                    Tools.SendMessage(args.PlayerID, "Invalid syntax! Proper syntax: /time <day/night>", new float[] { 255f, 0f, 0f });
            }
            else
                Tools.SendMessage(args.PlayerID, "Invalid syntax! Proper syntax: /time <day/night>", new float[] { 255f, 0f, 0f });
        }

        public static void Kill(CommandArgs args)
        {
            bool isadmin = TShock.players[args.PlayerID].IsAdmin(); ;
            var msgargs = Regex.Split(args.Message, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            if (msgargs.Length == 2 && isadmin)
            {
                int player = -1;
                player = Tools.FindPlayer((msgargs[1].TrimEnd('"')).TrimStart('"'));
                Tools.SendMessage(args.PlayerID, "You just killed " + Tools.FindPlayer(player) + "!");
                Tools.SendMessage(player, Tools.FindPlayer(args.PlayerID) + " just killed you!");
                TShock.KillMe(player);
            }
            else
            {
                Tools.SendMessage(args.PlayerID, "You just suicided.");
                TShock.KillMe(args.PlayerID);
            }
        }

        public static void Slap(CommandArgs args)
        {
            var msgargs = Regex.Split(args.Message, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            for (int i = 0; i < msgargs.Length; i++)
                msgargs[i] = (msgargs[i].TrimStart('"')).TrimEnd('"');
            if (msgargs.Length == 1)
                Tools.SendMessage(args.PlayerID, "Invalid syntax! Proper syntax: /slap <player> [dmg]", new float[] { 255f, 0f, 0f });
            else if (msgargs.Length == 2)
            {
                int player = Tools.FindPlayer(msgargs[1]);
                if (player == -1)
                    Tools.SendMessage(args.PlayerID, "Invalid player!", new float[] { 255f, 0f, 0f });
                else
                {
                    TShock.SendDataAll(26, -1, "", player, (float)((new Random()).Next(1, 20)), (float)5, (float)0);
                    Tools.Broadcast(Tools.FindPlayer(args.PlayerID) + " slapped " + Tools.FindPlayer(player) + " for 5 damage.");
                }
            }
            else if (msgargs.Length == 3)
            {
                int player = Tools.FindPlayer(msgargs[1]);
                int damage = 5;
                int.TryParse(msgargs[2], out damage);
                if (player == -1)
                    Tools.SendMessage(args.PlayerID, "Invalid player!", new float[] { 255f, 0f, 0f });
                else
                {
                    TShock.SendDataAll(26, -1, "", player, (float)((new Random()).Next(-1, 1)), (float)damage, (float)0);
                    Tools.Broadcast(Tools.FindPlayer(args.PlayerID) + " slapped " + Tools.FindPlayer(player) + " for " + damage.ToString() + " damage.");
                }
            }
            else
                Tools.SendMessage(args.PlayerID, "Invalid syntax! Proper syntax: /slap <player> [dmg]", new float[] { 255f, 0f, 0f });
        }

        public static void ProtectSpawn(CommandArgs args)
        {
            ConfigurationManager.spawnProtect = (ConfigurationManager.spawnProtect == false);
            Tools.SendMessage(args.PlayerID, "Spawn is now " + (ConfigurationManager.spawnProtect ? "protected" : "open") + ".");
        }
        #endregion
    }
}
