# SpeedrunComSharp

[![API Version](https://img.shields.io/badge/API-c4413b...-blue.svg)](https://github.com/speedruncom/api/tree/c4413b86d7c2088c317de8e3940b022b11c26323)

SpeedrunComSharp is a .NET wrapper Library for the [Speedrun.com API](https://github.com/speedruncom/api).

## How to use

Download and compile the library and add it as a reference to your project. You then need to create an object of the `SpeedrunComClient` like so:

```C#
var client = new SpeedrunComClient();
```

The Client is separated into the following Sub-Clients, just like the Speedrun.com API:
* Categories
* Games
* Guests
* Leaderboards
* Levels
* Notifications
* Platforms
* Profile
* Regions
* Runs
* Series
* Users
* Variables

The Sub-Clients implement all the API Calls for retrieving the Objects from the API. Once you obtained objects from those Clients, you can either use the References within the Objects to retrieve additional objects or you can use their IDs to retrieve them through the Clients.

## Example Usage

```C#
//Creating the Client
var client = new SpeedrunComClient();

//Searching for a game called "Wind Waker"
var game = client.Games.SearchGame(name: "Wind Waker");

//Printing all the categories of the game
foreach (var category in game.Categories)
{
  Console.WriteLine(category.Name);
}

//Searching for the category "Any%"
var anyPercent = game.Categories.First(category => category.Name == "Any%");

//Finding the World Record of the category
var worldRecord = anyPercent.WorldRecord;

//Printing the World Record's information
Console.WriteLine("The World Record is {0} by {1}", worldRecord.Times.Primary, worldRecord.Player.Name);

```

## Optimizations

The Clients are somewhat more flexible as the Properties in the individual Objects for traversing the API, because they can embed additional objects to decrease Network Usage. If you want to optimize your API usage, make sure to use the Clients where possible.

The Library automatically minimizes Network Usage, so iterating over `category.Runs` multiple times for example only results in a single API Call. If you are iterating over an IEnumerable that results in a Paginated API Call, the API will only be called for those pages that you are iterating over. If two completely unrelated events result in the same API Call, the SpeedrunComSharp Library will notice that and return a cached result for the second API Call.
