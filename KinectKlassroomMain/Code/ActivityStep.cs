using System;
using System.Windows.Documents;

namespace SeniorProject.Kinect
{
    public class ActivityStep
    {
        public int positionHoldThresholdMS = 1500;
        private bool IsHeldBegun = true;
        private DateTime heldBeginTime;
        public DateTime timeEnd;
        public int MSTimer; // Used to set a time limit, after which the activity step is considered to have failed]
        public bool isTimerUsed;
        public bool usesNewRotationFormat = false;
        public bool boolFirstRun = true;
        public FlowDocument activityTextDefault;
        public FlowDocument activityTextCorrect;
        public FlowDocument activityTextWrong;
        public int scoreValue;
        public JointOrientationCharacteristics[] JointComparison;
        public BoneOrientationCharacteristics[] BoneComparison;

        private void SetTimer()
        {
            if (isTimerUsed == true)
            {
                timeEnd.ToLocalTime();
                timeEnd = System.DateTime.Now;
                timeEnd = timeEnd.Add(System.TimeSpan.FromSeconds(Convert.ToInt32(MSTimer)));
            }
        }

        public void ResumeTimer(DateTime PausedTime)
        {
            timeEnd = timeEnd.Add(System.DateTime.Now.ToLocalTime() - PausedTime);
        }

        public void FirstStepSetup()
        {
            SetTimer(); //sets the timer

            //make sure that first attempt steps aren't repeated
            boolFirstRun = false;
        }

        public bool isFirstRun()
        {
            if (boolFirstRun)
            {
                return true;
            }
            return false;
        }

        public bool isHeldLongEnough()
        {
            heldBeginTime.ToLocalTime();
            if ((System.DateTime.Now - heldBeginTime).TotalMilliseconds >= positionHoldThresholdMS)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetHoldTimer()
        {
            heldBeginTime = System.DateTime.Now;
            IsHeldBegun = true;
        }

        public void ClearHoldTimer()
        {
            IsHeldBegun = false;
        }

        public bool IsHoldInProgress()
        {
            if (IsHeldBegun == true)
            {
                return true;
            }
            return false;
        }
    }
}