using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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
        public delegate Task EventConsumerDelegate(PlannedEvent generatedEvent);



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
        public async Task StartRunner(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            //generated a new guid of the simulation
            Guid simulationRun = Guid.NewGuid();

            //pre-build the sequence of the 
            int simulationLoop = _simulationPlan.SimulationLoops.HasValue ? _simulationPlan.SimulationLoops.Value : 1;

            //the start time of the simulation is provided by the plan OR it's just the actual time 
            DateTime lastSimulationStartTime = _simulationPlan.SimulationStartTime.HasValue ? _simulationPlan.SimulationStartTime.Value : getNowProxSec();
                  
            for (int currentLoop = 0; currentLoop < simulationLoop; currentLoop++)
            {
                List<Queue<PlannedEvent>> preGeneratedEventQueueByTrack = new List<Queue<PlannedEvent>>();
                                                
                DateTime nextLoopLastSimulationStartTime = lastSimulationStartTime;


                foreach (var track in _simulationPlan.PlannedEventsTracks)
                {
                    List<PlannedEvent> preGeneratedEvents = buildEventsPipeline(
                        _simulationPlan.PlanTiming, track.Value, lastSimulationStartTime, simulationRun, currentLoop);

                    var lastEventTime = preGeneratedEvents.Last().EventTime.Value;

                    if (lastEventTime > nextLoopLastSimulationStartTime)
                        nextLoopLastSimulationStartTime = lastEventTime;

                    preGeneratedEventQueueByTrack.Add(new Queue<PlannedEvent>(preGeneratedEvents));
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
        private List<PlannedEvent> buildEventsPipeline(PlanTimingEnum planTiming, List<PlannedEvent> plannedEventTrack, DateTime currentsimulationTime, Guid simulationRun, int? simulationLoop)
        {
            List<PlannedEvent> preGeneratedEvents = null;

            if (planTiming == PlanTimingEnum.OffsetFromSimulationStart)
            {
                preGeneratedEvents = new List<PlannedEvent>();
              
                //now loop over all the planned Items and generate the EventTime and OriginalEventTime according to their offsett & the simulation start time

                foreach (var simulationItem in plannedEventTrack)
                {
                    if (!simulationItem.EventOffset.HasValue)
                    {
                        throw new ApplicationException("Malformed PlannedEvent according to teh Simulation plan it should have the EventOffsett property!");
                    }

                    //calculate event time
                    var eventTime = currentsimulationTime + simulationItem.EventOffset.Value;
                    
                    //update the current simulation time up to the current event
                    currentsimulationTime = eventTime;

                    //update the simulated event properties
                    simulationItem.EventTime = eventTime;
                    simulationItem.OriginalEventTime = eventTime;

                    simulationItem.SimulationRun = simulationRun;
                    simulationItem.SimulationLoop = simulationLoop.HasValue ? simulationLoop : (int?)null;

                    //and add into the 
                    preGeneratedEvents.Add(simulationItem);
                }
            }

            if (planTiming == PlanTimingEnum.ExactTimeAccordingToPlan)
            {
                //this is the simple one...   planned event should have already their time configured to be used.
                preGeneratedEvents = plannedEventTrack;
            }

            return preGeneratedEvents;
        }

        private DateTime getNowProxSec()
        {
            var now = DateTime.UtcNow;

            var retNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            return retNow;
        }

        private void playbackEventPipeline(Queue<PlannedEvent> preGeneratedEventQueue)
        {
            //we look for event in the "past" respect Utc Now and we generate all of them immediately (with 25ms of delay to avoid huring downstream services)
            PlannedEvent preGeneratedEvent = null;

            while (preGeneratedEventQueue.Count > 0)
            {
                if (_cancellationToken.IsCancellationRequested)
                    break;

                preGeneratedEvent = preGeneratedEventQueue.Dequeue();

                if (preGeneratedEvent.EventTime.Value < DateTime.UtcNow)
                {
                    //we continue to generate events till we reach "NOW"  from that moment in the tiem we will use the real offsett
                    Task.Delay(25).Wait();
                }
                else
                {
                    //wait until the next event offsett.
                    Task.Delay(preGeneratedEvent.EventOffset.Value).Wait();
                }

                try
                {
                    //just fire the delegate! 
                    _eventConsumerDelegate(preGeneratedEvent).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception happened while firing the consume event delegate\n{ex.ToString()}");
                }
            }            
        }
        
    }
}
