# OpenVR2OSC

An OSC app to be used with VRChat. It sets a given parameter to true when a corresponding controller button is held down.

Discord for support and roadmap: 

[<img src="https://assets-global.website-files.com/6257adef93867e50d84d30e2/636e0b5061df29d55a92d945_full_logo_blurple_RGB.svg" alt="discord" width="80"/>](https://discord.gg/Wvnz28xeVM)

The program is currently nonfunctional. Will be updating over the next few days. (Technically if you were willing to convert every letter of each OSC endpoint by hand it's usable).

You would do that by running the program, hitting refresh mappings, then inputting your values into the config file by hand. 

For example:
{"R3":[62,59,48,55,55],"L14":[62]} would hit /avatar/parameters/SPELL with true when you hold the R3 binding (See your steamvr binding menu to determine which binding is which), and /avatar/parameters/S when L14 is held.

In the next few days I am planning to create a solution that lets you generate binding files easily. 

Over time I hope to cut down the program significantly. BOLL7708s OpenVR2Key program that this uses as a base is very optimized for what it does, and as such isn't simple to change to suit what I want it to do. But it will get there eventually.

Disclaimer:
This is my first ever C# project, it is also the first project I am seriously using github for. Expect jank in the code, expect it to be messy until I get it in a functionality state that I like. Big thanks to BOLL7608 for creating OpenVR2Key which gave me a light at the end of the tunnel when it came to actually getting steamvr input to work. 