# How to use the Code Manager tool

Due to the expansive nature of the tool created, this guide aims to direct you through using the various features.

The main feature of this tool is the usage of the Scriptable Object types created.
New instances of these can be created by right clicking then selecting: Create, Code Manager, then navigating to the type you wish to create.

![Unity_l9AtvZwEOC](https://github.com/aiden-knight/Tools-Project/assets/27894212/6a4c144a-5108-4f03-8559-d7dbe234b185)

There are default components and scripts created to interface with the scriptable objects.
The reference class is created to be used in custom components to store references and use Scriptable Object variables.
This can be seen on the script called Tester currently within the Unity project.

![image](https://github.com/aiden-knight/Tools-Project/assets/27894212/882f4b84-4463-4371-add2-27e32a30d193)

The Listener components are used to link up Scriptable Object events to functions in other classes, the simplest way to add these would be to click add component on a game object.

![Unity_vPtI5gzPBN](https://github.com/aiden-knight/Tools-Project/assets/27894212/35e75e4c-488f-4abd-be73-d1dd2da5a7f3)


## Code Manager Wizard
The code manager wizard window can be opened from the bar at the top under:
Window -> Code Manager -> Wizard

Once opened it can be docked next to the inspector for similar positioning to how it has been used over the project.

![image](https://github.com/aiden-knight/Tools-Project/assets/27894212/7c73e2b0-9c32-4f58-b3c1-bffed93c906c)

The above is a picture of the wizard.

At the top a list of all the Scriptable Objects of custom types created in this project are listed, the buttons take you to the asset in the project folder and create an instance of the inspector for the asset underneath the inspector text. The deselect button not far above that clears the inspector shown here should the you wish to.

Underneath this is a button that is intended to find all the references to all the Scriptable Objects. Once the references are found there are scripts to update these references on changes in scenes and prefabs.
This will be stored inside of a Unity Text Asset, located under Assets -> AidenK.CodeManager -> Settings. This asset should appear if it doesn't exist when the code manager wizard is opened.
Should the managers settings need to be reset, delete the settings folder then reimport the code manager wizard.

![image](https://github.com/aiden-knight/Tools-Project/assets/27894212/07f3692b-4b69-4a58-9fc4-87373c4ca1db)

For class generation, fill in the text field above with the data type. For example, can fill in with PlayerInfo, which is a struct already in the project. This field is case sensitive and can generate incorrect scripts with wrong input but these are easily deleted from Assets -> AidenK.CodeManager -> Generated. Then select the type of class you wish to generate and click "Generate Class".

## Inspectors
![image](https://github.com/aiden-knight/Tools-Project/assets/27894212/377ba138-6a81-4b4e-802a-39c6066ce82a)
![image](https://github.com/aiden-knight/Tools-Project/assets/27894212/a388b965-d775-4bb5-9984-b20e0054f4b0)


Above are a couple of images showcasing the custom inspector for a Scriptable Object type. It contains the default inspector followed by some extra features tagged on.

### Event
For the events there is a specific button to fire the event with a debug value from the inspector.

### References
For all types there is a list of buttons of the references to the assets.
These buttons take you to the assets, in the project folder, or in scene.

For assets in scenes, if the asset is in a scene that is not currently open it will take you to the scene asset. Double clicking this button opens the scene then takes you to the game object mentioned in the scene.
