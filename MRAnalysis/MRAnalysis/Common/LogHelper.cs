using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using MRAnalysis.Appender;
using MRAnalysis.Model;

namespace MRAnalysis.Common
{
    public class LogHelper
    {
        public static bool IsConfigured { get; }

        public static ILog GetLogger(LogControl logControl)
        {
            var logger = LogManager.GetLogger(logControl.GetType());
            var dataGridAppender = logger.Logger.Repository.GetAppenders().SingleOrDefault(l => l.GetType() == typeof(DataGridAppender));
            var appender = dataGridAppender as DataGridAppender;
            if (appender != null)
                appender.LogControl = logControl;

            return logger;
        }

        public static void Log(LogControl logControl, Log log)
        {
            var logger = GetLogger(logControl);
            switch (log.Level)
            {
                case EnumHelper.State.Debug:
                    logger.Debug(log);
                    break;
                case EnumHelper.State.Warn:
                    logger.Warn(log);
                    break;
                case EnumHelper.State.Error:
                    logger.Error(log);
                    break;
                case EnumHelper.State.Info:
                    logger.Info(log);
                    break;
                default:
                    throw new Exception("Current level: " + log.Level + " can is not handled by Log4NetHelper.");
            }
        }
    }
}
