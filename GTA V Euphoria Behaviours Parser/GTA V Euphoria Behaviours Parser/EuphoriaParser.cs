namespace GTA_V_Euphoria_Behaviours_Parser
{
    // System
    using System;
    using System.Windows;
    using System.Windows.Forms;
    using System.Reflection;
    using System.Globalization;
    using System.Diagnostics;
    using System.Threading;
    using System.Collections.Generic;
    using System.Xml;
    using System.IO;
    using System.Text;
    using MessageBox = System.Windows.MessageBox;

    // MS
    using Microsoft.Win32;

    internal class EuphoriaParser
    {
        public static EuphoriaParser Instance { get; private set; }
        public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        private string LogFileName { get; } = $"EuphoriaBehavioursParser_{DateTime.Now.ToShortDateString().Replace(CultureInfo.CurrentUICulture.DateTimeFormat.DateSeparator, "")}_{DateTime.Now.ToLongTimeString().Replace(CultureInfo.CurrentUICulture.DateTimeFormat.TimeSeparator, "")}.log";

        public MainWindow Window { get; }
        public UIManager UIManager { get; private set; }

        public Thread CurrentThread { get; set; }

        public EuphoriaParser(MainWindow window)
        {
            Instance = this;
            
            Window = window;

        }

        public void InitializeUI()
        {
            Window.Closing += OnWindowClosing;
            Window.InputFileBrowseButton.Click += InputFileBrowse;
            Window.OutputFolderBrowseButton.Click += OutputFolderBrowse;
            Window.ParseGenCodeButton.Click += ParseGenCodeClick;
            UIManager = new UIManager(Window);
            UIManager.WindowTitle += " v" + Version;

            Logger.FileName = LogFileName;
            Logger.Logged += (level, text, formattedText) => { UIManager.Dispatcher.Invoke(() => { UIManager.LogLine(formattedText); }); };

            Logger.Info("Loaded " + UIManager.WindowTitle);
            if(Logger.CanLogToFile)
                Logger.Info("Log file: " + Logger.FileName);
        }

        private void ParseGenCodeClick(object sender, RoutedEventArgs e)
        {
            if (UIManager.InputFile == null || UIManager.InputFile.Length == 0)
                MessageBox.Show(Window, "Please provide an input file", "Invalid settings", MessageBoxButton.OK, MessageBoxImage.Information);
            else if(UIManager.OutputFolder == null || UIManager.OutputFolder.Length == 0)
                MessageBox.Show(Window, "Please provide an output folder", "Invalid settings", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (UIManager.FileName == null || UIManager.FileName.Length == 0)
                MessageBox.Show(Window, "Please provide an output file name", "Invalid settings", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (UIManager.Namespace == null || UIManager.Namespace.Length == 0)
                MessageBox.Show(Window, "Please provide a namespace", "Invalid settings", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                if (!Directory.Exists(UIManager.OutputFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(UIManager.OutputFolder);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show(this.Window, "Please provide a valid output folder", "Invalid settings", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                UIManager.ControlsEnabled = false;
                CurrentThread = new Thread(ParseAndGenerateCode);
                CurrentThread.Start();
            }
        }

        private void ParseAndGenerateCode()
        {
            List<NMBehaviour> behaviours = ParseBehavioursFile(UIManager.InputFile);
            WriteBehavioursToFile(UIManager.OutputFolder, behaviours);
            UIManager.ControlsEnabled = true;
        }

        private void InputFileBrowse(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.AddExtension = true;
            fileDialog.Multiselect = false;
            fileDialog.DefaultExt = "XML Files (*.xml)|*.xml";
            fileDialog.Filter = "XML Files (*.xml)|*.xml";
            fileDialog.Title = "Please select the XML file to parse.";

            bool? result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                UIManager.InputFile = fileDialog.FileName;
                Logger.Info("Selected input file: " + fileDialog.FileName);
            }
        }


        private void OutputFolderBrowse(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            browser.ShowNewFolderButton = true;
            browser.Description = "Please select the output folder.";
            if (browser.ShowDialog() == DialogResult.OK)
            {
                UIManager.OutputFolder = browser.SelectedPath;
                Logger.Info("Selected output folder: " + browser.SelectedPath);
            }
        }


        public List<NMBehaviour> ParseBehavioursFile(string behavioursXmlFile)
        {
            Logger.Info("Started parsing euphoria behaviours from " + behavioursXmlFile);
            List<NMBehaviour> behaviours = new List<NMBehaviour>();

            XmlDocument doc = new XmlDocument();
            using (XmlReader reader = XmlReader.Create(behavioursXmlFile))
            {
                doc.Load(reader);

                XmlNodeList nodes = doc.DocumentElement.SelectNodes("/rage__NMBehaviorPool/behaviors/Item");

                NMBehaviour currentBehaviour;
                foreach (XmlNode node in nodes)
                {
                    currentBehaviour = new NMBehaviour();

                    // get name
                    currentBehaviour.Name = node.ChildNodes[0].InnerText;

                    Logger.Info("Found euphoria behaviour: " + currentBehaviour.Name);

                    // get description
                    if ((!String.IsNullOrEmpty(node.ChildNodes[1].InnerText) && !String.IsNullOrWhiteSpace(node.ChildNodes[1].InnerText)) && node.ChildNodes[1].InnerText != "... (check docs)")
                    {
                        currentBehaviour.Description = node.ChildNodes[1].InnerText;
                    }
                    else
                    {
                        currentBehaviour.Description = "";
                    }

                    List<NMParam> behaviourParams = new List<NMParam>();
                    NMParam currentParam;
                    foreach (XmlNode n in node.ChildNodes[2].ChildNodes)
                    {
                        currentParam = new NMParam();

                        // get param description
                        if (!String.IsNullOrEmpty(n["description"].InnerText) && !String.IsNullOrWhiteSpace(n["description"].InnerText))
                        {
                            currentParam.Description = n["description"].InnerText;
                        }
                        else
                        {
                            currentParam.Description = "";
                        }

                        // get param name
                        currentParam.Name = n["name"].InnerText;

                        // get param type
                        currentParam.Type = n["type"].InnerText;

                        // get param initial value
                        currentParam.InitialValue = n["init"].InnerText;

                        // get param min/max values
                        currentParam.MinValue = n["min"] == null ? "" : n["min"].Attributes["value"].Value;
                        currentParam.MaxValue = n["max"] == null ? "" : n["max"].Attributes["value"].Value;

                        behaviourParams.Add(currentParam);
                    }
                    currentBehaviour.Params = behaviourParams.ToArray();

                    behaviours.Add(currentBehaviour);
                }
            }

            Logger.Info("Finished parsing euphoria behaviours");
            return behaviours;
        }

        public void WriteBehavioursToFile(string outputFolder, List<NMBehaviour> behaviours)
        {
            string path = Path.Combine(outputFolder, UIManager.FileName);
            Logger.Info("Started writing euphoria behaviours to " + path);

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter w = new StreamWriter(File.Create(path)))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($@"// Generated by {UIManager.WindowTitle} by alexguirre
namespace {UIManager.Namespace}
{{
    using Rage;
    using Rage.Euphoria;");

                foreach (NMBehaviour behaviour in behaviours)
                {
                    string startText = $@"

    /// <summary>
    /// {behaviour.Description}
    /// </summary>
    {GetVisibilityText()} class {UIManager.ClassesPrefix}{behaviour.Name.FirstLetterToUpper()}{UIManager.ClassesSuffix} : EuphoriaMessage
    {{";
                    sb.AppendLine(startText);

                    string resetMethodText = $@"public new void Reset()
        {{
";

                    foreach (NMParam param in behaviour.Params)
                    {
                        string initValue = param.InitialValue.Replace("ART::kITSourceCurrent", "").Replace("ART::KITSourceCount-1", "");
                        if (param.Type == "vector3")
                        {
                            string[] values = initValue.Split(',');
                            initValue = $"new Vector3({values[0]}f, {values[1]}f, {values[2]}f)";
                        }
                        else if (param.Type == "string")
                        {
                            initValue = $"\"{initValue}\"";
                        }
                        else if (param.Type == "float")
                        {
                            initValue = $"{initValue}f";
                        }

                        string paramMinValue = param.MinValue.Replace("ART::kITSourceCurrent", "").Replace("ART::KITSourceCount-1", "");
                        string paramMaxValue = param.MaxValue.Replace("ART::kITSourceCurrent", "").Replace("ART::KITSourceCount-1", "");

                        string paramClampText = "";
                        if (!String.IsNullOrEmpty(paramMinValue) && !String.IsNullOrWhiteSpace(paramMinValue) && !String.IsNullOrEmpty(paramMaxValue) && !String.IsNullOrWhiteSpace(paramMaxValue))
                        {
                            if (param.Type == "float")
                            {
                                paramClampText = $"value = MathHelper.Clamp(value, {paramMinValue}f, {paramMaxValue}f);";
                            }
                            else if (param.Type == "int")
                            {
                                paramClampText = $"value = MathHelper.Clamp(value, {paramMinValue}, {paramMaxValue});"; //substract and add 1 because Clamp minimun and maximun are exclusive
                            }
                            else if (param.Type == "vector3")
                            {
                                paramClampText = $@"value.X = MathHelper.Clamp(value.X, {paramMinValue}f, {paramMaxValue}f);
                value.Y = MathHelper.Clamp(value.Y, {paramMinValue}f, {paramMaxValue}f);
                value.Z = MathHelper.Clamp(value.Z, {paramMinValue}f, {paramMaxValue}f);";
                            }
                        }
                        
                        string defaultValue = "";
                        if(!String.IsNullOrEmpty(initValue) && !String.IsNullOrWhiteSpace(initValue))
                        {
                            defaultValue = $" = {initValue}";
                        }

                        string backendFieldName = param.Name.FirstLetterToLower();
                        string propertyName = param.Name.FirstLetterToUpper();

                        string paramText = $@"        private {param.Type.Replace('v', 'V')} {backendFieldName}{defaultValue};
        /// <summary>
        /// {param.Description}
        /// </summary>
        public {param.Type.Replace('v', 'V')} {propertyName}
        {{
            get {{ return {backendFieldName}; }} 
            set 
            {{  
                {paramClampText}
                SetArgument(""{param.Name}"", value);
                {backendFieldName} = value;
            }} 
        }}
";
                        if (!String.IsNullOrEmpty(initValue) && !String.IsNullOrWhiteSpace(initValue))
                        {
                            resetMethodText += $@"            {param.Name.FirstLetterToLower()} = {initValue};{Environment.NewLine}";
                        }
                        else
                        {
                            resetMethodText += $@"            {param.Name.FirstLetterToLower()} = default({param.Type.Replace('v', 'V')});{Environment.NewLine}";
                        }

                        sb.AppendLine(paramText);
                    }

                    resetMethodText += $@"            base.Reset();
        }}";

            string endText = $@"
        public EuphoriaMessage{behaviour.Name.FirstLetterToUpper()}(bool startNow) : base(""{behaviour.Name}"", startNow)
        {{ }}

        {resetMethodText}
    }}";

                    sb.AppendLine(endText);
                }

                sb.AppendLine("}");
                w.Write(sb.ToString());
            }

            Logger.Info("Finished writing euphoria behaviours");
        }

        public string GetVisibilityText()
        {
            if (UIManager.VisibilityPublic)
                return "public";
            else if (UIManager.VisibilityInternal)
                return "internal";
            else if(UIManager.VisibilityProtected)
                return "protected";
            else if(UIManager.VisibilityPrivate)
                return "private";
            return "internal";
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CurrentThread != null && CurrentThread.IsAlive)
                CurrentThread.Abort();
        }
    }
}
