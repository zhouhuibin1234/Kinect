using Microsoft.Kinect;
using System;

namespace SeniorProject.Kinect
{
    public class BoneOrientationCharacteristics
    {
        public JointType FirstJoint;
        public JointType SecondJoint;
        public float slopeYX;
        public float acceptableError;
        public bool tempMatched = false;

        private static float slopeFromImage(Joint jntFirstJoint, Joint jntSecondJoint)
        {
            float floatSlope = 0;
            //protect against div by zero, which would happen in the case of
            if (jntSecondJoint.Position.X != jntFirstJoint.Position.X)
            {
                floatSlope = (jntSecondJoint.Position.Y - jntFirstJoint.Position.Y) / (jntSecondJoint.Position.X - jntFirstJoint.Position.X);
                Console.WriteLine(floatSlope.ToString());
            }
            else
            {
                return floatSlope = 1000;
            }

            return floatSlope;
        }

        public bool boolBoneOrientationIsMatch(Joint jntFirstJoint, Joint jntSecondJoint)
        {
            float lowerBound = slopeYX - acceptableError;
            float UpperBound = slopeYX + acceptableError;
            float calculatedSlope = slopeFromImage(jntFirstJoint, jntSecondJoint);
            if (calculatedSlope >= lowerBound && calculatedSlope <= UpperBound)
            {
                return true;
            }

            return false;
        }
    }
}