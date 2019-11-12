using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using MqttHome.WebSockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome
{
    public class MqttHomeLogger : ILog
    {
        WebsocketManager wsm;
        ILog logger;

        public MqttHomeLogger(WebsocketManager wsm, ILog logger)
        {
            this.wsm = wsm;
            this.logger = logger;
        }

        public bool IsDebugEnabled => throw new NotImplementedException();

        public bool IsInfoEnabled => throw new NotImplementedException();

        public bool IsWarnEnabled => throw new NotImplementedException();

        public bool IsErrorEnabled => throw new NotImplementedException();

        public bool IsFatalEnabled => throw new NotImplementedException();

        public ILogger Logger => throw new NotImplementedException();

        public void Debug(object message)
        {
            logger.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            logger.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(object message)
        {
            wsm.SendMessageToAllAsync(message.ToString());
            logger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            wsm.SendMessageToAllAsync(message.ToString());
            logger.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Fatal(object message)
        {
            wsm.SendMessageToAllAsync(message.ToString());
            logger.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            wsm.SendMessageToAllAsync(message.ToString());
            logger.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(object message, Exception exception)
        {
            logger.Info(message, exception);
        }

        public void Info(object message)
        {
            logger.Info(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warn(object message)
        {
            logger.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            logger.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
