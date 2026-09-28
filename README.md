# Vibe Coded AI Slop

The entire project is **VIBE CODED AI SLOP** since I can't be bothered to spend time writing profiling code for someone
else's game. I make no guarantees that any of this code works, is safe, is correct, or is even useful. Use at your own
risk.

# TCG Profiler

Collection of profiling tools used to trace performance issues in TCG Card Shop Simulator on low power devices.

## Overview

[TCG Card Shop Simulator](https://store.steampowered.com/app/3070070?snr=5000_5100__) is a relatively simple game yet it
has some noticeable performance issues on low power devices, like the Steam Deck. As a software engineer who is only just
now getting into game development, this _feels_ wrong to me. I wanted to see if I could figure out where the game was
spending so much of it's time in an attempt to _maybe_ develop a mod that would fix things. So far, I've come up with
precious little, save for NPC's being the single biggest source of update time in the game. Need more vibe coded
profiling code to get a better idea of what's going on.
