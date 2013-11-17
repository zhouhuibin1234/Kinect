using Microsoft.Kinect;

namespace SeniorProject.Kinect
{
    public class JointOrientationCharacteristics
    {
        public JointType JointTypeID;
        public float jointZPosition;
        public float jointXPosition;
        public float jointYPosition;
        public bool zCompared;
        public bool xCompared;
        public bool yCompared;
        public float acceptableError; //This will cause the program to accept angles that are + or - this value to be accepted
    }
}