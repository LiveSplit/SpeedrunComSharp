# SpeedrunComSharp

[![Build Status](https://travis-ci.org/LiveSplit/SpeedrunComSharp.svg?branch=master)](https://travis-ci.org/LiveSplit/SpeedrunComSharp)

SpeedrunComSharp is a .NET wrapper Library for the [Speedrun.com
API](https://github.com/speedruncom/api). The current implementation of
SpeedrunComSharp implements all of the features available at [`5734df...`](https://github.com/speedruncom/api/tree/5734df52a8902a8c26e6a1060e8511092eb568ef)

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

//Getting the World Record Run
var worldRecordRun = worldRecord.Run;

//Printing the World Record's information
Console.WriteLine("The World Record is {0} by {1}", worldRecordRun.Times.Primary, worldRecordRun.Player.Name);

```

## Optimizations

The Clients are somewhat more flexible as the Properties in the individual Objects for traversing the API, because they can embed additional objects to decrease Network Usage. If you want to optimize your API usage, make sure to use the Clients where possible.

The Library automatically minimizes Network Usage, so iterating over `category.Runs` multiple times for example only results in a single API Call. If you are iterating over an IEnumerable that results in a Paginated API Call, the API will only be called for those pages that you are iterating over. If two completely unrelated events result in the same API Call, the SpeedrunComSharp Library will notice that and return a cached result for the second API Call.
