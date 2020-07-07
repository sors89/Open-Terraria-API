﻿/*
Copyright (C) 2020 DeathCradle

This file is part of Open Terraria API v3 (OTAPI)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/
using System.IO;
using System.Linq;

namespace OTAPI.Common.Targets
{
    public class TMLPatchTarget : IPatchTarget
    {
        public string GetZipUrl()
        {
            return $"https://github.com/tModLoader/tModLoader/releases/download/v0.11.7.5/tModLoader.Windows.v0.11.7.5.zip";
        }

        public string DetermineInputAssembly(string extractedFolder)
        {
            return Directory.EnumerateFiles(extractedFolder, "tModLoaderServer.exe", SearchOption.TopDirectoryOnly).Single();
        }
    }
}
