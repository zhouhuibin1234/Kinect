using System;
using System.Collections.Generic;

namespace SeniorProject.Kinect
{
    public class Activity
    {
        private string fActivityName;
        private List<ActivityStep> fActivityStepArray;
        private int fCurrentScore;
        private int fCurrentStepIndex;
        private bool fIsPaused;
        private bool fIsLoaded = false;
        private DateTime timePaused;

        public void IncrementStep()
        {
            if (fCurrentStepIndex < fActivityStepArray.Count - 2 && fIsLoaded == true)
            {
                fIsPaused = false;
                fActivityStepArray[fCurrentStepIndex].boolFirstRun = true;
                fCurrentStepIndex++;
                fActivityStepArray[fCurrentStepIndex].boolFirstRun = true;
            }
            else if (fIsLoaded == true)
            {
                fActivityStepArray[fCurrentStepIndex].boolFirstRun = true;
                fIsPaused = false;
            }
        }

        public void DecrementStep()
        {
            if (fCurrentStepIndex > 0 && fIsLoaded == true)
            {
                fIsPaused = false;
                fActivityStepArray[fCurrentStepIndex].boolFirstRun = true;
                fCurrentStepIndex--;
                fActivityStepArray[fCurrentStepIndex].boolFirstRun = true;
            }
            else if (fIsLoaded == true)
            {
                fIsPaused = false;
                fActivityStepArray[fCurrentStepIndex].boolFirstRun = true;
            }
        }

        public void TogglePause()
        {
            if (fIsLoaded == true)
            {
                if (fIsPaused == true)
                {
                    fIsPaused = false;
                    if (fActivityStepArray[fCurrentStepIndex].isTimerUsed == true)
                    {
                        fActivityStepArray[fCurrentStepIndex].ResumeTimer(timePaused);
                    }
                }
                else
                {
                    fIsPaused = true;
                    timePaused = System.DateTime.Now;
                }
            }
        }

        public void IncrementScore()
        {
            fCurrentScore = fCurrentScore + fActivityStepArray[fCurrentStepIndex].scoreValue;
        }

        public bool IsTimerExpired()
        {
            ActivityStep actStep = fActivityStepArray[fCurrentStepIndex];
            if (actStep.isTimerUsed == true && System.DateTime.Now > actStep.timeEnd)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ActivityStep StepCurrent()
        {
            return fActivityStepArray[fCurrentStepIndex];
        }

        public void HandleNullActivity()
        {
            if (StepCurrent() == null)
            {
                Helpers.Errors.WriteError(new InvalidOperationException("null activity step"), "There probably was an indexing error that is already handled appropriately");
                IncrementStep();
                return;
            }
        }

        public void SetupCurrentStep()
        {
            StepCurrent().FirstStepSetup();
        }

        public Activity()
        {
        }

        public string activityName
        {
            get
            {
                return fActivityName;
            }
            set
            {
                fActivityName = value;
            }
        }

        public List<ActivityStep> activityStepArray
        {
            get
            {
                return fActivityStepArray;
            }
            set
            {
                fActivityStepArray = value;
            }
        }

        public int currentScore
        {
            get
            {
                return fCurrentScore;
            }
            set
            {
                fCurrentScore = value;
            }
        }

        public int currentStepIndex
        {
            get
            {
                return fCurrentStepIndex;
            }
            set
            {
                fCurrentStepIndex = value;
            }
        }

        public bool isLoaded
        {
            get
            {
                return fIsLoaded;
            }
            set
            {
                fIsLoaded = value;
            }
        }

        public bool isPaused
        {
            get
            {
                return fIsPaused;
            }
            set
            {
                fIsPaused = value;
            }
        }
    }
}