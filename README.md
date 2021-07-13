# YASE
Yet Another Simulation Engine


This is a super simple __simulation playback__  tool that uses JSON files as template to feed in a repeatable way Event Hub with a set of pre-recorded data at specific time intervals.

This id a DEV FIRST tool, you nee to know a bit of C# at least for the generation of the template file.

Playback of a template is quite simple and doesn't need DEV skills.




___
## Playback command line utility

Below you can see the output of the command line utility with its own embedded help:
~~~
Welcome to YASE - > Yet Another Simulation Engine!
--------------------------------------------------

Yet Another Simulation Engine playback command line interface 1.0.0

Usage: YASE [options] [command]

Options:
  -s|--simulationPlan             Simulation plan in YASEON format.
  -eh|--eventHubConnectionString  Event Hub Connection string

Commands:
  listTrack
  playAll
  playTracks
Please for additional help / information visit  the git hub repo:
https://github.com/algorni/YASE


Ciao
~~~

All what you need is a simulation plan file (a JSON file with a list of event and a timing definition) you can define even to repeate multiple time the same content if you want.

In the same file you can have multiple __tracks__.  You can leverage tracks to define multiple indipendent sources that emit events at different speed but still belonging to the same simulation (since you want to perform some analysis on all of them together).

An example could be multiple GPS Tracker which is moving indipendently but during the same period of time where eventually you want to detect the distances between each other.

You can get a list of track included in a simulation plan with the __listTrack__ command.

You can playback all the track with the command __playAll__.

You can playback a subset of track with the command __playTracks__ providing as additional parameters the list of track you want to playback.

The other parameter  you need is the Event Hub connection string (including the event hub topic name).

>NOTE: as Event Hub Partition key will be used the __SourceId__ field of the event see the code below:
~~~
    /// <summary>
    /// do something with this simulation!
    /// </summary>
    /// <param name="generatedEvent"></param>
    private static async Task doSomethingWithTheEventAsync(GeneratedEvent generatedEvent)
    {
        var eventJson = generatedEvent.ToJSON();

        // Create a batch of events using the SOURCE ID as the Partition
        EventDataBatch eventBatch = await eventHubProducerClient.CreateBatchAsync(
            new CreateBatchOptions() { PartitionKey = generatedEvent.SourceId });

        // Add events to the batch. An event is a represented by a collection of bytes and metadata. 
        var eventData = new EventData(Encoding.UTF8.GetBytes(eventJson));

        eventBatch.TryAdd(eventData);

        await eventHubProducerClient.SendAsync(eventBatch);

        Console.WriteLine($"{DateTime.UtcNow.ToString()}\n{eventJson}");
    }
~~~
___
## YASEON format
The YASEON (a mix between Yet Another Simulation Engie and JSON) is a JSON file with the list of track with their events.

This is an example of simulation plan:

~~~
{
  "PlanTiming": 0,
  "SimulationLoops": null,
  "PlannedEventsTracks": {
    "EnteringStartArea": [
      {
        "EventOffset": "00:00:05",
        "SourceId": "EnteringStartArea",
        "Payload": {
          "Latitude": 44.256250557075134,
          "Longitude": 7.781101127752286
        }
      },
      {
        "EventOffset": "00:00:05",
        "SourceId": "EnteringStartArea",
        "Payload": {
          "Latitude": 44.25593264442683,
          "Longitude": 7.781340135153765
        }
      },
      {
        "EventOffset": "00:00:05",
        "SourceId": "EnteringStartArea",
        "Payload": {
          "Latitude": 44.25550468237836,
          "Longitude": 7.781459638854503
        }
      },   
    ],
    "ExitingArea": [
      {
        "EventOffset": "00:00:05",
        "SourceId": "ExitingArea",
        "Payload": {
          "Latitude": 44.25405570203315,
          "Longitude": 7.781425494940008
        }
      },
      {
        "EventOffset": "00:00:05",
        "SourceId": "ExitingArea",
        "Payload": {
          "Latitude": 44.254122955075545,
          "Longitude": 7.78139135102551
        }
      }
    ]
  }
}
~~~

The root of the simulation plan contains a couple of attributes required by the simulation engine.

|Attribute|Possible Values|Description|
|---|---|---|
| PlanTiming | 0 | Entries in the Track contains a time offsett between each other, exact replay time will be calculated at the start of the playback |
| PlanTiming | 1 | Entries in the Track contains an exact time in which will be reproduced. |
| SimulationLoops | {integer} | [OPTIONAL] It reppresent the number of time YASE will reproduce the tracks, this is valid just for offsett based simulations  | 

Then there is an __PlannedEventsTracks__ entry with a list of tracks.

In the example above we have __two tracks__: _EnteringStartArea_ and _ExitingArea_ with some entries.

Every entity of a Tracks contains some attributes requireds by the simulation engine and a custom __Payload__.

| Attribute | Possible Values | Description |
| --- | --- | --- |
| SourceId | {string} | The Source ID of this event (used also as partion key in Event Hub) |
| EventOffset | {timespan} | [OPTIONAL] The offsett between this event and the next one |
| PlannedEventTime | {timestamp} | [OPTIONAL] The exact Time in which the evnet will be reproduced |
| Payload | {object} | The fully customizable payload |


> NOTE: use only  _PlannedEventTime_ or _EventOffset_ according to the _PlanTiming_ of the root object

> NOTE: actually you cannot mix track with EventOffset and PlannedEventTime on the same Simulation Plan file.

## Generating the Simulation Plan file
This repo contains couple of sample application to generate easly a simulation plan.

|Sample Name | Description|
|---|--|
|SampleSimulationGenerator| Generate random payloads with arbitrary km value |
|SpatialSimulationConverter| Transform a GEO JSON with a specific structure (multiline feature) into a sequence of events - useful to simulate a GPS Tracker |