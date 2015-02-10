using System;
using ObjCRuntime;

[assembly: LinkWith ("libgdal.a", LinkTarget.Simulator | LinkTarget.ArmV7 | LinkTarget.ArmV7s | LinkTarget.Arm64, SmartLink = true, ForceLoad = true, LinkerFlags = "-lstdc++")]
