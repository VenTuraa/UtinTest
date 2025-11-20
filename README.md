# UtinTest - Ball Path Puzzle Game

### Controls
- **Hold Left Mouse Button**: Charge a shot
  - The longer you hold, the larger the shot becomes
  - Your ball size decreases as the shot grows
- **Release Left Mouse Button**: Launch the shot
  - The shot travels toward the target
  - On impact, it explodes and destroys nearby obstacles

### Game Mechanics

1. **Size System**
   - Your ball starts at a default size
   - Creating shots reduces your ball's size
   - You must maintain enough size to pass through the path
   - The path width dynamically adjusts to match your ball's size

2. **Path Clearing**
   - The red line shows your intended path
   - Obstacles block your path if they're too close
   - You can only move when the path is clear
   - Destroy obstacles with shots to clear the way

3. **Movement**
   - Your ball automatically moves toward the target when:
     - The path is clear
     - Your size is sufficient
     - You're not charging a shot
   - Movement stops if obstacles block the path

4. **Win/Lose Conditions**
   - **Win**: Reach the target
   - **Lose**: 
     - Your ball becomes too small
     - You can't create more shots and the path is blocked


## Architecture
### Core Components

#### **PlayerBall** (`PlayerBall.cs`)
The main player character controlled by the player.
- **Responsibilities**:
  - Handles input (mouse button for charging/releasing shots)
  - Manages ball size and movement
  - Creates and launches shots via factory
  - Communicates with PathManager for path validation
- **Key Features**:
  - Size decreases when creating shots
  - Automatic movement when path is clear
  - Size validation before movement

#### **PathManager** (`PathManager.cs`)
Manages the path visualization and obstacle collision detection.
- **Responsibilities**:
  - Renders the path line (LineRenderer)
  - Tracks all obstacles in the scene
  - Validates if path is clear for movement
  - Dynamically adjusts path width based on player size
- **Key Features**:
  - Uses HashSet for efficient obstacle tracking
  - Geometric collision detection (point-to-line distance)
  - Path width synchronization with player size

#### **ShotBall** (`ShotBall.cs`)
Projectile that destroys obstacles.
- **Responsibilities**:
  - Moves toward target using physics
  - Explodes on obstacle collision
  - Destroys obstacles within explosion radius
- **Key Features**:
  - Size affects explosion radius
  - Factory pattern for creation (Zenject)

#### **Obstacle** (`Obstacle.cs`)
Blocking objects that prevent player movement.
- **Responsibilities**:
  - Registers/unregisters with PathManager
  - Can be destroyed by shots
  - Notifies GameStateHandler when destroyed

#### **GameManager** (`GameManager.cs`)
Manages game state and win/lose conditions.
- **Responsibilities**:
  - Tracks game end state
  - Handles victory/defeat logic
  - Manages UI popups
  - Scene restart functionality

#### **GameStateHandler** (`GameStateHandler.cs`)
Coordinates game state transitions.
- **Responsibilities**:
  - Mediates between PlayerBall, PathManager, and GameManager
  - Checks game state after obstacle destruction
  - Enables player movement when conditions are met

#### **Door** (`Door.cs`)
Interactive door that opens when player approaches.
- **Responsibilities**:
  - Monitors player distance
  - Smoothly opens when player is near
  - Uses async/await for smooth animation

### Design Patterns

1. **Dependency Injection (Zenject)**
   - All components are injected via constructors
   - Loose coupling between components
   - Easy testing and modularity

2. **Factory Pattern**
   - `ShotBall.Factory` creates shot instances
   - Managed by Zenject container

3. **Observer Pattern**
   - `IGameStateHandler` interface for state notifications
   - Components notify each other of state changes

4. **Strategy Pattern**
   - Different game end conditions handled uniformly
   - Extensible win/lose logic


### Key Constants (`GameConstants.cs`)
- `INITIAL_PLAYER_SIZE`: Starting ball size
- `PATH_SIZE_RATIO`: Ratio for path width calculation
- `SIZE_RESERVE_MULTIPLIER`: Safety margin for size checks
- `TARGET_REACH_DISTANCE`: Distance threshold for victory
- `DOOR_CHECK_INTERVAL_MS`: Door proximity check frequency
