This test has a more or less working grid system. Pretty barebones as it's a prototype, but things should be able to move around each other.

Controls:
- Click on pawns to select them, then click on a valid spot to move them. They will become inactive, and cannot move until the next turn.
- Right Click to start a new turn.

If you want pawns to move further, increase their move speed. Depending on move type, certain tiles will slow or block their progress. By Default, the obstacle prefabs work on all movement types. More than one movement type can be selected at a time. Pawns can move through tiles with a teammate on them, but not onto the same tiles. Make sure all pawn and obstacle innitial grid coordinates are within the grid, and are children objects to it. Remember, it starts at 0, so the furthest positions are Width -1 and Height -1. The grid will make an attempt to center itself on the camera.The prefab green tiles are slow and red ones are block.

Certain information is stored in teh project and scene (e.g. input buttons) so it may be easier to port certain things into this than vice versa. Try to stick to the order that'll cause the fewest headaches.

Feel free to clean up or refactor the code if you see something you can improve on, just make sure you commit it to git under a non-master (i.e. your own) branch. If you are unsure how to do that, you should probably focus on learning git prior to messing with the files.