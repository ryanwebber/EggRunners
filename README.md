# Egg Runners

Egg Runners is a party game where you compete in obstacle courses against your friends
and computers. Also, challenge yourself with time trials on difficult courses.

## Status
 - [x] Basic lighting and materials
 - [x] Chunk loading
 - [x] Basic camera dolly
 - [x] Scene loading 
 - [x] Player input system
 - [x] Movement mechanics
 - [x] Obstacle kill-plane clipping
 - [x] Kill-plane physics disable
 - [x] Player "death"
 - [x] Late-jump forgiveness
 - [x] Camera panning
 - [x] Level restart
 - [x] Level complete
 - [ ] Race-variation supporting multi-player
 - [ ] Player collision mechanics
 - [ ] Player knockout tracking
 - [ ] Computer players
 - [ ] Round-based scene management for races
 - [ ] Point tracking for races
 - [ ] Menu UI
 - [ ] HUDs
 - [ ] Wall-running? Wall-jumping?
 - [ ] Level editor?

## Computer Player Architecture

 * Fully ML based
  * Needs input "smoothing"
  * Difficult to do special movement events (wall running)
  * Flixible with things like moving platform
  * Difficult to control difficulty
  * Might be able to train against other players to get novel offensive capabilities
 * Emergent approach
  * Many small rules
   * Jump if no floor in front of you
   * Nav-mash forwards per-platform
    * Where to actually go? Needs to be at a place that connects to next chunk
    * Needs to be able to be dynamically generated
   * How to do moving platforms?
   * How to do wall jumps?
   * How to handle branching decisions?
   * How to handle offensive maneuvers?
  * Maybe using decision trees?
 * Hybrid approach?

