﻿/*   
TShock, a server mod for Terraria
Copyright (C) 2011 The TShock Team

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.IO;
using Terraria;

namespace TShockAPI
{
    internal class FileTools
    {
        public static readonly string ErrorsPath = Path.Combine(TShock.SavePath, "errors.txt");
        public static readonly string RulesPath = Path.Combine(TShock.SavePath, "rules.txt");
        public static readonly string MotdPath = Path.Combine(TShock.SavePath, "motd.txt");
        public static readonly string BansPath = Path.Combine(TShock.SavePath, "bans.txt");
        public static readonly string WhitelistPath = Path.Combine(TShock.SavePath, "whitelist.txt");
        public static readonly string GroupsPath = Path.Combine(TShock.SavePath, "groups.txt");
        public static readonly string UsersPath = Path.Combine(TShock.SavePath, "users.txt");
        public static readonly string ConfigPath = Path.Combine(TShock.SavePath, "config.json");

        public static void CreateFile(string file)
        {
            File.Create(file).Close();
        }

        public static void CreateIfNot(string file, string data = "")
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, data);
            }
        }

        /// <summary>
        /// Writes an error message to errors.txt
        /// </summary>
        /// <param name="err">string message</param>
        public static void WriteError(string err)
        {
            TextWriter tw = new StreamWriter(ErrorsPath, true);
            tw.WriteLine(err);
            tw.Close();
        }

        /// <summary>
        /// Sets up the configuration file for all variables, and creates any missing files.
        /// </summary>
        public static void SetupConfig()
        {
            if (!Directory.Exists(TShock.SavePath))
            {
                Directory.CreateDirectory(TShock.SavePath);
            }

            CreateIfNot(RulesPath, "Respect the admins!\nDon't use TNT!");
            CreateIfNot(MotdPath, "This server is running TShock. Type /help for a list of commands.\n%255,000,000%Current map: %map%\nCurrent players: %players%");
            CreateIfNot(BansPath);
            CreateIfNot(WhitelistPath);
            CreateIfNot(GroupsPath, Resources.groups);
            CreateIfNot(UsersPath, Resources.users);

            if (File.Exists(ConfigPath))
            {
                ConfigurationManager.ReadJsonConfiguration();
            }
            else
            {
                ConfigurationManager.WriteJsonConfiguration();
                ConfigurationManager.ReadJsonConfiguration();
            }

            Netplay.serverPort = ConfigurationManager.ServerPort;
        }

        /// <summary>
        /// Tells if a user is on the whitelist
        /// </summary>
        /// <param name="ip">string ip of the user</param>
        /// <returns>true/false</returns>
        public static bool OnWhitelist(string ip)
        {
            if (!ConfigurationManager.EnableWhitelist)
            {
                return true;
            }
            CreateIfNot(WhitelistPath, "127.0.0.1");
            TextReader tr = new StreamReader(WhitelistPath);
            string whitelist = tr.ReadToEnd();
            ip = Tools.GetRealIP(ip);
            return whitelist.Contains(ip);
        }
    }
}