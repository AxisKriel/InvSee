InvSee
======
[TShock](https://github.com/NyxStudios/TShock/) plugin. Utilizes SSC technology to copy a player's inventory.

Current version: 1.0.1.0307


### Commands ###
`invsee <player name>` - The main command. Will copy target player's inventory, if possible. Will also generate a backup of your character's inventory. The backup will only be generated if one is not in place -  this prevents the loss of your precious items when copying multiple players in a row. Last but not least, use `invsee` with no parameters to restore your inventory, destroying the backup in the proccess (simple, huh?).

Inventories are automatically restored upon leaving. Crashes and connection losses are exceptions - try not to hang around with other's inventories for too long. A fix might be implemented in the future.


### Permissions ###
`invsee.main` - Required to use the `invsee` command.

`invsee.user` - Combine with the above to allow copying offline players (users).
