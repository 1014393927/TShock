﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using LuaInterface;

namespace TShockAPI.LuaSystem
{
	public class LuaLoader
	{
		private Lua Lua = null;
		public string LuaPath = "";
		public string LuaAutorunPath = "";
		public LuaHookBackend HookBackend = new LuaHookBackend();
		public LuaHooks HookCalls = new LuaHooks();
		public Dictionary<string, KeyValuePair<string, LuaFunction>> Hooks = new Dictionary
			<string, KeyValuePair<string, LuaFunction>>(); 
		public LuaLoader(string path)
		{
			Lua = new Lua();
			LuaPath = path;
			LuaAutorunPath = Path.Combine(LuaPath, "autorun");
			SendLuaDebugMsg("Lua 5.1 (serverside) initialized.");

			if (!string.IsNullOrEmpty(LuaPath) && !Directory.Exists(LuaPath))
			{
				Directory.CreateDirectory(LuaPath);
			}
			if (!string.IsNullOrEmpty(LuaAutorunPath) && !Directory.Exists(LuaAutorunPath))
			{
				Directory.CreateDirectory(LuaAutorunPath);
			}

			RegisterLuaFunctions();
			LoadServerAutoruns();
			HookTest();
		}
		static void test()
		{
			var loader = new LuaLoader("");
			loader.RunLuaString(@"
function hookme()
	Print('Hook test')
end

Hook(""doesntmatter"", ""hookmeee"", hookme)");

			loader.HookTest();
		}

		public void LoadServerAutoruns()
		{
			try
			{
				foreach (string s in Directory.GetFiles(LuaAutorunPath))
				{
					SendLuaDebugMsg("Loading: " + s);
					RunLuaFile(s);
				}
			}
			catch (Exception e)
			{
				SendLuaDebugMsg(e.Message);
				SendLuaDebugMsg(e.StackTrace);
			}
		}

		public void RunLuaString(string s)
		{
			try
			{
				Lua.DoString(s);
			}
			catch (LuaException e)
			{
				SendLuaDebugMsg(e.Message);
			}
		}

		public void RunLuaFile(string s)
		{
			try
			{
				Lua.DoFile(s);
			}
			catch (LuaException e)
			{
				SendLuaDebugMsg(e.Message);
			}
		}

		public void SendLuaDebugMsg(string s)
		{
			ConsoleColor previousColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Lua: " + s);
			Console.ForegroundColor = previousColor;
		}

		public void RegisterLuaFunctions()
		{
			LuaFunctions LuaFuncs = new LuaFunctions(this);
			Lua.RegisterFunction("Print", LuaFuncs, LuaFuncs.GetType().GetMethod("Print"));
			Lua.RegisterFunction("RegisterHook", LuaFuncs, LuaFuncs.GetType().GetMethod("Hook"));

		}
	}

	public class LuaFunctions
	{
		LuaLoader Parent;
		public LuaFunctions(LuaLoader parent)
		{
		   Parent = parent;
		}
		public void Print(string s)
		{
			ConsoleColor previousColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(s);
			Console.ForegroundColor = previousColor;
		}

		public void Hook(string hook, string key, LuaFunction callback)
		{
			KeyValuePair<string, LuaFunction> internalhook = new KeyValuePair<string, LuaFunction>(hook, callback);
			Parent.Hooks.Add(key, internalhook);
		}
	}

	public class LuaHookBackend
	{
		public void HookRun(string call, object parameters)
		{
			foreach (KeyValuePair<string, KeyValuePair<string, LuaFunction>> kv in TShock.LuaLoader.Hooks)
			{
				KeyValuePair<string, LuaFunction> hook = kv.Value;
				if (call == hook.Key)
				{
					LuaFunction lf = hook.Value;

					if (lf != null)
					{
						try
						{
							lf.Call(parameters);
						}
						catch (LuaException e)
						{
							TShock.LuaLoader.SendLuaDebugMsg(e.Message);
						}
					}
				}
			}
		}
	}

	public class LuaHooks
	{
		[Description("Called on debug hook test.")]
		public void OnHookTest()
		{
			object[] response = new object[2];
			response[0] = true;
			response[1] = "Hook win!";
			TShock.LuaLoader.HookBackend.HookRun("HookTest", response);
		}
	}
}
