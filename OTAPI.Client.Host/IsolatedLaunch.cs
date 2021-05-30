﻿// Copyright (C) 2020-2021 DeathCradle
//
// This file is part of Open Terraria API v3 (OTAPI)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;

namespace OTAPI.Client.Host
{
    public class IsolatedLaunch
    {
        public static void Launch(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            OTAPI.Hooks.Main.Create = () =>
            {
                return new HostGame();
            };

            Terraria.MacLaunch.Main(args);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.IO.File.AppendAllText("errors.txt", e.ExceptionObject.ToString());
        }
    }
}
