# QuickPlacer
## Summary
The Quick Placer is for rapidly placing gameobjects in a scene. It is fairly straight forward and simple, but there is also the Hydro Pole Placer and Fence Placer which use the Quick Placer.

The Hydro Pole Placer will place hydro/telephone poles and connect wires between them (which is simply a line renderer).

The Fence Placer will place fence posts and generate a mesh connecting the posts using a provided material.

## How to use

### Quick Placer
1. Put a gameobject in the "Prefab to Spawn" field
2. Use the toggles for snapping or random roation if you want
3. Click "Place Mode" to enter place mode. Click to place (hold to drag it around, scroll wheel to rotate), and release to place it.

### Fence Placer
1. Simialr to the QuickPlacer, put the fence pole in the prefab to spawn.
2. Place a fencing material in the "Fence Material" field. This can be a chain-link material for example.
3. Adjust the height percent to change how high the fence will be relative to the pole. A value of 0.5 will draw the fence halfway up the pole.
4. You can place fences only by clicking on an already placed pole, then clicking again somewhere else.

### Hydro/Telephone Pole Placer
1. Similar to the QuickPlacer, place the pole in the "Prefab to Spawn" field.
2. Place a wire prefab in the wire field. This prefab should have a line renderer component that uses local space.
3. The "Wire Connecting Name" is what is used to connect wires to the poles. In the pole prefab, there will be empty gameobjects with the name "[WirePoint]". By having that name in the inspector field, we're telling the script to look for those names in the children of the gameobject, and to set the wire's start and end points to those gameobjects' position.
4. The "Wire Connection Radius" tells us how far the wire points need to be from a connecting point to be considered "broken". More on this below. There is a yellow wire sphere gizmo that shows this value around the connecting points.
5. The "Number of Points" is how smooth you want the wire to be. Two points are the minimum (start and end points).
6. The curve is used to shape the wire. For a traditional wire that dips in the middle, the values should be as follows: (0,0),(0.5,-1), (1,0)
7. Place mode works the same as quick placer. With the exception of the "Previous Link" label below it. When there is a previous link, the next pole that's placed will have wires drawn between them.
8. "Wires Only" does not place the poles. It instead only draws wires between them. Click on a pole to select it (it becomes the previous link), then click on another pole to draw wires between them.
9. "Remove Broken Wires" will look though all the children of the placer (which would be where the poles are) and look for any wires that are too far from a connecting point. If a wire is found that's too far, it will be deleted.
