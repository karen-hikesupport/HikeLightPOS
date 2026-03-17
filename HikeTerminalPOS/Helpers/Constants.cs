using System;
namespace HikePOS;

public class Messages
{
    public static readonly string IncomingPayloadReceived = "IncomingPayloadReceived";
    public static readonly string RegisteredForRemoteNotifications = "RegisteredForRemoteNotifications";
    public static readonly string IncomingPayloadReceivedInternal = "IncomingPayloadReceived";
    public static readonly string DatabaseUpdated = "ChallengesUpdated";
    public static readonly string AuthenticationComplete = "AuthenticationComplete";
    public static readonly string UserAuthenticated = "UserAuthenticated";
}

public static class PrinterConst
{
    public const string IMinPrinter = "imin";


    public const int QuantityLeght = 7;
    public const int AmounLeght = 13;

    public const int RegisterAmounLeght576 = 16;
    public const int RegisterAmounLeght384 = 14;

    public const int PickAndPackCountLeght = 9;
}