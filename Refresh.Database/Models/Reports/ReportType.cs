namespace Refresh.Database.Models.Reports; 

public enum ReportType 
{
    Unknown = 0,
    Obscene = 1,
    Mature = 2,
    Offensive = 3,
    Violence = 4,
    IllegalAct = 5,
    // 6 is unknown
    TermsOfService = 7,
    Other = 8,
}