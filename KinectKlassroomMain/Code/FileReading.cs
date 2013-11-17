using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace SeniorProject.Helpers
{
    public static class FileReading
    {
        private enum VersionType { V1SeniorProjectDemo, VSprint2 };

        public static Kinect.Activity ActivityFromText(string location)
        {
            Kinect.Activity newActivityFromText = new Kinect.Activity();
            newActivityFromText.activityStepArray = new List<Kinect.ActivityStep>();
            Kinect.ActivityStep newActivityStep = new Kinect.ActivityStep();
            List<string> strListLines = new List<string>();
            using (StreamReader reader = new StreamReader(location))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    strListLines.Add(line); // Add to list.
                }
            }
            VersionType ActivityVersion = new VersionType();
            bool boolInStep = false;
            bool boolFirstDoc = true;
            bool boolInDoc = false;
            bool boolInJoint = false;
            Kinect.JointOrientationCharacteristics JointCompared = new Kinect.JointOrientationCharacteristics();
            Kinect.BoneOrientationCharacteristics BoneCompared = new Kinect.BoneOrientationCharacteristics();
            string XAMLString = "";
            foreach (string strLine in strListLines)
            {
                if (strLine.StartsWith("Version-Number:"))
                {
                    if (strLine.Substring(15).Trim() == "0.0.2")
                    {
                        ActivityVersion = VersionType.VSprint2;
                    }
                }

                if (strLine.StartsWith("ActivityName: -"))
                {
                    string strName = strLine.Substring(13).TrimEnd(Convert.ToChar("-")).Trim();
                    newActivityFromText.activityName = strName;
                }
                //Begin a new step
                if (strLine.StartsWith("Step#: "))
                {
                    boolInStep = true;
                    newActivityStep = new Kinect.ActivityStep();
                }
                //If already in a step, place all new commands in the current step
                if (boolInStep == true)
                {
                    if (strLine.StartsWith("TimerUsed:")) //deal with the timer
                    {
                        if (strLine.StartsWith("TimerUsed: N")) //no timer
                        {
                            newActivityStep.isTimerUsed = false;
                            newActivityStep.MSTimer = 0;
                        }
                        else
                        {
                            string strtime = strLine.Substring(15);
                            strtime = strtime.TrimStart(Convert.ToChar("#"));
                            strtime = strtime.TrimEnd(Convert.ToChar("#"));
                            strtime = strtime.Trim();
                            newActivityStep.isTimerUsed = true;
                            newActivityStep.MSTimer = Convert.ToInt32(strtime);
                        }
                    }
                    if (strLine.StartsWith("ScoreValue:")) // deal with scoring
                    {
                    }
                    if (strLine.StartsWith("FlowDocCorrect: -")) //deal with correct flow doc
                    {
                        boolInDoc = true;
                        XAMLString = "";
                    }
                    if (strLine.StartsWith("EndFlowDocCorrect: -")) //add flowdoc to activity
                    {
                        boolInDoc = false;

                        using (StringReader stringReader = new StringReader(XAMLString))
                        {
                            XmlReader xmlReader = XmlReader.Create(stringReader);
                            FlowDocument readerLoadFlow = new FlowDocument();
                            readerLoadFlow.Blocks.Add((Section)XamlReader.Load(xmlReader));
                            newActivityStep.activityTextCorrect = readerLoadFlow;
                            XAMLString = "";
                            boolFirstDoc = true;
                        }
                    }
                    if (strLine.StartsWith("FlowDocIncorrect: -")) //deal with incorrect flow doc
                    {
                        boolInDoc = true;
                        XAMLString = "";
                    }
                    if (strLine.StartsWith("EndFlowDocIncorrect: -")) //add incorrect to activity
                    {
                        boolInDoc = false;
                        using (StringReader stringReader = new StringReader(XAMLString))
                        {
                            XmlReader xmlReader = XmlReader.Create(stringReader);
                            FlowDocument readerLoadFlow = new FlowDocument();
                            readerLoadFlow.Blocks.Add((Section)XamlReader.Load(xmlReader));
                            newActivityStep.activityTextWrong = readerLoadFlow;
                            XAMLString = "";
                            boolFirstDoc = true;
                        }
                    }
                    if (strLine.StartsWith("FlowDocDefault: -")) //deal with default flow doc
                    {
                        boolInDoc = true;
                        XAMLString = "";
                    }
                    if (strLine.StartsWith("EndFlowDocDefault: -")) //add default to activity
                    {
                        boolFirstDoc = true;
                        boolInDoc = false;
                        using (StringReader stringReader = new StringReader(XAMLString))
                        {
                            XmlReader xmlReader = XmlReader.Create(stringReader);
                            FlowDocument readerLoadFlow = new FlowDocument();
                            readerLoadFlow.Blocks.Add((Section)XamlReader.Load(xmlReader));
                            newActivityStep.activityTextDefault = readerLoadFlow;
                            XAMLString = "";
                        }
                    }
                    if (boolInDoc == true)
                    {
                        XAMLString = XAMLString + strLine;
                        if (boolFirstDoc == true)
                        {
                            if (XAMLString.StartsWith("FlowDocDefault: -"))
                            {
                                XAMLString = XAMLString.Substring(17);
                            }
                            if (XAMLString.StartsWith("FlowDocIncorrect: -"))
                            {
                                XAMLString = XAMLString.Substring(19);
                            }
                            if (XAMLString.StartsWith("FlowDocCorrect: -"))
                            {
                                XAMLString = XAMLString.Substring(17);
                            }
                            boolFirstDoc = false;
                        }
                    }
                    if (strLine.StartsWith("JointComparison - ") || strLine.StartsWith("BoneComparison - "))//Deal with Joint Comparison
                    {
                        boolInJoint = true;
                        continue;
                    }
                    if (boolInJoint == true) //Add characteristics to joint orientation comparison
                    {
                        if (ActivityVersion == VersionType.VSprint2)
                        {
                            if (strLine.StartsWith("Joint1"))
                            {
                                BoneCompared.FirstJoint = jntJointTypeFromString(strLine, 10);
                            }
                            if (strLine.StartsWith("Joint2"))
                            {
                                BoneCompared.SecondJoint = jntJointTypeFromString(strLine, 10);
                            }
                            if (strLine.StartsWith("Slope"))
                            {
                                BoneCompared.slopeYX = SlopeFromString(strLine);
                            }
                            if (strLine.StartsWith("ErrorRange"))
                            {
                                BoneCompared.acceptableError = ErrorFromString(strLine);
                            }
                        }
                        else
                        {
                            if (strLine.StartsWith("XCompare: Y"))
                            {
                                JointCompared.xCompared = true;
                                JointCompared.jointXPosition = PositionFromString(strLine);
                            }
                            if (strLine.StartsWith("XCompare: N"))
                            {
                                JointCompared.xCompared = false;
                                JointCompared.jointXPosition = 0f;
                            }
                            if (strLine.StartsWith("YCompare: Y"))
                            {
                                JointCompared.yCompared = true;
                                JointCompared.jointYPosition = PositionFromString(strLine);
                            }
                            if (strLine.StartsWith("YCompare: N"))
                            {
                                JointCompared.yCompared = false;
                                JointCompared.jointYPosition = 0f;
                            }
                            if (strLine.StartsWith("ZCompare: Y"))
                            {
                                JointCompared.zCompared = true;
                                JointCompared.jointZPosition = PositionFromString(strLine);
                            }

                            if (strLine.StartsWith("ZCompare: N"))
                            {
                                JointCompared.zCompared = false;
                                JointCompared.jointZPosition = 0f;
                            }
                            if (strLine.StartsWith("Joint Type: - "))
                            {
                                JointCompared.JointTypeID = jntJointTypeFromString(strLine, 15);
                            }
                            if (strLine.StartsWith("ErrorRange: - #"))
                            {
                                JointCompared.acceptableError = ErrorFromString(strLine);
                            }
                        }
                    }
                    if (strLine.StartsWith("EndJoint: - ")) // submit the joint
                    {
                        int intLength = 0;
                        boolInJoint = false;
                        try
                        {
                            intLength = newActivityStep.JointComparison.Length - 1;
                        }
                        catch (Exception ex)
                        {
                            Errors.WriteError(ex, "index array too small");
                            newActivityStep.JointComparison = new Kinect.JointOrientationCharacteristics[1];
                        }
                        Array.Resize(ref newActivityStep.JointComparison, intLength + 2); //account for zero-index and add an extra slot
                        if (intLength >= 0)
                        {
                            newActivityStep.JointComparison[intLength] = JointCompared;
                        }
                        else
                        {
                            newActivityStep.JointComparison[0] = JointCompared;
                        }
                        JointCompared = new Kinect.JointOrientationCharacteristics();
                    }
                    if (strLine.EndsWith("EndBone: - "))
                    {
                        int intLength = 0;
                        boolInJoint = false;
                        try
                        {
                            intLength = newActivityStep.BoneComparison.Length - 1;
                        }
                        catch (Exception ex)
                        {
                            Errors.WriteError(ex, "index array too small");
                            newActivityStep.BoneComparison = new Kinect.BoneOrientationCharacteristics[1];
                        }
                        Array.Resize(ref newActivityStep.BoneComparison, intLength + 2);
                        if (intLength >= 0)
                        {
                            newActivityStep.BoneComparison[intLength] = BoneCompared;
                        }
                        else
                        {
                            newActivityStep.BoneComparison[0] = BoneCompared;
                        }
                        BoneCompared = new Kinect.BoneOrientationCharacteristics();
                    }
                }
                //end the current step, add it to the activity

                if (strLine.EndsWith("EndStep: -"))
                {
                    int intLength = 0;
                    if (ActivityVersion == VersionType.VSprint2)
                    {
                        newActivityStep.usesNewRotationFormat = true;
                    }

                    boolInStep = false;
                    if (newActivityStep.activityTextDefault == null)
                    {
                        continue;
                    }
                    newActivityFromText.activityStepArray.Add(newActivityStep);
                    newActivityStep = new Kinect.ActivityStep();
                }
            }
            if (newActivityFromText == null || newActivityFromText.activityStepArray == null || newActivityFromText.activityStepArray.Count < 1)
            {
                throw new FormatException("Activity Contains No Steps");
            }

            return newActivityFromText;
        }

        private static float SlopeFromString(string strLine)
        {
            string strSlope = strLine.Substring(9);
            strSlope = strSlope.TrimEnd(' ');
            strSlope = strSlope.TrimEnd(Convert.ToChar("#"));
            strSlope = strSlope.TrimStart(Convert.ToChar("#"));
            return (float)Convert.ToDouble(strSlope);
        }

        private static float PositionFromString(string strLine)
        {
            string strPos = strLine.Substring(14);
            strPos = strPos.TrimEnd(' ');
            strPos = strPos.TrimEnd(Convert.ToChar("#"));
            strPos = strPos.TrimStart(Convert.ToChar("#"));
            return (float)Convert.ToDouble(strPos);
        }

        private static float ErrorFromString(string strLine)
        {
            string strError = "";
            strError = strLine.Substring(14);
            strError = strError.TrimEnd(Convert.ToChar("#"));
            strError = strError.TrimStart(Convert.ToChar("#"));
            return (float)Convert.ToDouble(strError);
        }

        private static JointType jntJointTypeFromString(string strLine, int charPos)
        {
            string strJointID = strLine.Substring(charPos).TrimEnd(Convert.ToChar("#")).TrimStart('#').Trim();
            switch (strJointID)
            {
                case "Head":
                    return JointType.Head;

                case "AnkleLeft":
                    return JointType.AnkleLeft;

                case "AnkleRight":
                    return JointType.AnkleRight;

                case "ElbowLeft":
                    return JointType.ElbowLeft;

                case "ElbowRight":
                    return JointType.ElbowRight;

                case "FootLeft":
                    return JointType.FootLeft;

                case "FootRight":
                    return JointType.FootRight;

                case "HandLeft":
                    return JointType.HandLeft;

                case "HandRight":
                    return JointType.HandRight;

                case "HipCenter":
                    return JointType.HipCenter;

                case "HipLeft":
                    return JointType.HipLeft;

                case "HipRight":
                    return JointType.HipRight;

                case "KneeLeft":
                    return JointType.KneeLeft;

                case "KneeRight":
                    return JointType.KneeRight;

                case "ShoulderCenter":
                    return JointType.ShoulderCenter;

                case "ShoulderLeft":
                    return JointType.ShoulderLeft;

                case "ShoulderRight":
                    return JointType.ShoulderRight;

                case "Spine":
                    return JointType.Spine;

                case "WristLeft":
                    return JointType.WristLeft;

                case "WristRight":
                    return JointType.WristRight;

                default:
                    return JointType.AnkleLeft;
            }
        }

        public static void WriteActivityFile(Kinect.Activity activityToWrite, string fileLocation)
        {
            // create a writer and open the file
            using (TextWriter tw = new StreamWriter(fileLocation))
            {
                String strTextToWrite;
                //Write Copyright and Version Info
                strTextToWrite = "Kinect Classroom - Nathan C. Castle - Copyright 2013" + Environment.NewLine +
                    "Version-Type: Pre-Release" + Environment.NewLine + "Version-Number: 0.0.1" + Environment.NewLine +
                    "Warning: Modifying this document may prevent it from being used by Kinected Classroom. Modify at own risk.";
                tw.Write(strTextToWrite);

                //Parse Through Activity and write information about the activity
                String strActivityName = activityToWrite.activityName;
                tw.WriteLine("ActivityName: -" + strActivityName + "- ");

                //Go Through Each Step and Write Info
                int intloop = 0;
                foreach (Kinect.ActivityStep i in activityToWrite.activityStepArray)
                {
                    if (activityToWrite.activityStepArray[intloop] == null)
                    {
                        //Don't write if the step is null
                    }
                    else
                    {
                        //Write step #
                        tw.WriteLine("Step#: " + intloop);
                        if (activityToWrite.activityStepArray[intloop].isTimerUsed == true)
                        {
                            tw.WriteLine("TimerUsed: Y - " + activityToWrite.activityStepArray[intloop].MSTimer.ToString() + "#");
                        }
                        else
                        {
                            tw.WriteLine("TimerUsed: N");
                        }

                        tw.WriteLine("ScoreValue: " + activityToWrite.activityStepArray[intloop].scoreValue.ToString());
                        //Writing the flowdocument for correctness
                        tw.WriteLine("FlowDocCorrect: -");
                        TextRange textRange = new TextRange(activityToWrite.activityStepArray[intloop].activityTextCorrect.ContentStart,
                        activityToWrite.activityStepArray[intloop].activityTextCorrect.ContentEnd);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            textRange.Save(ms, DataFormats.Xaml);
                            string xamlString = ASCIIEncoding.Default.GetString(ms.ToArray());
                            tw.WriteLine(xamlString + Environment.NewLine + "EndFlowDocCorrect: -");
                            //Flowdoc wrong
                            tw.WriteLine("FlowDocIncorrect: -");
                            textRange = new TextRange(activityToWrite.activityStepArray[intloop].activityTextWrong.ContentStart,
                            activityToWrite.activityStepArray[intloop].activityTextWrong.ContentEnd);
                        }
                        using (MemoryStream ms = new MemoryStream())
                        {
                            textRange.Save(ms, DataFormats.Xaml);
                            string xamlString = ASCIIEncoding.Default.GetString(ms.ToArray());
                            tw.WriteLine(xamlString + Environment.NewLine + "EndFlowDocIncorrect: -");
                            //Flowdocdefault
                            tw.WriteLine("FlowDocDefault: -");
                            textRange = new TextRange(activityToWrite.activityStepArray[intloop].activityTextDefault.ContentStart,
                            activityToWrite.activityStepArray[intloop].activityTextDefault.ContentEnd);
                        }

                        using (MemoryStream ms = new MemoryStream())
                        {
                            textRange.Save(ms, DataFormats.Xaml);
                            string xamlString = ASCIIEncoding.Default.GetString(ms.ToArray());
                            tw.WriteLine(xamlString + Environment.NewLine + "EndFlowDocDefault: -");
                        }
                        foreach (Kinect.JointOrientationCharacteristics j in activityToWrite.activityStepArray[intloop].JointComparison)
                        {
                            tw.WriteLine("JointComparison - ");
                            //dimensions of comparisons
                            try
                            {
                                if (j.xCompared)
                                {
                                    tw.WriteLine("XCompare: Y - #" + j.jointXPosition.ToString() + "#");
                                }
                                else
                                {
                                    tw.WriteLine("XCompare: N - ");
                                }
                                if (j.zCompared)
                                {
                                    tw.WriteLine("ZCompare: Y - #" + j.jointZPosition.ToString() + "#");
                                }
                                else
                                {
                                    tw.WriteLine("ZCompare: N - ");
                                }
                                if (j.yCompared)
                                {
                                    tw.WriteLine("YCompare: Y - #" + j.jointYPosition.ToString() + "#");
                                }
                                else
                                {
                                    tw.WriteLine("YCompare: N - ");
                                }

                                //joint type
                                tw.WriteLine("Joint Type: - #" + j.JointTypeID.ToString() + "#");
                                //
                                tw.WriteLine("ErrorRange: - #" + j.acceptableError.ToString() + "#");
                                //End Joint
                                tw.WriteLine("EndJoint: - ");
                                tw.WriteLine("EndJointComparison - ");
                            }
                            catch (Exception ex)
                            {
                                tw.Write("XCompare: N - " + Environment.NewLine + "ZCompare: N - " + Environment.NewLine +
                                    "YCompare: N - " + Environment.NewLine + "Joint Type: - #Head#" + Environment.NewLine +
                                    "ErrorRange: - #.1#" + Environment.NewLine + "EndJoint: - " + Environment.NewLine + "EndJointComparison - ");
                                Errors.WriteError(ex, "Writing Joint Comparison to file");
                            }
                        }
                        tw.WriteLine("EndStep: -");
                    }
                    intloop++;
                }
            }
        }
    }
}