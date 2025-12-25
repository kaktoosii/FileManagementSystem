using System.Net;

namespace Base.DomainClasses;

public class Setting
{
    protected Setting()
    {
        RegisterDate = DateTime.Now;
    }
    public Setting(string title,
                    string meta,
                    string description,
                    string pushToken,
                    string pushApikey,
                    string icon,
                    string fcmServerKey,
                    string fcmSenderId,
                    string footer,
                    string linkedIn,
                    string telegram,
                    string instagram,
                    string copyRight,
                    string phone,
                    string aboutUs,
                    string rules,
                    string questions
        ) : this()
    {
        Update(title,
                meta,
                description,
                pushToken,
                pushApikey,
                icon,
                fcmServerKey,
                fcmSenderId,
                footer,
                linkedIn,
                telegram,
                instagram,
                copyRight,
                phone,
                aboutUs,
                rules,
                questions
                );
    }

    public void Update(string title,
                        string meta,
                        string description,
                        string pushToken,
                        string pushApikey,
                        string icon,
                        string fcmServerKey,
                        string fcmSenderId,
                        string footer,
                        string linkedIn,
                        string telegram,
                        string instagram,
                        string copyRight,
                        string phone,
                        string aboutUs,
                        string rules,
                        string questions
        )
    {
        Title = title;
        Description = description;
        Meta = meta;
        PushToken = pushToken;
        PushApikey = pushApikey;
        Icon = icon;
        FcmServerKey = fcmServerKey;
        FcmSenderId = fcmSenderId;
        Footer = footer;
        Telegram = telegram;
        Instagram = instagram;
        LinkedIn = linkedIn;
        CopyRight = copyRight;
        AboutUs = aboutUs;
        Rules = rules;
        Phone = phone;
        Questions = questions;
    }
    public int Id { get; private set; }
    public string Title { get; private set; }
    public string Meta { get; private set; }
    public string Description { get; private set; }
    public string Footer { get; private set; }
    public string PushToken { get; private set; }
    public string PushApikey { get; private set; }
    public string Icon { get; private set; }
    public string FcmServerKey { get; private set; }
    public string FcmSenderId { get; private set; }
    public string Telegram { get; private set; }
    public string Instagram { get; private set; }
    public string LinkedIn { get; private set; }
    public string CopyRight { get; private set; }
    public string Phone { get; private set; }
    public string AboutUs { get; private set; }
    public string Rules { get; private set; }
    public string Questions { get; private set; }
    public DateTime RegisterDate { get; private set; }
}