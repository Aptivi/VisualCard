//
// VisualCard  Copyright (C) 2021-2024  Aptivi
//
// This file is part of VisualCard
//
// VisualCard is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// VisualCard is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace VisualCard.Parsers
{
    internal class VcardParserTools
    {
        internal static string GetTypesString(string[] args, string @default, bool isSpecifierRequired = true)
        {
            // We're given a split of this: "IMPP;TYPE=home:sip:test" delimited by the colon. Split by semicolon to get list of args.
            string[] splitArgs = args[0].Split(VcardConstants._fieldDelimiter);

            // Filter list of arguments with the arguments that start with the type argument specifier, or, if specifier is not required,
            // that doesn't have an equals sign
            var ArgType = splitArgs.Where((arg) => arg.StartsWith(VcardConstants._typeArgumentSpecifier) || !arg.Contains("=")).ToArray();

            // Trying to specify type without TYPE= is illegal according to RFC2426 in vCard 3.0 and 4.0
            if (ArgType.Count() > 0 && !ArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier) && isSpecifierRequired)
                throw new InvalidDataException("Type must be prepended with TYPE=");

            // Get the type from the split argument
            string Type = "";
            if (isSpecifierRequired)
                // Attempt to get the value from the key strictly
                Type =
                    ArgType.Count() > 0 ?
                    string.Join(VcardConstants._valueDelimiter.ToString(), ArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                    @default;
            else
                // Attempt to get the value from the key
                Type =
                    ArgType.Count() > 0 ?
                        ArgType[0].StartsWith(VcardConstants._typeArgumentSpecifier) ?
                        string.Join(VcardConstants._valueDelimiter.ToString(), ArgType.Select((arg) => arg.Substring(VcardConstants._typeArgumentSpecifier.Length))) :
                        ArgType[0]
                    :
                        @default;

            // Return the type
            return Type;
        }

        internal static string[] GetTypes(string[] args, string @default, bool isSpecifierRequired = true) =>
            GetTypesString(args, @default, isSpecifierRequired).Split(VcardConstants._valueDelimiter);

        internal static string GetValuesString(string[] args, string @default, string argSpecifier)
        {
            // We're given a split of this: "IMPP;ARGSPECIFIER=etc;TYPE=home:sip:test" delimited by the colon. Split by semicolon to get list of args.
            string[] splitArgs = args[0].Split(VcardConstants._fieldDelimiter);

            // Filter list of arguments with the arguments that start with the specified specifier (key)
            var argFromSpecifier = splitArgs.Where((arg) => arg.StartsWith(argSpecifier));

            // Attempt to get the value from the key
            string argString =
                    argFromSpecifier.Count() > 0 ?
                    string.Join(VcardConstants._valueDelimiter.ToString(), argFromSpecifier.Select((arg) => arg.Substring(argSpecifier.Length))) :
                    @default;
            return argString;
        }

        internal static string[] GetValues(string[] args, string @default, string argSpecifier) =>
            GetValuesString(args, @default, argSpecifier).Split(VcardConstants._valueDelimiter);

        internal static string[] SplitToKeyAndValueFromString(string line)
        {
            if (line.IndexOf(':') < 0)
                return [line, ""];
            string key = line.Substring(0, line.IndexOf(':'));
            string value = line.Substring(line.IndexOf(':'));
            return [key, value];
        }

        internal static IEnumerable<int> GetDigits(int num)
        {
            int individualFactor = 0;
            int tennerFactor = Convert.ToInt32(Math.Pow(10, num.ToString().Length));
            while (tennerFactor > 1)
            {
                num -= tennerFactor * individualFactor;
                tennerFactor /= 10;
                individualFactor = num / tennerFactor;
                yield return individualFactor;
            }
        }
    }
}
