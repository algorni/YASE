using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using YASE.Core.Entities;

namespace YASE.Core
{
    /// <summary>
    /// This is the Simulation Runner engine
    /// It takes a Simulation Plan and it just playback the simulation "tape" deserializing and generating the event at the appropriate time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimulationRunner 
    {
        /// <summary>
        /// The delegate that the simulation running will call back is going to get a "IPlannedEvent" type
        /// </summary>
        /// <param name="generatedEvent"></param>
        public delegate Task EventConsumerDelegate(GeneratedEvent generatedEvent);



        private SimulationPlan _simulationPlan;

        private EventConsumerDelegate _eventConsumerDelegate;

        private CancellationToken _cancellationToken;


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="simulationPlan"></param>
        public SimulationRunner(SimulationPlan simulationPlan, EventConsumerDelegate eventConsumerDelegate)
        {
            _simulationPlan = simulationPlan;
            _eventConsumerDelegate = eventConsumerDelegate;
        }


        /// <summary>
        /// Star the Task asyncronously...
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartRunner(CancellationToken cancellationToken, string[] selectedTracks = null)
        {
            _cancellationToken = cancellationToken;

            //generated a new guid of the simulation
            Guid simulationRun = Guid.NewGuid();

            //pre-build the sequence of the 
            int simulationLoop = _simulationPlan.SimulationLoops.HasValue ? _simulationPlan.SimulationLoops.Value : 1;

            //the start time of the simulation is actual time at second precision...
            DateTime lastSimulationStartTime = getNowProxSec();
                  
            for (int currentLoop = 0; currentLoop < simulationLoop; currentLoop++)
            {
                List<List<GeneratedEvent>> preGeneratedEventQueueByTrack = new List<List<GeneratedEvent>>();
                                                
                DateTime nextLoopLastSimulationStartTime = lastSimulationStartTime;

                List<List<BaseEvent>> tracks = null;

                //a track list (selection of) is provided....  so just filter by the following track...
                if (selectedTracks != null)
                {
                    tracks = (from track in _simulationPlan.PlannedEventsTracks
                              where selectedTracks.Contains(track.Key)
                              select track.Value).ToList();
                }
                else
                {
                    tracks = (from track in _simulationPlan.PlannedEventsTracks
                              select track.Value).ToList();
                }

                foreach (var track in tracks)
                {
                    List<GeneratedEvent> preGeneratedEvents = buildEventsPipeline(
                        _simulationPlan.PlanTiming, track, lastSimulationStartTime, simulationRun, currentLoop);

                    var lastEventTime = preGeneratedEvents.Last().EventTime;

                    if (lastEventTime > nextLoopLastSimulationStartTime)
                        nextLoopLastSimulationStartTime = lastEventTime;

                    preGeneratedEventQueueByTrack.Add(preGeneratedEvents);
                }

                //ready for next loop in case...
                lastSimulationStartTime = nextLoopLastSimulationStartTime;


                List<Task> playbackTask = new List<Task>();

                //now actually playback them in parallell and wait for all of them
                foreach (var preGeneratedTrackQueue in preGeneratedEventQueueByTrack)
                {
                    var runTask = Task.Run(() => playbackEventPipeline(preGeneratedTrackQueue), cancellationToken);

                    playbackTask.Add(runTask);
                }

                await Task.WhenAll(playbackTask);
            }
        }

       

        /// <summary>
        /// calculate the generated event list!
        /// </summary>
        /// <returns></returns>
        private List<GeneratedEvent> buildEventsPipeline(PlanTimingEnum planTiming, List<BaseEvent> plannedEventTrack, DateTime currentsimulationTime, Guid simulationRun, int? simulationLoop)
        {
            List<GeneratedEvent> preGeneratedEvents = new List<GeneratedEvent>();

            if (planTiming == PlanTimingEnum.OffsetFromSimulationStart)
            {
                //now loop over all the planned Items and generate the EventTime and OriginalEventTime according to their offsett & the simulation start time

                int index = 0; 

                foreach (var simulationItem in plannedEventTrack)
                {
                    if (!(simulationItem is PlannedOffsettEvent))
                    {
                        throw new ApplicationException("Malformed PlannedEvent according to teh Simulation plan it should have the EventOffsett property!");
                    }
                                        
                    PlannedOffsettEvent plannedOffsettEvent = simulationItem as PlannedOffsettEvent;

                    GeneratedEvent generatedEvent = new GeneratedEvent(plannedOffsettEvent);


                    //calculate event time
                    var eventTime = currentsimulationTime + plannedOffsettEvent.EventOffset;
                    
                    //update the current simulation time up to the current event
                    currentsimulationTime = eventTime;

                    //update the simulated event properties
                    generatedEvent.EventTime = eventTime;
                    generatedEvent.CurrentSimulationRun = simulationRun;
                    generatedEvent.CurrentSimulationLoop = simulationLoop.HasValue ? simulationLoop.Value : 0;
                    
                    generatedEvent.EventIndex = index++;

                    //and add into the 
                    preGeneratedEvents.Add(generatedEvent);
                }
            }

            if (planTiming == PlanTimingEnum.ExactTimeAccordingToPlan)
            {
                int index = 0;

                //this is the simple one...   planned event should have already their time configured to be used.
                foreach (var simulationItem in plannedEventTrack)
                {
                    if (!(simulationItem is PlannedTimedEvent))
                    {
                        throw new ApplicationException("Malformed PlannedEvent according to teh Simulation plan it should have the EventOffsett property!");
                    }

                    PlannedTimedEvent plannedTimedEvent = simulationItem as PlannedTimedEvent;

                    GeneratedEvent generatedEvent = new GeneratedEvent(plannedTimedEvent);

                    generatedEvent.CurrentSimulationRun = simulationRun;

                    //this should be always 0  BTW....
                    generatedEvent.CurrentSimulationLoop = simulationLoop.HasValue ? simulationLoop.Value : 0;

                    generatedEvent.EventIndex = index++;

                    //and add into the 
                    preGeneratedEvents.Add(generatedEvent);
                }
            }

            return preGeneratedEvents;
        }

        private DateTime getNowProxSec()
        {
            var now = DateTime.UtcNow;

            var retNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            return retNow;
        }

        private void playbackEventPipeline(List<GeneratedEvent> preGeneratedEventQueue)
        {
            if (preGeneratedEventQueue == null || preGeneratedEventQueue.Count == 0)
                return;
              
            GeneratedEvent generatedEvent = null;
  
            for (int index=0; index < preGeneratedEventQueue.Count; index++)
            {
                if (_cancellationToken.IsCancellationRequested)
                    break;

                DateTime now = DateTime.UtcNow;

                generatedEvent = preGeneratedEventQueue[index];

                if (generatedEvent.EventTime > now)
                {
                    //wait until the event timing...
                    Task.Delay(generatedEvent.EventTime - now).Wait();
                }
                else
                {
                    Task.Delay(250).Wait();
                }

                try
                {
                    //just fire the delegate! 
                    _eventConsumerDelegate(generatedEvent).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception happened while firing the consume event delegate\n{ex.ToString()}");
                }
            }            
        }
        
    }
}
