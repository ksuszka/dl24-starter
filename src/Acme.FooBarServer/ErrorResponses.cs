
namespace Acme.FooBarServer
{
    class ErrorResponses
    {
        public const string BadLoginOrPassword = "FAILED 1 bad login or password";
        public const string UnknownCommand = "FAILED 2 unknown command";
        //public const string BadFormat = "FAILED 3 bad format";
        //public const string TooManyArguments = "FAILED 4 too many arguments";
        //public const string InternalError = "FAILED 5 internal error, sorry...";
        public const string CommandLimitReaching = "FAILED 6 commands limit reached, forced waiting activated";
        //public const string BadCave = "FAILED 100 bad cave identifier";
        //public const string NoTunnel = "FAILED 101 there is no tunnel between given caves";
        //public const string NotEnoughEnergy = "FAILED 102 not enough energy";
        //public const string CaveIsAlreadyOccupied = "FAILED 103 cave is already occupied";
        //public const string CageAlreadySet = "FAILED 104 you have already set cage in this cave";
        //public const string BadDirectionValue = "FAILED 105 bad direction value";
        //public const string BadTurnNumber = "FAILED 106 bad turn number";
    }
}
