# Minipoll V5 - Unity Project

## 📋 Project description
An Unity Project for Game/MINIPOLL App with perfect Entrance Sequance.

## 🎬 Entrance sequence system
The system includes:
- ** Logo Video **: displayed once at the beginning of the run
- ** minipoll video
- ** Menu Buttons **: appearance after the video ends

## 🛠️ main components

### Simpleentracemerager.cs
The main script that manages the entry sequence:
- Video Sequance Management
- Moving between steps
- Treatment in Pressure to skip
- Fullscreen settings for video

### Menubuttonsanimationcontroller.cs
Menuen animation manager

## 🎯 Installation and activation
1. Open the project at Unity 2022+
2
3. Choose Gameobject `Entraceneager '
4. Drag the components in Inspector:
   - Videoplayer → Video Player Field
   - Videodisplay → Video Display Field  
   - Menubuttons → Menu Buttons Field
   - Add Video Clips to Logo Video and Minipoll Video
5. Run the game

## 🔧 Settings
- `Enabledbugs': activate logs for tracking
- 'SkipvideosFortestting': skipping video to fast tests

## 📝 Technical Notes
- The project uses Videoplayer with Rendexture
- support at Fullscreen Video Display
- adapted to different resolutions
- Includes a skipping system with any key or pressing

## 🚀 Current version
A clean and working version of Entrance Sequance with repairing all compilation and display problems.

## 🎮 Git Version Control
The project is managed in GIT for safe retention of all changes.

### basic GIT commands:
`` BASH
# Check status
git status

# Add new changes
Git Add.

# Create a new commit
Git Commit -M “Description of Change”

# Create tag for a new version
git tag -a v1.1.0 -M "version description"

# See history
Git Log - -Onine
``

### versions:
- ** V1.0.0 **: First version works with full Entrance Sequence

## 📅 ​​last update date
July 2025