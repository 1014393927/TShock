<p align="center">
  <img src="https://tshock.co/newlogo.png" alt="TShock for Terraria"><br />
  <a href="https://travis-ci.org/Pryaxis/TShock"><img src="https://travis-ci.org/Pryaxis/TShock.svg?branch=general-devel" alt="Build Status"></a><a href="https://ci.appveyor.com/project/hakusaro/tshock"><img src="https://ci.appveyor.com/api/projects/status/chhe61q227lqdlg1?svg=true" alt="AppVeyor Build Status"></a><br />
</p>

TShock is a toolbox for Terraria servers and communities. That toolbox is jam packed with anti-cheat tools, server-side characters, groups, permissions, item bans, tons of commands, and limitless potential. It's one of a kind.

* Download: [Stable](https://github.com/TShock/TShock/releases) or [Experimental](https://travis.tshock.co/).
* Read [the documentation](https://tshock.readme.io/) to quickly get up to speed.
* Join [Discord](https://discord.gg/XUJdH58) to get help, chat, and enjoy some swell Australian company.
* Download [other plugins](https://tshock.co/xf/index.php?resources/) to supercharge your server.

----

## Table of Contents

  * [New to TShock?](#new-to-tshock)
  * [Developer's Guide](#developers-guide)

## New to TShock?

_These instructions assume Windows. If you're setting up on Linux or macOS, please refer to [the in-depth guide](https://tshock.readme.io/docs/getting-started) (and don't forget to install the *latest version* of `mono-complete` on Linux)._

1. Download [the latest stable version](https://github.com/TShock/TShock/releases) and `unzip` the folder using your favorite unzip tool. Make sure that all of the files in the zip get into one folder. This is where your server will be stored. The file structure looks like this:

      
          GeoIP.dat
          Newtonsoft.Json.dll
          OTAPI.dll
          ServerPlugins\
          |------BCrypt.Net.dll
          |------HttpServer.dll
          |------Mono.Data.Sqlite.dll
          |------MySql.Data.dll
          |------TShockAPI.dll
          TerrariaServer.exe
          sqlite3.dll
      

1. Start `TerrariaServer.exe` and TShock will boot. Answer the startup questions, and you should be ready to roll. In the background, TShock made some folders for you. We'll come back to those later.

1. Startup Terraria. Connect to a `multiplayer` server via IP and enter `localhost` if you're doing this on your local computer. If you're doing it on another computer, you need its IP address.

1. Look at the server console for the _auth code_. Type `/auth [code]` (example: `/auth 12345`), then a space, then the code you see in the console in your game chat. Instead of chatting, you'll run a command on the server. This one makes you temporary admin. All commands are prefixed with `/` or `!` (to make them silent).

1. Use the in-game command `/register [password]` (example: `/register lovely-ashes`) to create an account. This gives you owner rights on your server, which you can configure more to your liking later. Your `character name` is your `account name`.

1. Login to your newly created account with `/login [username] [password]` (example: `/login shank ashes`). You should see a login success message.

1. Turn off the backdoor with `/auth-verify` and your server is setup for initial use. TShock also created several files inside a new `tshock` folder. These files include `config.json` (our big configuration file), `sscconfig.json` (the server side characters configuration file), and `tshock.sqlite`. Don't lose your `tshock.sqlite` or you'll have to re-setup TShock all over again.

1. You can now [customize your configuration](https://tshock.readme.io/docs/config-settings), build groups, ban items, and install more plugins.

## Developer's Guide

Whether you want to contribute to TShock by sending a pull request, customize it to suit your own elfish desires, or want to build your own plugin, this is the best starting point. By the end of this, you'll be able to build TShock from source, start to finish. More than that, though, you'll know how to start on the path of becoming an expert TShock developer.

But first, you need some background.

### Background

Terraria is a C# application written on the .NET framework using the XNA game framework. TShock is a mod for Terraria's server, which is also written in C# on the .NET framework. Some might compare TShock to hMod in the Minecraft world (the precursor to Bukkit and its server, CraftBukkit). This is a good comparison to make in how the underlying build process works. When the project started, TShock was injected directly into the decompiled source code for Terraria. Unlike Minecraft, Terraria is not obfuscated, which means that many variable names and inner workings are sanely-named out of the box. Now, TShock uses advanced techniques to operate.

TShock is, first and foremost, a plugin written for the server variant of the Terraria API, an unofficial construct originally built by `bladecoding`. `TShock` has been colloquially used to refer to both the plugin as well as the server and plugin together. Similarly, the Terraria API's client version was abandoned long ago, and development of the `Server` API led to the abbreviation `TSAPI`, for `Terraria Server API`. The plugin `TShock` is executed by the [Terraria Server API](https://github.com/Pryaxis/TerrariaAPI-Server), which is in turn bound to the `Open Terraria API`, more commonly `OTAPI`. The [Open Terraria API](https://github.com/DeathCradle/Open-Terraria-API) is maintained by [DeathCradle](https://github.com/DeathCradle).

Now, the way that `TShock` runs on `TSAPI` through `OTAPI` can be summarized as the following:

1. The Open Terraria API deeply integrates with Terraria by modifying the official server's binary directly. This is done through rewriting the Terraria bytecode, the [CIL code](https://en.wikipedia.org/wiki/Common_Intermediate_Language), using a patching tool designed by DeathCradle and tools from the Mono project. For `TSAPI`, additional modifications are done to support TSAPI specific features. This done through the `TShock Mintaka Patcher`.
2. The `Terraria Server API` uses hooks provided by `OTAPI` to provide higher level hooks as well as legacy hooks for existing TSAPI applications.
3. `TShock` is executed by `TSAPI`, uses hooks provided by both `TSAPI` and `OTAPI`, and provides even higher level hooks and support tools to other `TSAPI` plugins.

With all of this in mind, the primary goal when compiling TShock is to remember that only the second and third layers are required to be interacted with. The first layer, `OTAPI`, is provided pre-compiled through NuGet. The second layer, `TSAPI`, is provided in the `TShock` repository through a git submodule.

Let's get started.

## Building

You need to get the source code. Using git, [clone this repository](https://help.github.com/articles/cloning-a-repository/). 

The next set of instructions are the technical details to setup both the Terraria Server API and TShock. More importantly, the Terraria API steps here are written under the assumption that you are building TShock primarily. Before you start, you need to **initialize the git submodules** and then **update them**. You can do this in a graphical git client, or use the following commands.

          $ git submodule init
          $ git submodule update

### On Windows

#### The Terraria Server API



#### 
