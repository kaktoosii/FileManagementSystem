using System.Collections.Generic;
using System.Threading.Tasks;
using ViewModels.Settings;

namespace Services.Contracts;
    public interface ISmsSender
    {
        #region BaseClass

           Task SendSmsAsync(string number, string message);
            Task<string> SendWebPushWithFcmToTopicAsync(WebPushConfig webPushConfig, List<string> token, string title, string message, string adminSdkJson, string topicName);
    #endregion

}
