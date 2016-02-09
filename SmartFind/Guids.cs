// Guids.cs
// MUST match guids.h
using System;

namespace ChristianZangl.SmartFind
{
  static class GuidList
  {
    public const string guidSmartFindPkgString = "3b28ec01-ab7f-427a-81d3-afc41415ee5e";
		public const string guidSmartFindCmdSetString = "56eca5cf-209d-45ac-a3d4-09ea3df7a7c4";
		public const string guidSmartFindCmdReset0String = "b75ab5f1-ea6a-49ac-96b6-711346dbb6ea";
		public const string guidSmartFindCmdReset1String = "2e9903df-97bd-4e7c-9796-8ab92597e288";

		public static readonly Guid guidSmartFindCmdSet = new Guid(guidSmartFindCmdSetString);
		public static readonly Guid guidSmartFindCmdReset0 = new Guid(guidSmartFindCmdReset0String);
		public static readonly Guid guidSmartFindCmdReset1 = new Guid(guidSmartFindCmdReset1String);
  };
}