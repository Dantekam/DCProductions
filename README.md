# DCProductions

DCProductions is a Unity-based immersive VR project exploring interactive 360° video and branching narrative experiences.

The application combines immersive video playback with a node-based story system that allows the experience to progress automatically or branch based on user choices.

## Features

- Immersive 360° video playback
- VR interaction
- Meta/Oculus support
- Hand-tracking configuration
- Branching narrative structure
- Story nodes and user choices
- Automatic and choice-driven story progression
- Event-driven video transitions
- RenderTexture-based 360° presentation

## Branching Story System

The project uses a modular story graph built from individual story nodes.

Each node can contain:

- An associated video
- A unique node identifier
- Available user choices
- References to subsequent story nodes
- A trigger mode controlling story progression

The story-flow system manages transitions between nodes and coordinates narrative progression with video playback.

Depending on the configured node, the experience can automatically continue after a video finishes or wait for the user to make a choice before loading the next part of the experience.

## 360° Video

Unity's video and rendering systems are used to present immersive 360° media within the VR environment.

Video playback is coordinated with the story system so that completion events can trigger narrative transitions or make new choices available to the player.

## Technologies

- Unity
- C#
- Meta/Oculus VR
- 360° video
- Unity VideoPlayer
- RenderTexture
- Hand tracking
- Event-driven programming

## Architecture

Key systems include:

- `PlaybackManager` — manages immersive video playback
- `StoryFlowManager` — controls progression through the experience
- `StoryGraphManager` — manages the available story graph
- `StoryNode` — represents an individual narrative state
- `StoryChoice` — represents branching user decisions
- `HandTrackingSetup` — configures VR hand interaction

## Project Background

This project explores how immersive 360° media can be combined with interactive branching narratives in virtual reality.

Development focused on separating video playback from narrative state management so that additional story nodes and choices could be incorporated without hard-coding the entire experience into a single sequence.

## Development Status

**Prototype to be built upon**
