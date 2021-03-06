﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//-----------------------------------------------------------------------
// </copyright>
// <summary>Deal with converting strings to .net versions</summary>
//-----------------------------------------------------------------------

using System;
using Microsoft.Win32;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace Microsoft.Build.Shared
{
    /// <summary>
    /// Set of methods to deal with versions in the tasks
    /// </summary>
    internal static class VersionUtilities
    {
        /// <summary>
        /// Convert a version number like 0.0.0.0 to a Version instance.
        /// The method will return null if the string is not a valid value
        /// </summary>
        /// <param name="version">Version string to convert to a version object</param>
        internal static Version ConvertToVersion(string version)
        {
            return ConvertToVersion(version, false);
        }

        /// <summary>
        /// Go though an enumeration and create a sorted list of strings which can be parsed as versions. Keep around the original 
        /// string because it may contain a v and this would be required to create the correct path on disk if the string was part of a path.
        /// </summary>
        internal static SortedDictionary<Version, List<string>> GatherVersionStrings(Version targetPlatformVersion, IEnumerable versions)
        {
            SortedDictionary<Version, List<string>> versionValues = new SortedDictionary<Version, List<string>>(ReverseVersionGenericComparer.Comparer);

            // Loop over versions from registry.
            foreach (string version in versions)
            {
                if (version.Length > 0)
                {
                    Version candidateVersion = VersionUtilities.ConvertToVersion(version);

                    if (candidateVersion != null && (targetPlatformVersion == null || (candidateVersion <= targetPlatformVersion)))
                    {
                        if (versionValues.ContainsKey(candidateVersion))
                        {
                            List<string> versionList = versionValues[candidateVersion];
                            if (!versionList.Contains(version))
                            {
                                versionList.Add(version);
                            }
                        }
                        else
                        {
                            versionValues.Add(candidateVersion, new List<string>() { version });
                        }
                    }
                }
            }

            return versionValues;
        }

        /// <summary>
        ///  Convert a version number like 0.0.0.0 to a Version instance.
        /// </summary>
        /// <param name="throwException">Should we use Parse to TryParse (parse means we throw an exception, tryparse means we will not).</param>
        internal static Version ConvertToVersion(string version, bool throwException)
        {
            Version result = null;

            if (version.Length > 0 && (version[0] == 'v' || version[0] == 'V'))
            {
                version = version.Substring(1);
            }

            if (throwException)
            {
                result = Version.Parse(version);
            }
            else
            {
                if (!Version.TryParse(version, out result))
                {
                    return null;
                }
            }

            return result;
        }
    }

    sealed internal class ReverseStringGenericComparer : IComparer<string>
    {
        /// <summary>
        /// Static accessor for a ReverseVersionGenericComparer
        /// </summary>
        internal static readonly ReverseStringGenericComparer Comparer = new ReverseStringGenericComparer();

        /// <summary>
        /// The Compare implements a reverse comparison
        /// </summary>
        int IComparer<string>.Compare(string x, string y)
        {
            // Reverse the sign of the return value.
            return StringComparer.OrdinalIgnoreCase.Compare(y, x);
        }
    }

    sealed internal class ReverseVersionGenericComparer : IComparer<Version>
    {
        /// <summary>
        /// Static accessor for a ReverseVersionGenericComparer
        /// </summary>
        internal static readonly ReverseVersionGenericComparer Comparer = new ReverseVersionGenericComparer();

        /// <summary>
        /// The Compare implements a reverse comparison
        /// </summary>
        int IComparer<Version>.Compare(Version x, Version y)
        {
            // Reverse the sign of the return value.
            return y.CompareTo(x);
        }
    }
}