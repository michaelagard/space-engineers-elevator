# space-engineers-elevator
A script for Space Engineers, this script offers a solution to a simple elevator setup with only 2 floors.

This script was written for a solution to a simple elevator setup with only 2 floors. By default the distance the piston travels is 4 large grid blocks tall, but you could manually set that value to what ever you'd prefer. The elevator doors are optional and can be disabled in the configuration.

This script does not support multi floor stops and does not support multi piston setups.

## Setup

This setup involves the following components:
* 1 Piston
* 2 Sensors
* 1 Programming block
* 2 Doors (optional)

Luckily the build instructions can be represented by a simple 2d ASCII diagram. The diagram shows the piston fully extended at 10 meters or 4 large grid blocks.

## Design

NOTE: The ascii should only be viewed in the programming blocks interface.

PP - piston
__ - piston top fully extended
|| - piston shaft piece
AB - any block / armor block
SS - sensor shaft 
SB - sensor bottom
ED - elevator door (optional)

ED __
      | |
      | |
      | | 
      | |  ED
      PP ZZ
      SS SB

The shaft sensor (SS) will need to detect every block in the shaft of the elevator reaching just above the piston top (__). The bottom sensor (SB) will only need to reach slightly above the armor block to detect the person wanting to call the elevator down. The elevator doors are optional, but prevent other players from falling into the elevator shaft.