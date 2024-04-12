# Checkers C# WPF Application
This is the documentation for the Checkers (Draughts) game built using C# with a Windows Presentation Foundation (WPF) on the .NET Framework platform. The application comprises a graphical user interface, adhering to the Model-View-ViewModel (MVVM) design pattern. It features a standard 8x8 board with red and white pieces and incorporates multiple game functionalities including piece movement, king transformation, multi-jump capabilities, and game-saving features.

## Installation

Follow these steps to run the application:

1. Ensure you have the [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework) installed on your machine.
2. Download the source code of the application from GitHub.
3. Open the solution file (.sln) in Visual Studio.
4. Build the solution by selecting **Build > Build Solution** from the menu.
5. Start the application by pressing **F5** or selecting **Debug > Start Debugging**.

## Features

1. **Basic Moves and King Transformation**: Pieces move diagonally and can transform into a "King" when reaching the opposite end of the board, allowing them to move both forward and backward.
2. **Capturing Moves**: Players can capture an opponent's piece by jumping over it. If subsequent captures are possible, multi-jump moves can be executed.
3. **Game Statistics and History**: The game tracks the number of games won by each color and can display statistics about past games.
4. **Game Save and Load**: Players can save their current game state to a file and load it later. This functionality supports different file formats (binary, text, JSON, XML, etc.).
5. **Dynamic Game Configuration**: Through the game menu, users can start a new game, save current game configurations, open a previously saved game, and enable options like multiple jumps.
6. **User Interface**: The main window displays the checker's board and indicates which player's turn it is. At the end of a game, a message announces the winner.

## Usage

Upon launching the Checkers game application, you are greeted with the main menu, which offers two primary options: *About* and *Play*.

### About Section

+ **Accessing Information**: Click on the *About* button in the main menu. This will display a brief description of the game along with some personal details about the developer (e.g., name).
+ **Purpose**: This section is meant to provide users with context about the game's development and to offer contact information should they need to reach out for support or feedback.

### Starting a Game

+ **Play Game**: Select *Play* from the main menu to enter the game setup menu.
+ **Game Options**:
+ **Load Previous Game**: You can load a game you've previously saved by selecting the appropriate option. This allows you to continue a game from where you left off.
+ **New Game Settings**: Before starting a new game, you can configure several settings:
  + **Allow Multiple Jumps**: Enables the ability to perform multiple jumps in one turn, capturing several pieces of the opponent if the layout allows.
  + **Statistics**: This option allows you to view historical statistics such as the number of games won by each color and the maximum number of pieces remaining on the board at the end of the game.

### During the Game

+ **Saving the Game**: At any point during your game, you can save your current progress. Simply access the game menu and select the Save option. This feature is crucial if you need to pause your game and resume later.

## License
This software is provided "as is", without warranty of any kind. Users may modify and distribute it under terms suitable for their framing.


