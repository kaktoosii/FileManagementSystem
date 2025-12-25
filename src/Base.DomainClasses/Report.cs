namespace Base.DomainClasses;

public class Report : BaseModel
{
    #region Report
    protected Report() { }
    public Report(string code,
                string title,
                string reportJson,
                int userId) : this()
    {
        UserId = userId;
        Update(code,
               title,
               reportJson);
    }
    public void Update(string code,
                string title,
                string reportJson)
    {

        this.Code = code;
        this.ReportJson = reportJson;
        this.Title = title;
    }
    #endregion

    #region properties
    public string Code { get; private set; }
    public string Title { get; private set; }
    public string ReportJson { get; private set; }
    public virtual User User { get; private set; }
    public int UserId { get; private set; }
    #endregion
}
