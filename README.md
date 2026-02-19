# DnDigital
DnDigital is a Dungeons & Dragons companion app designed to enhance tabletop gameplay by connecting physical and digital worlds. Players and Dungeon Masters can interact with and visualize the game map in real time, combining computer vision, a database infrastructure, and a multi-platform client.

Submitted for the course of Human Computer Interaction on the Web by:

- Chiara Frascaria
- Eleonora Ronca
- Michele Saraceno

## Content
- Computer Vision (C++, CMake)
- Database management (AWS)
- Interaction with the projector (Python)
- Client App (C#, Unity)

### Computer Vision (C++, CMake)

A camera mounted above the grid continuously captures frames, which are processed using a pipeline of computer vision techniques (e.g. blurring, frame differencing, warping, and contour detection) to identify colored pawns and their positions. The result is a cell matrix that is updated in real time whenever a pawn is moved and broadcast to all clients.

Note: requires the OpenCV library.

### Database management (AWS)

A cloud-hosted database manages all persistent game data, including information on Monsters, Obstacles, and Players.

### Interaction with the projector (Python)

Handles communication with the projector to render the map onto a flat surface. It listens for player input from the app and overlays visual feedback (ability ranges and area-of-effect indicators) directly onto the projected map.

### Client App (C#, Unity)

A cross-platform app (Android and PC) that receives and displays the positional matrix from the backend. The Dungeon Master can assign entities to unidentified pawns, while players can select abilities to preview their range on the projected map. Tapping on any pawn brings up a detailed entity card with stats and information.


