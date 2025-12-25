using Base.Common.SiteOptions;
using Base.DomainClasses;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Kavenegar;
using Kavenegar.Core.Models.Enums;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Settings;


namespace Services;


public class MessageSender : ISmsSender
{
    private readonly string _smsToken;
    public MessageSender(IOptionsSnapshot<SiteSettingsDto> configuration)
    {
        if (configuration is null || configuration.Value is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        _smsToken = configuration.Value.SmsToken ?? "";
    }

    public async Task SendSmsAsync(string number, string message)
    {
        KavenegarApi kavenegar = new KavenegarApi(_smsToken);
        try
        {
            await kavenegar.VerifyLookup(number, message, "confirm", VerifyLookupType.Sms);
        }
        catch (Kavenegar.Core.Exceptions.ApiException ex)
        {
            // در صورتی که خروجی وب سرویس 200 نباشد این خطارخ می دهد.
            Console.Write("Message : " + ex.Message);
        }
        catch (Kavenegar.Core.Exceptions.HttpException ex)
        {
            // در زمانی که مشکلی در برقرای ارتباط با وب سرویس وجود داشته باشد این خطا رخ می دهد
            Console.Write("Message : " + ex.Message);
        }
    }


    public async Task<string> SendWebPushWithFcmToTopicAsync(WebPushConfig webPushConfig, List<string> token, string title, string message, string adminSdkJson, string topicName)
    {
        try
        {
            //var relativePath = @"\wwwroot\adminsdk.json";
            //var rootPath = this._hostingEnvironment.ContentRootPath;
            //var path = Path.Combine(rootPath + relativePath);
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(adminSdkJson)
                    //Credential = GoogleCredential.FromFile(path),
                });
            }
            // See documentation on defining a message payload.
            var message1 = new FirebaseAdmin.Messaging.Message()
            {
                Data = new Dictionary<string, string>
                    {
                        {"test","1" }
                    },
                Notification = new Notification()
                {
                    Title = title,
                    Body = message,
                }
                ,
                Topic = topicName // "hamdard_app"
            };

            // Send a message to devices subscribed to the combination of topics
            // specified by the provided condition.
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message1);
            // Response is a message ID string.
            Console.WriteLine("Successfully sent message: " + response);
            return response;
        }
        catch (Exception e)
        {
            return e.ToString();
        }

    }

}
