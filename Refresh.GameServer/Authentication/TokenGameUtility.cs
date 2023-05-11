namespace Refresh.GameServer.Authentication;

public static class TokenGameUtility
{
    // All values taken from SerialStation.
    // https://serialstation.com
    
    private static readonly string[] LittleBigPlanet1Titles =
    {
        "BCES00141",
        "NPEW00047",
        "UCAS40262",
        "NPEA00147",
        "NPHG00035",
        "BCET70011",
        "BCUS98199",
        "BCAS20091",
        "NPEA00241",
        "NPUG70064",
        "BCET70002",
        "BCJS70009",
        "NPHA80092",
        "UCUS98744",
        "BCJB95003",
        "BCUS70030",
        "NPEG90019",
        "BCUS98148",
        "NPUA70045",
        "BCAS20058",
        "BCJS30018",
        "NPHG00033",
        "UCES01264",
        "BCKS10059",
        "BCAS20078",
        "BCUS98208",
        "BCES00611",
        "NPJG00073",
        "UCJS10107",
        "NPUA80472",
        "NPJA00052",
    };

    private static readonly string[] LittleBigPlanet2Titles =
    {
        "BCAS20201",
        "BCUS98245",
        "BCES01345",
        "BCJS30058",
        "BCUS98249",
        "BCES00850",
        "BCES01346",
        "BCUS90260",
        "BCUS98372",
        "NPUA80662",
    };

    private static readonly string[] LittleBigPlanet3Titles =
    {
        "CUSA00738",
        "CUSA00810",
        "CUSA00473",
        "CUSA01072",
        "CUSA00063",
        "PCJS50003",
        "BCES02068",
        "BCAS20322",
        "BCJS30095",
        "BCES01663",
        "BCUS98362",
        "PCKS90007",
        "PCAS00012",
        "CUSA00601",
        "NPJA00123",
        "BCUS81138",
        "CUSA00762",
        "PCAS20007",
        "CUSA01077",
        "CUSA01304",
        "CUSA00693",
    };

    private static readonly string[] LittleBigPlanetVitaTitles =
    {
        "PCSC00013",
        "PCSF00021",
        "PCSA22106",
        "PCSF00152",
        "PCSF00211",
        "VCAS32010",
        "PCSA22018",
        "VCJS10006",
        "PCSD00006",
        "PCSA00078",
        "PCSA00061",
        "PCSA00017",
        "PCSA00081", 
    };
    
    public static TokenGame? FromTitleId(string titleId)
    {
        if (LittleBigPlanet1Titles.Contains(titleId)) return TokenGame.LittleBigPlanet1;
        if (LittleBigPlanet2Titles.Contains(titleId)) return TokenGame.LittleBigPlanet2;
        if (LittleBigPlanet3Titles.Contains(titleId)) return TokenGame.LittleBigPlanet3;
        if (LittleBigPlanetVitaTitles.Contains(titleId)) return TokenGame.LittleBigPlanetVita;
        
        return null;
    }
}