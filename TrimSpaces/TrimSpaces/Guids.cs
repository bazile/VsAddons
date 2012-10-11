// Guids.cs
// MUST match guids.h
using System;

namespace VasilyPetruhin.TrimSpaces
{
    static class GuidList
    {
        public const string guidTrimSpacesPkgString = "b91960d6-68fd-4fd7-8162-a36b437524d2";
        public const string guidTrimSpacesCmdSetString = "1fd791b5-7085-4a9e-92ac-c05b04f38009";

        public static readonly Guid guidTrimSpacesCmdSet = new Guid(guidTrimSpacesCmdSetString);
    };
}