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

        private Timer _simulationTick = null;


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
        public void StartRunner(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            Task.Run(() =>
                {
                    //pre-build the sequence of the 

                    List<PlannedEvent> preGeneratedEvents = buildEventsPipeline();

                    //to see if need to reverse the order or not
                    Queue<PlannedEvent> preGeneratedEventQueue = new Queue<PlannedEvent>(preGeneratedEvents);

                    playbackEventPipeline(preGeneratedEventQueue);

                   
                    return;

                }, cancellationToken);
        }


        /// <summary>
        /// calculate the generated event list!
        /// </summary>
        /// <returns></returns>
        private List<PlannedEvent> buildEventsPipeline()
        {
            List<PlannedEvent> preGeneratedEvents = null;

            if (_simulationPlan.PlanTiming == PlanTimingEnum.OffsetFromSimulationStart)
            {
                preGeneratedEvents = new List<PlannedEvent>();

                //the start time of the simulation is provided by the plan OR it's just the actual time 
                DateTime simulationStartTime = _simulationPlan.SimulationStartTime.HasValue? _simulationPlan.SimulationStartTime.Value : DateTime.UtcNow;

                //now loop over all the planned Items and generate the EventTime and OriginalEventTime according to their offsett & the simulation start time

                DateTime currentsimulationTime = simulationStartTime;

                foreach (var simulationItem in _simulationPlan.PlannedEvents)
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

                    //and add into the 
                    preGeneratedEvents.Add(simulationItem);
                }
            }

            if (_simulationPlan.PlanTiming == PlanTimingEnum.ExactTimeAccordingToPlan)
            {
                //this is the simple one...   planned event should have already their time configured to be used.
                preGeneratedEvents = _simulationPlan.PlannedEvents;
            }

            return preGeneratedEvents;
        }


        private void playbackEventPipeline(Queue<PlannedEvent> preGeneratedEventQueue)
        {
            //we look for event in the "past" respect Utc Now and we generate all of them immediately (with 25ms of delay to avoid huring downstream services)
            PlannedEvent preGeneratedEvent = preGeneratedEventQueue.Dequeue();

            do
            {
                if (_cancellationToken.IsCancellationRequested)
                    break;

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

                //next item please...
                preGeneratedEvent = preGeneratedEventQueue.Dequeue();
            }
            while (preGeneratedEventQueue.Count > 0);

        }
        
    }
}
