using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace ChristianZangl.SmartFind
{
  [Export(typeof(IVsTextViewCreationListener))]
  [ContentType("text")]
  [TextViewRole(PredefinedTextViewRoles.Interactive)]
  class VsTextViewCreationListener : IVsTextViewCreationListener
  {
    public void VsTextViewCreated(IVsTextView textViewAdapter)
    {
      var filter=new Filter();

      IOleCommandTarget next;
      if (ErrorHandler.Succeeded(textViewAdapter.AddCommandFilter(filter, out next)))
        filter.Next=next;
    }
  }

  class Filter : IOleCommandTarget
  {
    internal IOleCommandTarget Next { get; set; }

		bool block_reentry;

    public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
    {
      //Debug.WriteLine("cmd: "+nCmdID+"-"+((VSConstants.VSStd97CmdID)nCmdID)+" "+(pguidCmdGroup==VSConstants.GUID_VSStandardCommandSet97));

			//Edit.FindNext is operating with the current QuickFind settings, I think, which we wont even really see here.
			//We need to force this to do something different
			if (!block_reentry && pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 &&
				nCmdID == (uint)VSConstants.VSStd97CmdID.FindNext)
			{
				var dte = Package.GetGlobalService(typeof(_DTE)) as _DTE;

				dte.Find.FindReplace(vsFindAction.vsFindActionFind, dte.Find.FindWhat);

				//moreover, in previous VS, pressing Enter would default to running a new search (for ctrl+f), which would leave the focus on the FiF window.
				//Unfortunately, in modern VS, the FiF always defaults to Find All.
				//Even when we click "Find Next", the focus returns to "Find All", a sure sign that someone at MS is out to lunch.
				//I would like to correct this behaviour, but it's just too broken. Use F3 instead and deal with 
				var findWindow = dte.Windows.Item(EnvDTE.Constants.vsWindowKindFindReplace) as EnvDTE.Window;
				//var o = findWindow.Object;
				//if (findWindow.Visible)
				//	findWindow.Activate();

				return 0;

				////run the command and then re-activate the dialog like older VS did
				//block_reentry = true;
				//dte.ExecuteCommand("Edit.FindNext", string.Empty);
				//block_reentry = false;
				////dte.ExecuteCommand("Edit.FindInFiles", string.Empty);
				//var dte2 = dte as EnvDTE80.DTE2;
				//System.Diagnostics.Debugger.Break();
				//var test = (dte2.ToolWindows.GetToolWindow(EnvDTE.Constants.vsWindowKindFindReplace));
				//EnvDTE80.Find2
				//return 0;
			}

      if (!block_reentry && 
				pguidCmdGroup==VSConstants.GUID_VSStandardCommandSet97 &&
        (nCmdID==(uint)VSConstants.VSStd97CmdID.Find ||
        nCmdID==(uint)VSConstants.VSStd97CmdID.FindInFiles ||
        nCmdID==(uint)VSConstants.VSStd97CmdID.ReplaceInFiles ||
        nCmdID==(uint)VSConstants.VSStd97CmdID.Replace))
      {
				int profile = 0;
				if (nCmdID == (uint)VSConstants.VSStd97CmdID.FindInFiles ||
					nCmdID == (uint)VSConstants.VSStd97CmdID.ReplaceInFiles)
					profile = 1;

				//this needs to be configurable: an option can control whether find (ctrl+f) is mapped to the FindInFiles dialog
        if(nCmdID==(uint)VSConstants.VSStd97CmdID.Find)
					nCmdID = (uint)VSConstants.VSStd97CmdID.FindInFiles;
				if (nCmdID == (uint)VSConstants.VSStd97CmdID.Replace)
					nCmdID = (uint)VSConstants.VSStd97CmdID.ReplaceInFiles;

				var dte = Package.GetGlobalService(typeof(_DTE)) as _DTE;

        if (!File.Exists(SmartFindPackage.SettingsFile[profile]))
          SmartFindPackage.Instance.ResetOptions(true,profile);

        dte.ExecuteCommand("Tools.ImportandExportSettings", "/import:\""+SmartFindPackage.SettingsFile[profile]+"\"");

				//more logic related to the option above. i need to discuss this with the original author
				//Next.Exec wasnt working for some reason
				block_reentry = true;
				if (nCmdID == (uint)VSConstants.VSStd97CmdID.FindInFiles)
				{
					dte.ExecuteCommand("Edit.FindInFiles", string.Empty);
				}
				else
					dte.ExecuteCommand("Edit.ReplaceInFiles", string.Empty);
				block_reentry = false;

				return 0;
      }

      int hresult=Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
      return hresult;
    }

    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
    {
      return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
    }
  }

}
