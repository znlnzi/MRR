using System;
using log4net.Appender;
using log4net.Core;
using MRAnalysis.Model;

namespace MRAnalysis.Appender
{
    public class DataGridAppender : AppenderSkeleton
    {
        public LogControl LogControl;

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent.MessageObject.GetType().IsAssignableFrom(typeof(Log)))
            {
                var logEntity = (Log)loggingEvent.MessageObject;
                AppendLog(logEntity);
            }
            else
            {
                throw new Exception(string.Format("The logging window received a log message of type '{0}' which was not convertable to an LogEntry", loggingEvent.MessageObject.GetType().Name));
            }
        }

        delegate void AppendLogDelegate(Log logEntity);

        private void AppendLog(Log logEntity)
        {
            if (LogControl.InvokeRequired)
            {
                var appendLogDelegate = new AppendLogDelegate(AppendLog);
                LogControl.Invoke(appendLogDelegate, new object[] { logEntity });
            }
            else
            {
                if (LogControl.LogEntities.Count == 0)
                {
                    LogControl.LogEntities.Add(logEntity);
                }
                else
                {
                    LogControl.LogEntities.Insert(0, logEntity);
                }
            }
        }
    }
}
