using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace VasilyPetruhin.TrimSpaces
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidTrimSpacesPkgString)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    public sealed class TrimSpacesPackage : Package
    {
	    private DocumentEvents _documentEvents;
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public TrimSpacesPackage()
        {
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

	        DTE dte = GetService(typeof(SDTE)) as DTE;
	        if (dte != null)
	        {
		        _documentEvents = dte.Events.DocumentEvents;
				_documentEvents.DocumentSaved += OnDocumentSaved;
	        }
        }

	    private static void OnDocumentSaved(Document document)
	    {
			if (IsTextFile(document.Name))
			{
				// Remove trailing whitespace
				vsFindResult result = document.DTE.Find.FindReplace(vsFindAction.vsFindActionReplaceAll, @"[ \t]+\r?$",
				                                                    (int) vsFindOptions.vsFindOptionsRegularExpression,
				                                                    String.Empty,
				                                                    vsFindTarget.vsFindTargetFiles, document.FullName, "",
				                                                    vsFindResultsLocation.vsFindResultsNone);
				if (result == vsFindResult.vsFindResultReplaced)
				{
					// Triggers DocumentEvents_DocumentSaved event again?
					document.Save();
				}
			}
	    }

		private static readonly string[] FileExtensions = new string[]
			{
				".cs", ".vb",
				".cpp", ".c", ".h", ".hpp",
				".sql",
				".xml", ".xsd",  ".xaml", ".resx", ".wsdl",
				".aspx", ".ascx", ".asax", ".ashx", ".asmx", ".css", ".js", ".htm", ".html", ".cshtml",
				".ps1", ".psm1", ".cmd", ".bat",
				".tt",
				".txt", ".nuspec", ".build", ".config", ".manifest"
			};
		private static readonly string[] FileNames = new string[] {};

		private static bool IsTextFile(string fileName)
		{
			fileName = fileName.ToLower();

			if (FileExtensions.Any(fileExt => fileName.EndsWith(fileExt)))
			{
				return true;
			}

			if (FileNames.Any(name => fileName.Equals(name)))
			{
				return true;
			}

			return false;
		}

	    #endregion
    }
}
