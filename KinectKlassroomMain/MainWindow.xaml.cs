namespace KinectedClassroom
{
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Diagnostics;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    ///
    ///

    public partial class MainWindow
    {
        private KinectSensorChooser sensorChooser;
        public SeniorProject.Kinect.Activity actCurrentActivity;
        private SeniorProject.Kinect.FrameProcessors frameProcessor;
        private string ActivityUserName;
        private SeniorProject.Kinect.ActivityRecordContainer ActivityRecorder;
        private DateTime ActivityStepBegin;

        private void SensorFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame sFrame = e.OpenSkeletonFrame())
            {
                //run image code
                using (ColorImageFrame cFrame = e.OpenColorImageFrame())
                {
                    if (cFrame == null)
                    {
                        return;
                    }

                    bmpImage.Source = frameProcessor.bmpFromColorFrame(cFrame, true, sFrame);
                }
                //run activity code, if appropriate

                if (actCurrentActivity.isLoaded == false || actCurrentActivity.isPaused == true)
                {
                    return; // don't do anything
                }
                //Skip the activity steps if there isn't a loaded activity

                actCurrentActivity.HandleNullActivity();

                //first loop steps
                if (actCurrentActivity.StepCurrent().isFirstRun())
                {
                    // The Work to perform on another thread
                    ActivityTextUpdate(actCurrentActivity.StepCurrent().activityTextDefault);
                    actCurrentActivity.SetupCurrentStep();
                    ActivityStepBegin = System.DateTime.Now;
                }

                //show expired timer if necessary
                if (actCurrentActivity.IsTimerExpired())
                {
                    ActivityTextUpdate(actCurrentActivity.StepCurrent().activityTextWrong);
                    OverlayUpdate(OverlayImages.Crossbuck);
                    ActivityRecorder.AddNewActivityEventWithStart(SeniorProject.Kinect.ActivityEvents.TimeOutStep, ActivityUserName, ActivityStepBegin);

                    return;
                }
                else if (actCurrentActivity.StepCurrent().isTimerUsed == true)
                {
                    lblTimer.Content = Convert.ToString(Convert.ToInt32((actCurrentActivity.StepCurrent().timeEnd - System.DateTime.Now).TotalSeconds)) + "s";
                }

                //iterate through bones and run comparisons. If the player has completed the step, move display correct text and add up score. Iterate to next step.

                if (frameProcessor.isSkelMatchTimed(sFrame, actCurrentActivity.StepCurrent()))
                {
                    //Skeleton Matched, update score and move to next step
                    ActivityTextUpdate(actCurrentActivity.StepCurrent().activityTextCorrect);
                    OverlayUpdate(OverlayImages.Check);
                    actCurrentActivity.IncrementScore(); //update score
                    lblScore.Content = "Score: " + (int)actCurrentActivity.currentScore;
                    ActivityRecorder.AddNewActivityEventWithStart(SeniorProject.Kinect.ActivityEvents.PassStep, ActivityUserName, ActivityStepBegin);
                    actCurrentActivity.isPaused = true;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            SetColors();
            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();


            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new System.Windows.Data.Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            //Register events
            if (this.sensorChooser.Kinect != null)
            {
                this.sensorChooser.Kinect.AllFramesReady += this.SensorFramesReady;
            }
            frameProcessor = new SeniorProject.Kinect.FrameProcessors();
            try
            {
                frameProcessor.inititalize(this.sensorChooser.Kinect.ColorStream.FramePixelDataLength, this.sensorChooser.Kinect, this.sensorChooser.Kinect.ColorStream.FrameHeight, this.sensorChooser.Kinect.ColorStream.FrameWidth);
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("No sensor attached or sensor issue. Sorry. :(");
                System.Environment.Exit(0);
            }

            //load empty activity
            actCurrentActivity = new SeniorProject.Kinect.Activity();
            actCurrentActivity.isLoaded = false;
            ActivityUserName = "Default User";
            ActivityRecorder = new SeniorProject.Kinect.ActivityRecordContainer();
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException ex)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    SeniorProject.Helpers.Errors.WriteError(ex, "Old Kinect invalid state, on sensor changed");
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable(new TransformSmoothParameters { Smoothing = .5F, JitterRadius = .2F, Prediction = 0F });
                }
                catch (InvalidOperationException ex)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    SeniorProject.Helpers.Errors.WriteError(ex, "New Kinect invalid state, on sensor changed");
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //cleanup code to ensure smooth shutdown
            try
            {
                this.sensorChooser.Kinect.AllFramesReady -= this.SensorFramesReady;
            }
            catch (NullReferenceException ex)
            {
                //The program has exited without starting a Kinect. Handle it.
                SeniorProject.Helpers.Errors.WriteError(ex);
            }
            this.sensorChooser.Stop();
        }

        /// <summary>
        /// Handle a button click from the wrap panel.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void KinectButtonClose(object sender, RoutedEventArgs e)
        {
            SetGrid(Screens.Closing);
        }

        /// <summary>
        /// Handle a click of the file button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KTFileClick(object sender, System.Windows.RoutedEventArgs e)
        {
            SetGrid(Screens.File);
            ActivityRecorder.AddNewUIEvent(SeniorProject.Kinect.UIEvents.File, ActivityUserName);
        }

        private void KTHome(object sender, System.Windows.RoutedEventArgs e)
        {
            SetGrid(Screens.Home);
            ActivityRecorder.AddNewUIEvent(SeniorProject.Kinect.UIEvents.Home, ActivityUserName);
        }

        private void KTPrev(object sender, System.Windows.RoutedEventArgs e)
        {
            actCurrentActivity.DecrementStep();
            OverlayUpdate(OverlayImages.ClearAll);
            ActivityRecorder.AddNewUIEvent(SeniorProject.Kinect.UIEvents.PreviousStep, ActivityUserName);
        }

        private void KTPause(object sender, System.Windows.RoutedEventArgs e)
        {
            if (actCurrentActivity.isLoaded == true)
            {
                actCurrentActivity.TogglePause();
                if ((string)btnActPause.Content == "Pause")
                {
                    btnActPause.Content = "Continue";
                    ActivityRecorder.AddNewUIEvent(SeniorProject.Kinect.UIEvents.Pause, ActivityUserName);
                }
                else
                {
                    btnActPause.Content = "Pause";
                    ActivityRecorder.AddNewUIEvent(SeniorProject.Kinect.UIEvents.Continue, ActivityUserName);
                }
            }
        }

        private void KTNext(object sender, System.Windows.RoutedEventArgs e)
        {
            if (actCurrentActivity.isLoaded)
            {
                actCurrentActivity.IncrementStep();
            }
            OverlayUpdate(OverlayImages.ClearAll);
            ActivityRecorder.AddNewUIEvent(SeniorProject.Kinect.UIEvents.NextStep, ActivityUserName);
        }

        private void KTLoadActivity(object sender, System.Windows.RoutedEventArgs e)
        {
            using (OpenFileDialog LoadActivityDialog = new OpenFileDialog())
            {
                string strFilePath;
                LoadActivityDialog.InitialDirectory = "";
                LoadActivityDialog.Filter = "Activity Files (.kka)|*.kka";
                LoadActivityDialog.FilterIndex = 2;
                LoadActivityDialog.RestoreDirectory = true;

                if (LoadActivityDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    strFilePath = LoadActivityDialog.FileName;
                    if (strFilePath != null)
                    {
                        try
                        {
                            actCurrentActivity = SeniorProject.Helpers.FileReading.ActivityFromText(strFilePath);
                            actCurrentActivity.isLoaded = true;

                            SetGrid(Screens.Home);
                            lblScore.Content = "Score: 0";
                            ActivityRecorder = new SeniorProject.Kinect.ActivityRecordContainer();
                        }
                        catch (FormatException)
                        {
                            System.Windows.MessageBox.Show("Invalid File, please select another activity.", "File Read Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        //make sure the activity isn't empty
                    }
                }
            }
        }

        private void KTSave(object sender, System.Windows.RoutedEventArgs e)
        {
            using (SaveFileDialog SaveLogDialog = new SaveFileDialog())
            {
                SaveLogDialog.Filter = "Kinected Classroom Record|*.kkr";
                SaveLogDialog.Title = "Save Your Progress";
                SaveLogDialog.ShowDialog();
                if (!String.IsNullOrWhiteSpace(SaveLogDialog.FileName))
                {
                    ActivityRecorder.WriteToFile(SaveLogDialog.FileName);
                }
                SetGrid(Screens.Home);
            }
        }

        public void ActivityTextUpdate(FlowDocument flowActivityText)
        {
            flowText.Document = flowActivityText;
        }

        public void TimerTextUpdate(string timerText)
        {
            lblTimer.Content = timerText;
        }

        private enum OverlayImages { Timer, Check, Crossbuck, ClearAll };

        private void OverlayUpdate(OverlayImages Resource)
        {
            BitmapImage overlay = new BitmapImage();
            overlay.BeginInit();
            switch (Resource)
            {
                case OverlayImages.Check:
                    overlay.UriSource = new Uri(("/Images/Check.png"), UriKind.Relative);
                    break;

                case OverlayImages.Crossbuck:
                    overlay.UriSource = new Uri(("/Images/Wrong.png"), UriKind.Relative);
                    break;

                case OverlayImages.Timer:
                    overlay.UriSource = new Uri(("/Images/Stopwatch.png"), UriKind.Relative);
                    break;

                case OverlayImages.ClearAll:
                    bmpActivityOverlay.Visibility = System.Windows.Visibility.Hidden;

                    return;
            }

            overlay.EndInit();
            bmpActivityOverlay.Source = overlay;
            bmpActivityOverlay.Visibility = System.Windows.Visibility.Visible;
        }

        private void KTCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void KTReturnClick(object sender, RoutedEventArgs e)
        {
            SetGrid(Screens.Home);
        }

        /// <summary>
        /// Enumeration of the various "screens" that can be presented to the user.
        /// </summary>
        private enum Screens { File, Closing, Activity, Home, Intro, Help, Options, About, OptionsTilt, HelpDocs, Theming };

        private string ResourceKeyStanardButton = "KKLabelButtonPurple";
        private string ResourceKeyActiveButton = "KKLabelButtonActivePurple";

        /// <summary>
        /// Used to handle all operations necessary to show the user a particular "screen".
        /// </summary>
        /// <param name="scrChoice"></param>
        private void SetGrid(Screens scrChoice)
        {
            //hid all screens, except for menu, then display appropriate screen
            wrpFileMenu.Visibility = System.Windows.Visibility.Visible;
            grdAbout.Visibility = System.Windows.Visibility.Collapsed;
            grdTilt.Visibility = System.Windows.Visibility.Collapsed;
            grdHelpHelp.Visibility = System.Windows.Visibility.Collapsed;
            grdClose.Visibility = System.Windows.Visibility.Collapsed;
            grdHome.Visibility = System.Windows.Visibility.Collapsed;
            grdHelp.Visibility = System.Windows.Visibility.Collapsed;
            grdOptions.Visibility = System.Windows.Visibility.Collapsed;
            grdActivity.Visibility = System.Windows.Visibility.Collapsed;
            grdOptionsTheme.Visibility = System.Windows.Visibility.Collapsed;
            btnFile.Style = (Style)FindResource(ResourceKeyStanardButton);
            btnHelp.Style = (Style)FindResource(ResourceKeyStanardButton);
            btnAbout.Style = (Style)FindResource(ResourceKeyStanardButton);
            btnOptions.Style = (Style)FindResource(ResourceKeyStanardButton);
            if (scrChoice != Screens.File)
            {
                grdFile.Visibility = System.Windows.Visibility.Collapsed;
            }

            switch (scrChoice)
            {
                case Screens.Activity:
                    grdActivity.Visibility = System.Windows.Visibility.Visible;

                    return;

                case Screens.Closing:
                    grdClose.Visibility = System.Windows.Visibility.Visible;
                    return;

                case Screens.File:

                    btnFile.Style = (Style)FindResource(ResourceKeyActiveButton);

                    if (grdFile.Visibility == System.Windows.Visibility.Visible)
                    {
                        grdFile.Visibility = System.Windows.Visibility.Collapsed;
                        grdHome.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        grdFile.Visibility = System.Windows.Visibility.Visible;
                    }

                    return;

                case Screens.Help:
                    btnHelp.Style = (Style)FindResource(ResourceKeyActiveButton);
                    grdHelp.Visibility = System.Windows.Visibility.Visible;
                    return;

                case Screens.Home:
                    btnFile.Style = (Style)FindResource(ResourceKeyActiveButton);
                    grdHome.Visibility = System.Windows.Visibility.Visible;
                    return;

                case Screens.Intro:
                    //grdWizard
                    return;

                case Screens.About:
                    btnAbout.Style = (Style)FindResource(ResourceKeyActiveButton);
                    grdAbout.Visibility = System.Windows.Visibility.Visible;
                    return;

                case Screens.Options:
                    btnOptions.Style = (Style)FindResource(ResourceKeyActiveButton);
                    grdOptions.Visibility = System.Windows.Visibility.Visible;
                    return;

                case Screens.OptionsTilt:
                    btnOptions.Style = (Style)FindResource(ResourceKeyActiveButton);
                    grdTilt.Visibility = System.Windows.Visibility.Visible;
                    return;

                case Screens.HelpDocs:
                    btnHelp.Style = (Style)FindResource(ResourceKeyActiveButton);
                    grdHelpHelp.Visibility = System.Windows.Visibility.Visible;
                    return;

                case Screens.Theming:
                    btnOptions.Style = (Style)FindResource(ResourceKeyActiveButton);
                    grdOptionsTheme.Visibility = System.Windows.Visibility.Visible;
                    return;

                default:
                    throw new InvalidOperationException("Unable to process screen option. The inputted enum selection was not planned for when this void was last modified");
            }
        }

        private void KTOptions(object sender, RoutedEventArgs e)
        {
            SetGrid(Screens.Options);
        }

        private void KTHelp(object sender, RoutedEventArgs e)
        {
            SetGrid(Screens.Help);
            ActivityRecorder.AddNewUIEvent(SeniorProject.Kinect.UIEvents.Help, ActivityUserName);
        }

        private void KTAbout(object sender, RoutedEventArgs e)
        {
            SetGrid(Screens.About);
            ActivityRecorder.AddNewUIEvent(SeniorProject.Kinect.UIEvents.About, ActivityUserName);
        }

        private void KTOptionsTiltClick(object sender, RoutedEventArgs e)
        {
            SetGrid(Screens.OptionsTilt);
        }

        private void KTOptionsTiltUpClick(object sender, RoutedEventArgs e)
        {
            sensorChooser.Kinect.ElevationAngle = 15;
            SetGrid(Screens.Home);
        }

        private void KTOptionsTiltDownClick(object sender, RoutedEventArgs e)
        {
            sensorChooser.Kinect.ElevationAngle = -15;
            SetGrid(Screens.Home);
        }

        private void KTOptionsTiltCenterClick(object sender, RoutedEventArgs e)
        {
            sensorChooser.Kinect.ElevationAngle = 0;
            SetGrid(Screens.Home);
        }

        private void KTHelpHelpClick(object sender, RoutedEventArgs e)
        {
            SetGrid(Screens.HelpDocs);
        }

        private void OnDragMoveWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (System.InvalidOperationException)
            {
            }
        }

        private void KTOptionsThemeClick(object sender, RoutedEventArgs e)
        {
            SetGrid(Screens.Theming);
        }

        private void SetColors()
        {
            switch (KinectedClassroom.Properties.Settings.Default.ColorTheme)
            {
                case "Blue":
                    ResourceKeyStanardButton = "KKLabelButtonBlue";
                    ResourceKeyActiveButton = "KKLabelButtonActiveBlue";
                    btnActPrev.Style = (Style)FindResource("KKLabelButtonBlue");
                    btnActNext.Style = (Style)FindResource("KKLabelButtonBlue");
                    btnActPause.Style = (Style)FindResource("KKLabelButtonBlue");
                    KinectedClassRoomMain.Style = (Style)FindResource("KKWindowBlue");
                    grdAbout.Background = (Brush)FindResource("KKBlue");
                    grdTilt.Background = (Brush)FindResource("KKBlue");
                    grdHelpHelp.Background = (Brush)FindResource("KKBlue");
                    grdClose.Background = (Brush)FindResource("KKBlue");
                    grdHelp.Background = (Brush)FindResource("KKBlue");
                    grdOptions.Background = (Brush)FindResource("KKBlue");
                    grdActivity.Background = (Brush)FindResource("KKBlue");
                    grdOptionsTheme.Background = (Brush)FindResource("KKBlue");
                    grdFile.Background = (Brush)FindResource("KKBlue");
                    flowText.Background = (Brush)FindResource("GradientBrushBlue");

                    break;

                case "Yellow":
                    ResourceKeyStanardButton = "KKLabelButtonYellow";
                    ResourceKeyActiveButton = "KKLabelButtonActiveYellow";
                    btnActPrev.Style = (Style)FindResource("KKLabelButtonYellow");
                    btnActNext.Style = (Style)FindResource("KKLabelButtonYellow");
                    btnActPause.Style = (Style)FindResource("KKLabelButtonYellow");
                    KinectedClassRoomMain.Style = (Style)FindResource("KKWindowYellow");
                    grdAbout.Background = (Brush)FindResource("KKGrey");
                    grdTilt.Background = (Brush)FindResource("KKGrey");
                    grdHelpHelp.Background = (Brush)FindResource("KKGrey");
                    grdClose.Background = (Brush)FindResource("KKGrey");
                    grdHelp.Background = (Brush)FindResource("KKGrey");
                    grdOptions.Background = (Brush)FindResource("KKGrey");
                    grdActivity.Background = (Brush)FindResource("KKGrey");
                    grdOptionsTheme.Background = (Brush)FindResource("KKGrey");
                    grdFile.Background = (Brush)FindResource("KKGrey");
                    flowText.Background = (Brush)FindResource("GradientBrushYellow");
                    break;

                case "Purple":
                default:
                    ResourceKeyStanardButton = "KKLabelButtonPurple";
                    ResourceKeyActiveButton = "KKLabelButtonActivePurple";
                    btnActPrev.Style = (Style)FindResource("KKLabelButtonPurple");
                    btnActNext.Style = (Style)FindResource("KKLabelButtonPurple");
                    btnActPause.Style = (Style)FindResource("KKLabelButtonPurple");
                    KinectedClassRoomMain.Style = (Style)FindResource("KKWindowPurple");
                    grdAbout.Background = (Brush)FindResource("KKPurple");
                    grdTilt.Background = (Brush)FindResource("KKPurple");
                    grdHelpHelp.Background = (Brush)FindResource("KKPurple");
                    grdClose.Background = (Brush)FindResource("KKPurple");
                    grdHelp.Background = (Brush)FindResource("KKPurple");
                    grdOptions.Background = (Brush)FindResource("KKPurple");
                    grdActivity.Background = (Brush)FindResource("KKPurple");
                    grdOptionsTheme.Background = (Brush)FindResource("KKPurple");
                    grdFile.Background = (Brush)FindResource("KKPurple");
                    flowText.Background = (Brush)FindResource("GradientBrushPurple");
                    break;
            }
            SetGrid(Screens.Home);
        }

        private void KTOptionsThemeBlue(object sender, RoutedEventArgs e)
        {
            KinectedClassroom.Properties.Settings.Default.ColorTheme = "Blue";
            KinectedClassroom.Properties.Settings.Default.Save();
            SetColors();
        }

        private void KTOptionsThemeYellow(object sender, RoutedEventArgs e)
        {
            KinectedClassroom.Properties.Settings.Default.ColorTheme = "Yellow";
            KinectedClassroom.Properties.Settings.Default.Save();
            SetColors();
        }

        private void KTOptionsThemePurple(object sender, RoutedEventArgs e)
        {
            KinectedClassroom.Properties.Settings.Default.ColorTheme = "Purple";
            KinectedClassroom.Properties.Settings.Default.Save();
            SetColors();
        }

        private void KTNavWeb(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.nathanccastle.com");
        }

        private void KTNavGit(object sender, RoutedEventArgs e)
        {

        }
    }
}