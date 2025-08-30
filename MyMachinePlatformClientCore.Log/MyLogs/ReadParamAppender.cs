namespace MyMachinePlatformClientCore.Log.MyLogs
{
    public class ReadParamAppender : log4net.Appender.AppenderSkeleton
    {
        private string _file;
        public string File
        {
            get { return this._file; }
            set { _file = value; }
        }

        private int _maxSizeRollBackups;
        public int MaxSizeRollBackups
        {
            get { return this._maxSizeRollBackups; }
            set { _maxSizeRollBackups = value; }
        }

        private bool _appendToFile = true;
        public bool AppendToFile
        {
            get { return this._appendToFile; }
            set { _appendToFile = value; }
        }

        private string _maximumFileSize;
        public string MaximumFileSize
        {
            get { return this._maximumFileSize; }
            set { _maximumFileSize = value; }
        }

        private string _layoutPattern;
        public string LayoutPattern
        {
            get { return this._layoutPattern; }
            set { _layoutPattern = value; }
        }

        private string _datePattern;
        public string DatePattern
        {
            get { return this._datePattern; }
            set { _datePattern = value; }
        }

        private string _level;
        public string Level
        {
            get { return this._level; }
            set { _level = value; }
        }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
        }
    }
}