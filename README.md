# OpenVR2OSC

An OSC app to be used with VRChat. It sets a given parameter to true when a corresponding controller button is held down.

Discord for support and roadmap: 

[<img src="https://assets-global.website-files.com/6257adef93867e50d84d30e2/636e0b5061df29d55a92d945_full_logo_blurple_RGB.svg" alt="discord" width="80"/>](https://discord.gg/Wvnz28xeVM)

Usage:
- Download the latest release of the OpenVR2OSC program.
- \*Ensure that no other OSC programs are running/binding the 9001 port. Once the program is launched, you can restart your other OSC programs if necessary.
- Open the steamVR bindings menu, select "choose another", if a choose binding page pops up hit the back button. Select more applications, and scroll till you find OpenVR2OSC. Click on it. In this menu you can see which keys correspond to which controller buttons. 
- Find the controller button that you want to use to activate the OSC endpoint and enter said OSC endpoint into that key's respective textbox on the OpenVR2OSC menu.
- Click the "Save bindings to config" button. 
- \*\*Now whenever the program is running, pushing that controller button will set the assigned OSC endpoint to true.

\*(After the initial launch, the OSC server binds itself to port 9110, unfortunately this is a limitation of the OSC library used. It is not required to use any sort of OSC router to transmit data to this port, as the program does not act on the incoming data. I hope to resolve this issue soon, or if VRChat implements OSC Query it will resolve on its own)

\*\*Opening the steamVR menu suspends the controller input to the program. This is to allow steamVR menu actions without accidentally setting off avatar features. 



If you are looking to do face expressions using the knuckles trackpad, check out KadachiiVR's Knuckles_to_osc: (https://github.com/KadachiiVR/knuckles_to_osc)!
They should have a unity script soon that easily converts facial expressions to use OSC inputs.

Disclaimer and Thanks:

Code in desperate need of a neatness pass, ill get to it, eventually. Big thanks to BOLL7608 for creating OpenVR2Key which gave me a light at the end of the tunnel when it came to actually getting steamVR input to work. Also a big thanks to ChanyaVRC for their handy VRCOscLib library.
