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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;

namespace TShockAPI.DB
{
    public class UserManager
    {        
        private IDbConnection database;

        public UserManager(IDbConnection db)
        {
            database = db;

            using (var com = database.CreateCommand())
            {
                com.CommandText =
                    "CREATE TABLE IF NOT EXISTS 'Users' ('ID' INTEGER PRIMARY KEY, 'Username' TEXT UNIQUE, 'Password' TEXT, 'UserGroup' TEXT, 'IP' TEXT);";
                com.ExecuteNonQuery();

                com.CommandText = "INSERT INTO Users (UserGroup, IP) VALUES (@group, @ip);";
                com.AddParameter("@group", "superadmin");
                com.AddParameter("@ip", "127.0.0.1");
                com.ExecuteNonQuery();
            }
        }

        public int AddUser(string ip = "" , string name = "", string password = "", string group = "default")
        {
            try
            {
                using (var com = database.CreateCommand())
                {
                    com.CommandText = "INSERT INTO Users (Username, Password, UserGroup, IP) VALUES (@name, @password, @group, @ip);";
                    com.AddParameter("@name", name.ToLower());
                    com.AddParameter("@password", Tools.HashPassword(password));

                    if(TShock.Groups.GroupExists(group))
                        com.AddParameter("@group", group);
                    else
                        //Return code 2 (Group not exist)
                        return 2;

                    com.AddParameter("@ip", ip);

                    using (var reader = com.ExecuteReader())
                    {
                        if (reader.RecordsAffected > 0)
                            //Return code 1 (User added)
                            return 1;
                        else
                            //Return code 0 (Add failed)
                            return 0;
                    }
                }                
            }
            catch (SqliteExecutionException ex)
            {
                //Return code 0 (Add failed)
                return 0;
            }
        }

        /// <summary>
        /// Fetches the hashed password and group for a given username
        /// </summary>
        /// <param name="username">string username</param>
        /// <returns>string[] {password, group}</returns>
        public string[] FetchHashedPasswordAndGroup(string username)
        {
            string[] returndata = new string[2];
            try
            {
                using (var com = database.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM Users WHERE Username=@name";
                    com.AddParameter("@name", username.ToLower());
                    using (var reader = com.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            returndata[0] = reader.Get<string>("Password");
                            returndata[1] = reader.Get<string>("UserGroup");
                            return returndata;
                        }
                    }
                }
            }
            catch (SqliteExecutionException ex)
            {                
            }
            return returndata;
        }

        /// <summary>
        /// Returns a Group for a ip from the database
        /// </summary>
        /// <param name="ply">string ip</param>
        public Group GetGroupForIP(string ip)
        {
            try
            {
                using (var com = database.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM Users WHERE IP=@ip";
                    com.AddParameter("@ip", ip);
                    using (var reader = com.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string group = reader.Get<string>("UserGroup");
                            return Tools.GetGroup(group);
                        }
                    }
                }
            }
            catch (SqliteExecutionException ex)
            {
            }
            return Tools.GetGroup("default");
        }
    }
}
