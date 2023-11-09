
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace WMSBrokerProject.Logging
{
	public class LoggingMessageInspector : IClientMessageInspector
    {
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            LogMessage("Request", ref request);
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            LogMessage("Response", ref reply);
        }

        private void LogMessage(string type, ref Message message)
        {
            var buffer = message.CreateBufferedCopy(int.MaxValue);
            var logMessage = buffer.CreateMessage();
            message = buffer.CreateMessage();

            var log = $"{type}: {logMessage}";
            System.IO.File.AppendAllText($"{type}.log", log);
        }
    }

}
