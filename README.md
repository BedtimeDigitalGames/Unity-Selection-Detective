# Selection Detective
Selection Detective is a tool for viewing and selecting contents of a GameObject and its children.
With this tool you can find easily select or find all the GameObjects that are using any:
- Components 
- Layers
- Materials
- Shaders
- Names
- Tags
- Serialized/Public fields

## How to use
Right click any GameObject(s) in your scene and click "Selection Detective" to get started!

![image](https://user-images.githubusercontent.com/104233613/164817137-6e644652-dae8-4fc7-ae00-031e36da22a2.png)

From here you can see what the entire contents of the selection and their hierarchy contains. 
Clicking on an item will select all the GameObjects using the item you clicked.
This can be extremely useful to for example locate all the objects that use a specific shader, etc.

# Examples
In these examples, A GameObject with thousands of children has been selected to showcase the various possible modes.

### Components
Shows all the components used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with this component attached.

![1](https://user-images.githubusercontent.com/104233613/164816360-b4239657-6dfb-43b2-a59b-08dd854cd574.png)

### Layers
Shows all the layers used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects using this layer.

![2](https://user-images.githubusercontent.com/104233613/164816362-d473f8a2-b695-4bcf-b013-fd0e39ba8945.png)

### Materials
Shows all the materials used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with a renderer using this material.

![3](https://user-images.githubusercontent.com/104233613/164816363-8b8def56-f5dc-43ad-b34b-d48ab3db3e1e.png)

### Names
Shows all the GameObject names in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with this name.

![4](https://user-images.githubusercontent.com/104233613/164816364-e58a8fec-dac5-47e3-ae8e-87b956655e6a.png)

### Shaders
Shows all the shaders used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with a renderer using this shader.

![5](https://user-images.githubusercontent.com/104233613/164816365-5158cfbb-2dbf-43b6-ad80-054840a6a86d.png)

### Tags
Shows all the tags used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with this tag.

![6](https://user-images.githubusercontent.com/104233613/164816367-921f1f02-1714-4c20-b518-f14d4a652986.png)

### Serialized/Public Fields
Shows the names of all serialized fields of all the components of your selection hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with a component that has this specific field.

![7](https://user-images.githubusercontent.com/104233613/164816368-a8fb28cc-8a68-47fa-ba2f-b821814aea3f.png)


