using Microsoft.Kinect;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SeniorProject.Kinect
{
    public class FrameProcessors
    {
        //public static WriteableBitmap bmpColor;
        private byte[] colorPixels;

        private int frameWidth;
        private int frameHeight;
        private Skeleton[] Skeletons;
        private KinectSensor ks;
        private WriteableBitmap bmpColor;
        public bool boolInitialized = false;

        public WriteableBitmap bmpFromColorFrame(ColorImageFrame NewFrame, bool SkeletonWanted, SkeletonFrame SkelFrame)
        {
            if (bmpColor == null)
            {
                bmpColor = new WriteableBitmap(frameWidth, frameHeight, 96, 96, PixelFormats.Bgr32, null);
            }

            if (NewFrame == null)
            {
                throw new InvalidOperationException("Null Image");
            }

            NewFrame.CopyPixelDataTo(colorPixels);
            // Write the pixel data into our bitmap
            bmpColor.WritePixels(
            new Int32Rect(0, 0, bmpColor.PixelWidth, bmpColor.PixelHeight),
                colorPixels,
                bmpColor.PixelWidth * sizeof(int),
                0);
            //Skeleton
            if (SkeletonWanted != false && SkelFrame != null)
            {
                return bmpWithSkelFromColor(bmpColor, SkelFrame);
            }
            return bmpColor;
        }

        private WriteableBitmap bmpWithSkelFromColor(WriteableBitmap bmpSource, SkeletonFrame SkelFrame)
        {
            bmpSource = BitmapFactory.ConvertToPbgra32Format(bmpSource);
            Skeletons = new Skeleton[SkelFrame.SkeletonArrayLength];
            SkelFrame.CopySkeletonDataTo(Skeletons);
            //Go through and draw active skeleton
            foreach (Skeleton Skel in Skeletons)
            {
                if (Skel.TrackingState == SkeletonTrackingState.Tracked)
                {
                    //iterate through each joint in the skeleton
                    foreach (Joint skeljoint in Skel.Joints)
                    {
                        SkeletonPoint JointLocation = Skel.Joints[skeljoint.JointType].Position;

                        ColorImagePoint Cloc = SkeletonPointToColorImage(JointLocation, ColorImageFormat.RgbResolution640x480Fps30);
                        bmpSource.FillRectangle((Cloc.X - 5), (Cloc.Y - 5), (Cloc.X + 5), (Cloc.Y + 5), Colors.Purple);
                    }
                    foreach (BoneOrientation orientation in Skel.BoneOrientations)
                    {
                    }
                }
            }
            return bmpSource;
        }

        private ColorImagePoint SkeletonPointToColorImage(SkeletonPoint SkelPoint, ColorImageFormat colForm)
        {
            return ks.CoordinateMapper.MapSkeletonPointToColorPoint(SkelPoint, colForm);
        }

        public void inititalize(int framePixelDataLength, KinectSensor ksensor, int height, int width)
        {
            colorPixels = new byte[framePixelDataLength];
            ks = ksensor;
            this.frameWidth = width;
            this.frameHeight = height;
            boolInitialized = true;
        }

        private static Boolean compareIsSame(Joint jointtoBeCompared, JointOrientationCharacteristics jointIdeal)
        {
            //evaluate X position of joint
            if (jointIdeal.xCompared == true)
            {
                //evaluates true if the x position is less than the max and greater than the min
                if (jointtoBeCompared.Position.X < jointIdeal.jointXPosition + jointIdeal.acceptableError & jointtoBeCompared.Position.X > jointIdeal.jointXPosition - jointIdeal.acceptableError)
                {
                    //return true;
                }
                else
                {
                    return false;
                }
            }
            //evaluate Y position
            if (jointIdeal.yCompared == true)
            {
                //evaluates true if the y position is less than the max and greater than the min
                if (jointtoBeCompared.Position.Y < jointIdeal.jointYPosition + jointIdeal.acceptableError & jointtoBeCompared.Position.Y > jointIdeal.jointYPosition - jointIdeal.acceptableError)
                {
                    //return true;
                }
                else
                {
                    return false;
                }
            }
            //evaluate Z position
            if (jointIdeal.zCompared == true)
            {
                //evaluates true if the z position is less than the max and greater than the min
                if (jointtoBeCompared.Position.Z < jointIdeal.jointZPosition + jointIdeal.acceptableError & jointtoBeCompared.Position.Z > jointIdeal.jointZPosition - jointIdeal.acceptableError)
                {
                    //return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        //Takes a skeleton frame and an activity step, and checks if it is correct
        public Boolean IsSkeletonMatch(SkeletonFrame skelFrame, Kinect.ActivityStep currentStep)
        {
            if (currentStep == null)
            {
                return false;
            }

            Skeletons = new Skeleton[skelFrame.SkeletonArrayLength];
            skelFrame.CopySkeletonDataTo(Skeletons);
            //flag, so that we can return false if no skeleton compared
            bool isGoodJoint = false;
            //flag to set as false if a joint comparison isn't run

            foreach (Skeleton Skel in Skeletons)
            {
                if (Skel.TrackingState == SkeletonTrackingState.NotTracked || Skel.TrackingState == SkeletonTrackingState.PositionOnly)
                {
                    continue;
                }

                if (currentStep.usesNewRotationFormat)
                {
                    foreach (Kinect.BoneOrientationCharacteristics boneIdealBone in currentStep.BoneComparison) //new format
                    {
                        if (boneIdealBone == null)
                        {
                            continue;
                        }
                        Joint jnt1 = new Joint();
                        Joint jnt2 = new Joint();
                        foreach (Joint skelJoint in Skel.Joints)
                        {
                            if (skelJoint.JointType == boneIdealBone.FirstJoint)
                            {
                                jnt1 = skelJoint;
                            }
                            if (skelJoint.JointType == boneIdealBone.SecondJoint)
                            {
                                jnt2 = skelJoint;
                            }
                        }
                        boneIdealBone.tempMatched = boneIdealBone.boolBoneOrientationIsMatch(jnt1, jnt2);
                    }
                    foreach (Kinect.BoneOrientationCharacteristics boneIdealBone in currentStep.BoneComparison)
                    {
                        if (boneIdealBone == null)
                        {
                            continue;
                        }
                        if (boneIdealBone.tempMatched)
                        {
                            isGoodJoint = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else //Old format comparison
                {
                    bool jointflag = false;
                    foreach (Kinect.JointOrientationCharacteristics jntIdealJoint in currentStep.JointComparison)
                    {
                        if (jntIdealJoint != null)
                        {
                            foreach (Joint skelJoint in Skel.Joints)
                            {
                                isGoodJoint = true;
                                if (skelJoint.JointType == jntIdealJoint.JointTypeID)
                                {
                                    jointflag = true;
                                    //run comparison and stop comparing if
                                    if (compareIsSame(skelJoint, jntIdealJoint) == false)
                                    {
                                        //stop checking through bones, comparison doesn't work
                                        return false;
                                    }
                                    break;
                                }
                            }
                            if (jointflag == false)
                            {
                                //returns false if not every joint has been available for comparison
                                return false;
                            }
                        }
                    }
                }
            }

            //return true if comparison run and no false joints found
            if (isGoodJoint == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Boolean isSkelMatchTimed(SkeletonFrame skelFrame, Kinect.ActivityStep currentStep)
        {
            if (IsSkeletonMatch(skelFrame, currentStep) == true)
            {
                if (currentStep.IsHoldInProgress() == true)
                {
                    if (currentStep.isHeldLongEnough() == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    currentStep.SetHoldTimer();
                }
            }
            else
            {
                currentStep.ClearHoldTimer();
            }
            return false;
        }
    }
}