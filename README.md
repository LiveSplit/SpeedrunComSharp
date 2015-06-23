# SpeedrunComSharp

SpeedrunComSharp is a .NET wrapper Library for the [Speedrun.com
API](https://github.com/speedruncom/api). The current implementation of
SpeedrunComSharp implements all of the features available @ [`5ce7d20670ed89cc4b9781c0a745112eeef0cfe6`](https://github.com/speedruncom/api/tree/5ce7d20670ed89cc4b9781c0a745112eeef0cfe6).

## How to use

Download and compile the library and add it as a reference to your project. You then need to create an object of the `SpeedrunComClient` like so:

```C#
var client = new SpeedrunComClient();
```

The Client is separated into the following Sub-Clients, just like the Speedrun.com API:
* Categories
* Games
* Guests
* Levels
* Platforms
* Profile
* Records
* Regions
* Runs
* Users
* Variables

The only difference is the Records Sub-Client that allows you to use their legacy Records API that's available at http://www.speedrun.com/api_records.php.

## Example Usage

```C#
//Creating the Client
var client = new SpeedrunComClient();

//Searching for a game called "Wind Waker"
var game = client.Games.GetGames(name: "Wind Waker").First();

//Printing all the categories of the game
foreach (var category in Game.Categories)
{
  Console.WriteLine(category.Name);
}

//Searching for the category "Any%"
var anyPercent = game.Categories.First(category => category.Name == "Any%");

//Getting the leaderboard for the category
var leaderboard = anyPercent.Leaderboard;

//Finding the World Record of the category
var worldRecord = leaderboard.First();

//Getting the World Record Run
var worldRecordRun = worldRecord.Run;

//Printing the World Record's information
Console.WriteLine("The World Record is {0} by {1}", worldRecordRun.Times.Primary, worldRecordRun.Player.Name);

```

## Optimizations

The Sub-Clients implement all the API Calls for retrieving the Objects from the API. Once you obtained objects from those Clients, you can either use the References within the Objects to retrieve additional objects or you can use their IDs to retrieve them through the Clients. The Clients are somewhat more flexible though, as they can embed additional objects to decrease Network Usage.

The Library automatically minimizes Network Usage, so iterating over `category.Runs` multiple times for example only results in a single API Call. If you are iterating over an IEnumerable that results in a Paginated API Call, the API will only be called for those pages that you are iterating over. If two completely unrelated events result in the same API Call, the SpeedrunComSharp Library will notice that and return a cached result for the second API Call.
