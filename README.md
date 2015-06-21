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

The only difference is the Records Sub-Client that allows you to use their legacy Records API that's available at http://www.speedrun.com/api_records.php. They Records API is not done yet though.

The Sub-Clients implement all the API Calls for retrieving the Objects from the API. Once you obtained objects from those Clients, you can either use the References within the Objects to retrieve additional objects or you can use their IDs to retrieve them through the Clients. The Clients are somewhat more flexible though, as they can embed additional objects to decrease Network Usage.
