﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpServer;
using Terraria;

namespace TShockAPI {

    public class RestManager
    {
        private Rest Rest;
        public RestManager(Rest rest)
        {
            Rest = rest;
        }

        public void RegisterRestfulCommands()
        {
            Rest.Register(new RestCommand("/status", Status) {RequiesToken = false});
            Rest.Register(new RestCommand("/tokentest", TokenTest) { RequiesToken = true });

            Rest.Register(new RestCommand("/users/read/{user}/info", UserInfo) {RequiesToken = true});
            Rest.Register(new RestCommand("/users/destroy/{user}", UserDestroy) {RequiesToken = true});
            Rest.Register(new RestCommand("/users/update/{user}", UserUpdate) {RequiesToken = true});
            //RegisterExamples();
        } 

        #region RestMethods
        
        object UserUpdate(RestVerbs verbs, IParameterCollection parameters)
        {
            var returnBlock = new Dictionary<string, string>();
            var password = parameters["password"];
            var group = parameters["group"];

            if (group == null && password == null)
            {
                returnBlock.Add("status", "400");
                returnBlock.Add("error", "No parameters were passed.");
                return returnBlock;
            }

            var user = TShock.Users.GetUserByName(verbs["user"]);
            if (user == null)
            {
                returnBlock.Add("status", "400");
                returnBlock.Add("error", "The specefied user doesn't exist.");
                return returnBlock;
            }

            if (password != null)
            {
                TShock.Users.SetUserPassword(user, password);
                returnBlock.Add("password-response", "Password updated successfully.");
            }

            if (group != null)
            {
                TShock.Users.SetUserGroup(user, group);
                returnBlock.Add("group-response", "Group updated successfully.");
            }

            returnBlock.Add("status", "200");
            return returnBlock;
        }

        object UserDestroy(RestVerbs verbs, IParameterCollection parameters)
        {
            var user = TShock.Users.GetUserByName(verbs["user"]);
            if (user == null)
            {
                return new Dictionary<string, string> { { "status", "400" }, { "error", "The specified user account does't exist." } };
            }
            var returnBlock = new Dictionary<string, string>();
            try
            {
                TShock.Users.RemoveUser(user);
            } catch (Exception)
            {
                returnBlock.Add("status", "400");
                returnBlock.Add("error", "The specified user was unable to be removed.");
                return returnBlock;
            }
            returnBlock.Add("status", "200");
            returnBlock.Add("response", "User deleted successfully.");
            return returnBlock;
        }

        object UserInfo(RestVerbs verbs, IParameterCollection parameters)
        {
            var user = TShock.Users.GetUserByName(verbs["user"]);
            if (user == null)
            {
                return new Dictionary<string, string>
                           {{"status", "400"}, {"error", "The specified user account does't exist."}};
            }

            var returnBlock = new Dictionary<string, string>();
            returnBlock.Add("status", "200");
            returnBlock.Add("group", user.Group);
            returnBlock.Add("id", user.ID.ToString());
            return returnBlock;
        }

        object TokenTest(RestVerbs verbs, IParameterCollection parameters)
        {
            return new Dictionary<string, string>
                       {{"status", "200"}, {"response", "Token is valid and was passed through correctly."}};
        }

        object Status(RestVerbs verbs, IParameterCollection parameters)
        {
            var returnBlock = new Dictionary<string, string>();
            if (TShock.Config.EnableTokenEndpointAuthentication)
            {
                returnBlock.Add("status", "403");
                returnBlock.Add("error", "Server settings require a token for this API call.");
                return returnBlock;
            }
            string CurrentPlayers = "";
            int PlayerCount = 0;
            for (int i = 0; i < Main.player.Length; i++ )
            {
                if (Main.player[i].active)
                {
                    CurrentPlayers += Main.player[i].name + ", ";
                    PlayerCount++;
                }
            }
            returnBlock.Add("status", "200");
            returnBlock.Add("name", TShock.Config.ServerNickname);
            returnBlock.Add("port", Convert.ToString(TShock.Config.ServerPort));
            returnBlock.Add("playercount", Convert.ToString(PlayerCount));
            returnBlock.Add("players", CurrentPlayers);

            return returnBlock;
        }

        #endregion

        #region RestExampleMethods

        public void RegisterExamples()
        {
            Rest.Register(new RestCommand("/HelloWorld/name/{username}", UserTest) {RequiesToken = false});
            Rest.Register(new RestCommand("/wizard/{username}", Wizard) {RequiesToken = false});
        }

        //The Wizard example, for demonstrating the response convention:
        object Wizard(RestVerbs verbs, IParameterCollection parameters)
        {
            var returnBack = new Dictionary<string, string>();
            returnBack.Add("status", "200"); //Keep this in everything, 200 = ok, etc. Standard http status codes.
            returnBack.Add("error", "(If this failed, you would have a different status code and provide the error object.)"); //And only include this if the status isn't 200 or a failure
            returnBack.Add("Verified Wizard", "You're a wizard, " + verbs["username"]); //Outline any api calls and possible responses in some form of documentation somewhere
            return returnBack;
        }

        //http://127.0.0.1:8080/HelloWorld/name/{username}?type=status
        object UserTest(RestVerbs verbs, IParameterCollection parameters)
        {
            var ret = new Dictionary<string, string>();
            var type = parameters["type"];
            if (type == null)
            {
                ret.Add("Error", "Invalid Type");
                return ret;
            }
            if (type == "status")
            {
                ret.Add("Users", "Info here");
                return ret;
            }
            return null;
        }
        #endregion
    }
}
