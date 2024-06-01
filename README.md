# MineMancers
![Logo](Assets/Sprites/Logo%20New.png?raw=true "MineMancers")

## Screenshots
![Player throwing a projectile](Screenshots/Attack.png?raw=true "Player throwing a Projectile")
![Player getting hit by another player](Screenshots/Combat.png?raw=true "Player getting hit by another player")
![Player becoming a ghost](Screenshots/Ghost.png?raw=true "Player becoming a ghost")
![Player getting killed by another player](Screenshots/Kill.png?raw=true "Player getting killed by another player")
![Player mining ammunition from the deposit](Screenshots/Mining.png?raw=true "Player mining ammunition from the deposit")
![Player using a shield](Screenshots/Shield.png?raw=true "Player using a shield")

## Description
MineMancers is an exhilarating free-for-all 2D multiplayer deathmatch game where players assume the roles of mystical minemancers vying to accumulate the most kills within a tight 5-minute round. Each player starts with 100 health points and engages in intense combat to outlast and outplay their opponents.

Key Features:
- Dynamic Combat: Players can attack using two weapons — a Pickaxe and a throwable Projectile, each dealing 10 damage. The throwable Projectiles have limited ammunition, encouraging strategic use and frequent resupply.
- Resource Management: Ammunition is scarce and must be mined from gold deposits scattered across the battlefield. Deposits closer to the centre yield more ammo, but disappear temporarily when depleted, adding a layer of strategy to resource collection.
- Defensive Tactics: Players can activate a temporary shield for one second to block incoming projectiles, crucial for surviving in the heat of battle.
- Second Chances: Upon reaching 10 health points or below, players transform into ghosts with enhanced abilities, including double damage output. Ghosts can revive to full health and ammo by securing a kill, offering a dramatic comeback potential.
- Vibrant Visuals and Sound: The game features full animation, engaging music, and immersive sound effects, including the distinct sounds of mining and weapon impacts.

Whether strategizing over resource management or engaging directly in combat, MineMancers promises a fast-paced and engaging multiplayer experience.

## Controls
- WASD movement.
- Mouse to control the target.
- Left mouse button to attack.
- Right mouse button to use the shield.
- Q key to switch between Pickaxe and Projectile weapons.

## Features
- Lobby-based multiplayer with lobby browser
- Client-side prediction for player input
- Lag compensation for projectiles (server rewind)

## Technologies
- Unity
- Unity Relay
- Unity Lobby

## Team
- [Omigot](https://github.com/omigot): Game Designer, Programmer and Tester
- [Ianis Tutunaru](https://github.com/iantutunaru): Game Designer, Programmer and Tester
- [y-denkovych](https://github.com/y-denkovych): Game Designer and Tester
- [Ivan Tutunaru](https://github.com/IvanT98): Game Designer, Level Designer, Programmer and Tester

## Credits
- Game design: The whole team with special thanks to [y-denkovych](https://github.com/y-denkovych) whose early insights helped shape the game and stay on track.
- Lobbies and lobby browser: [Omigot](https://github.com/omigot)
- Player's shield feature: [Ianis Tutunaru](https://github.com/iantutunaru)
- Integration with Unity Relay and Unity Lobby services: [Omigot](https://github.com/omigot)
- Level design: [Ivan Tutunaru](https://github.com/IvanT98)
- Client-side prediction for player input: [Omigot](https://github.com/omigot)
- Lag compensation for projectiles: [Omigot](https://github.com/omigot)
- Everything else: [Ivan Tutunaru](https://github.com/IvanT98)

## Assets Credits
- Most of the graphical assets were taken from the [Shattered Pixel Dungeon](https://github.com/00-Evan/shattered-pixel-dungeon).
- [Blood sprites](https://opengameart.org/content/blood-splatters)
- [Menu music](https://opengameart.org/content/menu-music)
- [In-game music](https://opengameart.org/content/battle-theme-a)
- [Weapon attack and switch sounds](https://opengameart.org/content/rpg-sound-pack)
- [Steps sounds](https://opengameart.org/content/foot-walking-step-sounds-on-stone-water-snow-wood-and-dirt)
- [Death and pain sounds](https://opengameart.org/content/11-male-human-paindeath-sounds)

## Known Issues
- Players can sometimes phase through walls.
- Kills are sometimes counted twice.
- Kills can sometimes be attributed to the killed players.
- High score is reset when a player respawns even if the round is not over.
