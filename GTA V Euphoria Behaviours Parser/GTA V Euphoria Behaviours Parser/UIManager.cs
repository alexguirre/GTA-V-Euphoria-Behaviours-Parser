namespace GTA_V_Euphoria_Behaviours_Parser
{
    // System
    using System;
    using System.Windows.Threading;
    using System.ComponentModel;

    internal class UIManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow Window { get; }

        public Dispatcher Dispatcher { get { return Window.Dispatcher; } }

        private string windowTitle = "GTA V Euphoria Behaviours Parser";
        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                if(value != windowTitle)
                {
                    windowTitle = value;
                    RaisePropertyChanged(nameof(WindowTitle));
                }
            }
        }

        private string inputFile = "";
        public string InputFile
        {
            get { return inputFile; }
            set
            {
                if(value != inputFile)
                {
                    inputFile = value;
                    RaisePropertyChanged(nameof(InputFile));
                }
            }
        }

        private string outputFolder = "";
        public string OutputFolder
        {
            get { return outputFolder; }
            set
            {
                if (value != outputFolder)
                {
                    outputFolder = value;
                    RaisePropertyChanged(nameof(OutputFolder));
                }
            }
        }

        private bool visibilityPublic = false;
        public bool VisibilityPublic
        {
            get { return visibilityPublic; }
            set
            {
                if (value != visibilityPublic)
                {
                    visibilityPublic = value;
                    RaisePropertyChanged(nameof(VisibilityPublic));
                }
            }
        }

        private bool visibilityInternal = true;
        public bool VisibilityInternal
        {
            get { return visibilityInternal; }
            set
            {
                if (value != visibilityInternal)
                {
                    visibilityInternal = value;
                    RaisePropertyChanged(nameof(VisibilityInternal));
                }
            }
        }

        private bool visibilityProtected = false;
        public bool VisibilityProtected
        {
            get { return visibilityProtected; }
            set
            {
                if (value != visibilityProtected)
                {
                    visibilityProtected = value;
                    RaisePropertyChanged(nameof(VisibilityProtected));
                }
            }
        }

        private bool visibilityPrivate = false;
        public bool VisibilityPrivate
        {
            get { return visibilityPrivate; }
            set
            {
                if (value != visibilityPrivate)
                {
                    visibilityPrivate = value;
                    RaisePropertyChanged(nameof(VisibilityPrivate));
                }
            }
        }

        private string @namespace = "Rage.Euphoria";
        public string Namespace
        {
            get { return @namespace; }
            set
            {
                if (value != @namespace)
                {
                    @namespace = value;
                    RaisePropertyChanged(nameof(Namespace));
                }
            }
        }

        private string filename = "EuphoriaMessages.cs";
        public string FileName
        {
            get { return filename; }
            set
            {
                if (value != filename)
                {
                    filename = value;
                    RaisePropertyChanged(nameof(FileName));
                }
            }
        }

        private bool controlsEnabled = true;
        public bool ControlsEnabled
        {
            get { return controlsEnabled; }
            set
            {
                if (controlsEnabled != value)
                {
                    controlsEnabled = value;
                    RaisePropertyChanged(nameof(ControlsEnabled));
                }
            }
        }

        private string classesPrefix = "EuphoriaMessage";
        public string ClassesPrefix
        {
            get { return classesPrefix; }
            set
            {
                if (value != classesPrefix)
                {
                    classesPrefix = value;
                    RaisePropertyChanged(nameof(ClassesPrefix));
                }
            }
        }

        private string classesSuffix = "";
        public string ClassesSuffix
        {
            get { return classesSuffix; }
            set
            {
                if (value != classesSuffix)
                {
                    classesSuffix = value;
                    RaisePropertyChanged(nameof(ClassesSuffix));
                }
            }
        }

        public UIManager(MainWindow window)
        {
            Window = window;
        }

        public void LogLine(string line)
        {
            if (Window.CheckAccess())
            {
                Window.OutputBox.AppendText(line + "\n");
                Window.OutputBox.ScrollToEnd();
            }
            else
                Window.Dispatcher.Invoke(() => { LogLine(line); });
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


