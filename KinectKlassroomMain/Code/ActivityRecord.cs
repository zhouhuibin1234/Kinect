using System;
using System.Collections.Generic;
using System.IO;

namespace SeniorProject.Kinect
{
    internal class ActivityRecordContainer
    {
        private List<UIEventEntry> UIEntries = new List<UIEventEntry>();
        private List<ActivityEventEntry> ActivityEntries = new List<ActivityEventEntry>();

        public void AddNewUIEvent(UIEvents type, string name)
        {
            UIEventEntry newEvent = new UIEventEntry();
            newEvent.Event = type;
            newEvent.time = DateTime.Now;
            newEvent.userName = name;
            UIEntries.Add(newEvent);
        }

        public void AddNewActivityEvent(ActivityEvents type, string name)
        {
            ActivityEventEntry newEvent = new ActivityEventEntry();
            newEvent.Event = type;
            newEvent.time = DateTime.Now;
            newEvent.userName = name;
            ActivityEntries.Add(newEvent);
        }

        public void AddNewActivityEventWithStart(ActivityEvents type, string name, DateTime begin)
        {
            ActivityEventEntry newEvent = new ActivityEventEntry();
            newEvent.Event = type;
            newEvent.time = DateTime.Now;
            newEvent.userName = name;
            newEvent.beginning = begin;
            ActivityEntries.Add(newEvent);
        }

        public void WriteToFile(string address)
        {
            // create a writer and open the file
            using (TextWriter tw = new StreamWriter(address))
            {
                tw.WriteLine("Kinect Classroom - Nathan C. Castle - Copyright 2013" + Environment.NewLine +
                    "Version-Type: Pre-Release" + Environment.NewLine + "Version-Number: 0.0.2" + Environment.NewLine +
                    "Warning: Modifying this document may prevent it from being used by Kinected Classroom. Modify at own risk.");

                //Go Through Each Step and Write Info
                int intloop = 0;

                foreach (Kinect.ActivityEventEntry Entry in ActivityEntries)
                {
                    tw.WriteLine("ActivityEntry - Start");

                    tw.WriteLine("ActivityEntry - " + intloop.ToString() + " - #" + Entry.beginning.ToString() + "#");
                    tw.WriteLine("ActivityEntry - " + intloop.ToString() + " - #" + Entry.score.ToString() + "#");
                    tw.WriteLine("ActivityEntry - " + intloop.ToString() + " - #" + Entry.secondsAllowed.ToString() + "#");
                    tw.WriteLine("ActivityEntry - " + intloop.ToString() + " - #" + Entry.time.ToString() + "#");
                    tw.WriteLine("ActivityEntry - " + intloop.ToString() + " - #" + Entry.userName.ToString() + "#");
                    switch (Entry.Event)
                    {
                        case ActivityEvents.FailStep:
                            tw.WriteLine("ActivityEntry - " + intloop.ToString() + " - #FAIL#");
                            break;

                        case ActivityEvents.PassStep:
                            tw.WriteLine("ActivityEntry - " + intloop.ToString() + " - #PASS#");
                            break;

                        case ActivityEvents.TimeOutStep:
                            tw.WriteLine("ActivityEntry - " + intloop.ToString() + " - #TIMEOUT#");
                            break;
                    }
                    tw.WriteLine("ActivityEntry - Stop");

                    intloop++;
                }
                foreach (Kinect.UIEventEntry Entry in UIEntries)
                {
                    tw.WriteLine("UIEntry - Start");

                    tw.WriteLine("UIEntry - " + intloop.ToString() + " - #" + Entry.time.ToString() + "#");
                    tw.WriteLine("UIEntry - " + intloop.ToString() + " - #" + Entry.userName + "#");

                    switch (Entry.Event)
                    {
                        case UIEvents.About:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #ABOUT#");
                            break;

                        case UIEvents.Close:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #CLOSE#");
                            break;

                        case UIEvents.Continue:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #CONTI#");
                            break;

                        case UIEvents.File:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #FILE#");
                            break;

                        case UIEvents.Help:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #HELP#");
                            break;

                        case UIEvents.Home:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #HOME#");
                            break;

                        case UIEvents.NextStep:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #NEXT#");
                            break;

                        case UIEvents.Options:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #OPTION#");
                            break;

                        case UIEvents.Pause:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #PAUS#");
                            break;

                        case UIEvents.PreviousStep:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #PREV#");
                            break;

                        case UIEvents.Save:
                            tw.WriteLine("UIEntry - " + intloop.ToString() + " - #SAVE#");
                            break;
                    }
                    tw.WriteLine("UIEntry - Stop");

                    intloop++;
                }
            }
        }
    }

    public class ActivityRecordEntry
    {
        public DateTime time;
        public String userName;

        public ActivityRecordEntry()
        {
            time = System.DateTime.Now;
            userName = "Default User";
        }

        public ActivityRecordEntry(string name)
        {
            time = System.DateTime.Now;
            userName = name;
        }
    }

    internal enum UIEvents { NextStep, PreviousStep, Pause, Continue, File, About, Options, Close, Home, Save, Help }

    internal class UIEventEntry : ActivityRecordEntry
    {
        public UIEvents Event;
    }

    internal enum ActivityEvents { PassStep, TimeOutStep, FailStep }

    internal class ActivityEventEntry : ActivityRecordEntry
    {
        public ActivityEvents Event;
        public int score;
        public int secondsAllowed;

        /// <summary>
        /// The datetime when the step began
        /// </summary>
        public DateTime beginning;
    }
}