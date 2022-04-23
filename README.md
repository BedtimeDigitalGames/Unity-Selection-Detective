# Selection Detective
<a href="https://openupm.com/packages/com.bedtime.selection-detective/">
  <img src="https://img.shields.io/npm/v/com.bedtime.selection-detective?label=openupm&amp;registry_uri=https://package.openupm.com" />
</a>

Selection Detective is a tool for viewing and selecting contents of a GameObject and its children.
With this tool you can easily select or find all the GameObjects that are using specific:
- Components 
- Layers
- Materials
- Shaders
- Names
- Tags
- Serialized/Public fields

## Installation
You can clone this repository directly into your *Packages* folder in your project, add the git URL through Unity's package manager, or through [OpenUPM](https://openupm.com/)
#### Git + Unity Package Manager
Add this URL to your Unity Package manager as a git package

```https://github.com/BedtimeDigitalGames/Unity-Selection-Detective``` 

![image](https://user-images.githubusercontent.com/104233613/164909451-0ca62c24-0106-463b-9c4b-e7fbcd6409ad.png)

#### OpenUPM
```$ openupm add com.bedtime.selection-detective```

# How to use
Right click any GameObject(s) in your scene and click "Selection Detective" to get started!

![image](https://user-images.githubusercontent.com/104233613/164817137-6e644652-dae8-4fc7-ae00-031e36da22a2.png)

From here you can see what the entire contents of the selection and their hierarchy contains. 
Clicking on an item will select all the GameObjects using the item you clicked.
This can be extremely useful to for example locate all the objects that use a specific shader, etc.

# Examples
In these examples, A GameObject with thousands of children has been selected to showcase the various possible modes.

## Components
Shows all the components used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with this component attached.

![1](https://user-images.githubusercontent.com/104233613/164816360-b4239657-6dfb-43b2-a59b-08dd854cd574.png)

## Layers
Shows all the layers used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects using this layer.

![2](https://user-images.githubusercontent.com/104233613/164816362-d473f8a2-b695-4bcf-b013-fd0e39ba8945.png)

## Materials
Shows all the materials used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with a renderer using this material.

![3](https://user-images.githubusercontent.com/104233613/164816363-8b8def56-f5dc-43ad-b34b-d48ab3db3e1e.png)

## Names
Shows all the GameObject names in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with this name.

![4](https://user-images.githubusercontent.com/104233613/164816364-e58a8fec-dac5-47e3-ae8e-87b956655e6a.png)

## Shaders
Shows all the shaders used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with a renderer using this shader.

![5](https://user-images.githubusercontent.com/104233613/164816365-5158cfbb-2dbf-43b6-ad80-054840a6a86d.png)

## Tags
Shows all the tags used in this hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with this tag.

![6](https://user-images.githubusercontent.com/104233613/164816367-921f1f02-1714-4c20-b518-f14d4a652986.png)

## Serialized/Public Fields
Shows the names of all serialized fields of all the components of your selection hierarchy. 
Clicking an item from the list will set your selection to all the GameObjects with a component that has this specific field.

![7](https://user-images.githubusercontent.com/104233613/164816368-a8fb28cc-8a68-47fa-ba2f-b821814aea3f.png)


