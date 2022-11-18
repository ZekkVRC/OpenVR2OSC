# OpenVR2OSC

Currently nonfunctional. Will be updating over the next few days. (Technically if you were willing to convert every letter of each OSC endpoint by hand its useable).

you would do that by running the program, hitting refresh mappings, then inputting your values into the config file by hand. For example:

{"R3":[62,59,48,55,55]} would hit /avatar/parameters/SPELL with true when you hold the R3 binding (See your steamvr binding menu to determine which binding is which).

In the next few days I am planning to create a solution that lets you generate binding files easily. 

Over time I hope to cut down the program signifigantly. BOLL7708s OpenVR2Key program that this uses as a base is very optimized for what it does, and as such isn't simple to change to suit what I want it to do. But it will get there eventually.

Disclaimer:
This is my first ever C# project, it is also the first project I am seriously using github for. Expect jank in the code, expect it to be messy until I get it in a functionality state that I like. Big thanks to BOLL7608 for creating OpenVR2Key which gave me a light at the end of the tunnel when it came to actually getting steamvr input to work. 
