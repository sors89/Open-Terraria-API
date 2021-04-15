﻿///*
//Copyright (C) 2020 DeathCradle

//This file is part of Open Terraria API v3 (OTAPI)

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program. If not, see <http://www.gnu.org/licenses/>.
//*/
//#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
//#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
//using OTAPI;
//using ModFramework;
//using System;

//namespace Terraria.IO
//{
//    public class patch_WorldFile
//    {
//        public static extern void orig_LoadWorld(bool loadFromCloud);
//        public static void LoadWorld(bool loadFromCloud)
//        {
//            if (Hooks.IO.WorldFile.LoadWorld?.Invoke(HookEvent.Before, ref loadFromCloud, orig_LoadWorld) != HookResult.Cancel)
//            {
//                orig_LoadWorld(loadFromCloud);
//                Hooks.IO.WorldFile.LoadWorld?.Invoke(HookEvent.After, ref loadFromCloud, orig_LoadWorld);
//            }
//        }

//        public static extern void orig_SaveWorld(bool useCloudSaving, bool resetTime = false);
//        public static void SaveWorld(bool useCloudSaving, bool resetTime = false)
//        {
//            if (Hooks.IO.WorldFile.SaveWorld?.Invoke(HookEvent.Before, ref useCloudSaving, ref resetTime, orig_SaveWorld) != HookResult.Cancel)
//            {
//                orig_SaveWorld(useCloudSaving, resetTime);
//                Hooks.IO.WorldFile.SaveWorld?.Invoke(HookEvent.After, ref useCloudSaving, ref resetTime, orig_SaveWorld);
//            }
//        }
//    }
//}

//namespace OTAPI
//{
//    public static partial class Hooks
//    {
//        public static partial class IO
//        {
//            public static partial class WorldFile
//            {
//                public delegate HookResult LoadWorldHandler(HookEvent @event, ref bool loadFromCloud, Action<bool> originalMethod);
//                public static LoadWorldHandler LoadWorld;

//                public delegate HookResult SaveWorldHandler(HookEvent @event, ref bool useCloudSaving, ref bool resetTime, Action<bool, bool> originalMethod);
//                public static SaveWorldHandler SaveWorld;
//            }
//        }
//    }
//}

