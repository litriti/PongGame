# Swapping individual Sprites
You can use __Sprite Swap__ to change only one Sprite on the actor without affecting the other Sprites. This allows you to alter part of an actor's visuals (for example, changing its clothes or skin to a different color) while keeping the rest of the visuals the same.

In the following example, there are two Sprites that are variations of the actor’s scarf, with one being green and the other being blue. This workflow demonstrates how to switch from one to the other in the same actor:

![A human character sprite. Left: The original sprite with a green scarf. Right: The alternate sprite with a blue scarf.](images/bothscarves.png)<br/>__Left:__ The original `green scarf` Sprite. __Right:__ An alternate `blue scarf` Sprite.

1. Place the Sprites for both scarves into the same [Sprite Library Asset](SL-Asset.md), and add them both to the same **Category** (named `Scarf`).

2. Give each of the Sprites a unique __Label__ name (in this case `green scarf` and `blue scarf` respectively).
   This and the previous step can be automated by dragging and dropping sprites into the Categories tab empty space.

3. In the Scene, select the [Instantiated Prefab](https://docs.unity3d.com/Manual/InstantiatingPrefabs.html) and then select the `Scarf` GameObject in the Hierarchy window.

4. Go to the [Sprite Resolver component](SL-Resolver.md) of the `Scarf` GameObject. The Sprite Resolver‘s visual selector displays the two Sprites available in the `Scarf` Category.

5. Select the `blue scarf` to switch the Sprite rendered by the `Scarf` GameObject to it instead.<br/>

If you want to switch more than one Sprite at a time, consider [swapping the Sprite Library Asset](SLASwap.md) to switch to an entire alternate set of Sprites.

## Sprite Swap Scene View overlay
Sprite Swap overlay allows you to swap individual Sprites directly from the Scene View. The overlay supports swapping Sprites from multiple resolvers. This might be useful when working on complex characters.
The overlay can be enabled from the Overlay Menu which is accessible with the ` shortcut key, or from the Scene View's settings button.

Once the overlay is displayed, select a single or multiple game objects with Sprite Resolver component. If a selected game object has child game objects with Sprite Resolvers, they will be displayed as well.

<br/>![The Scene view with a knight character and the Sprite Resolver overlay. The overlay contains dropdowns that switch between different versions of the character's mouth, eyes, and head.](images/2D-animation-SResolver-overlay.png)

To Sprite Swap, select Sprite Resolver's categories and labels using your mouse, or arrow keys on your keyboard when the overlay is in focus.

It's possible to filter the list of Sprite Resolvers to show only Resolvers that contain two or more labels in their selected category. Select the first button in the bottom-left corner of the overlay to toggle the filter.

## Sprites pivot alignment
When working with [skinned Sprites](SkinningEditor.md), the positions of their Meshes' vertices are calculated based on the current skeleton pose, and are unaffected by each Sprite’s individual pivot. However, when [swapping](SpriteSwapIntro.md) Sprites which are not skinned (that is not [Rigged](SkinEdToolsShortcuts.md#bone-tools) to an actor’s skeleton), then they may not align correctly as their pivots are not in the same relative positions. This is especially noticeable if the Sprites are of very different sizes. The following example shows how Sprites can misalign when a skinned Sprite is swapped with an unskinned one:

![Figure 1: The original open-hand sprite.](images/Pivot_Scene_OpenHand.png)

![Figure 2: Swapping to the thumbs-up sprite. The hand is detached from the wrist.](images/Pivot_Scene_GestureOffset.png)

In this example, the GameObject containing the Sprite and the Sprite Swap component are aligned to match the `open hand` Sprite in the Skinning Editor. As the `thumbs up` Sprite is not rigged to the same skeleton, it appears misaligned as its pivot location is not in the same relative position as the original Sprite. To align the unskinned  `thumbs up` Sprite, you must adjust its pivot to match the relative position of the `open hand` Sprite’s pivot.

__Note__: If a Sprite is rigged to a skeleton, then its individual pivot location is overridden by the influence and position of the bone it is weighted to .

To change the pivot position of a Sprite, first select the Sprite in the Sprite Editor, which causes the __Sprite__ panel to appear at the bottom right of the __Sprite Editor__ window. The __Sprite__ panel shows details of the selected Sprite, such as its __Name__, __Position__, and __Pivot__ properties. You can select from a dropdown list of predefined pivot options from the __Pivot__ menu. These include options such as __Center__ and __Top Left__, as well as __Custom Pivot__ (this unlocks the __Custom Pivot__ position property settings, allowing you to input your own custom position for the pivot).

In the following example, the two swapped Sprites are aligned by changing the __Pivot__ property from __Center__ to __Custom Position__, and inputting the Custom Pivot position that aligns the `thumbs up` Sprite with the ``open hand`` Sprite. After applying the changes, the swapped Sprite is now aligned with the rest of the actor after the Sprite Swap.

![The thumbs-up sprite is now correctly attached to the wrist.](images/Pivot_Scene_Fixed.png)
