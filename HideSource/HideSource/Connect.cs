using System;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;

namespace HideSource
{
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		private const string HideSourceCommandName = "HideSource.Connect.HideSource";
		private DTE2 _applicationObject;
		private AddIn _addInInstance;

		//public Connect()
		//{
		//    // Place your initialization code within this method
		//}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param name='application'>Root object of the host application.</param>
		/// <param name='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param name='addInInst'>Object representing this Add-in.</param>
		/// <param name="custom"></param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;
			if(connectMode == ext_ConnectMode.ext_cm_UISetup)
			{
				object []contextGUIDS = new object[] { };
				Commands2 commands = (Commands2)_applicationObject.Commands;
				const string toolsMenuName = "Tools";

				//Place the command on the tools menu.
				//Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
				CommandBar menuBarCommandBar = ((CommandBars)_applicationObject.CommandBars)["MenuBar"];

				//Find the Tools command bar on the MenuBar command bar:
				CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
				CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

				//This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
				//  just make sure you also update the QueryStatus/Exec method to include the new command names.
				try
				{
					//Add a command to the Commands collection:
					Command command = commands.AddNamedCommand2(_addInInstance, "HideSource", "HideSource", "Executes the command for HideSource", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

					//Add a control for the command to the tools menu:
					if((command != null) && (toolsPopup != null))
					{
						command.AddControl(toolsPopup.CommandBar, 1);
					}
				}
				catch(ArgumentException)
				{
					//If we are here, then the exception is probably because a command with that name
					//  already exists. If so there is no need to recreate the command and we can 
					//  safely ignore the exception.
				}
			}
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param name='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param name='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param name='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param name='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param name='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param name='commandName'>The name of the command to determine state for.</param>
		/// <param name='neededText'>Text that is needed for the command.</param>
		/// <param name='status'>The state of the command in the user interface.</param>
		/// <param name='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if (commandName != HideSourceCommandName) return;

			var doc = _applicationObject.ActiveDocument;
			switch (neededText)
			{
				case vsCommandStatusTextWanted.vsCommandStatusTextWantedStatus:
					status = (doc != null && IsSupportedLanguage(doc.Language)) ? vsCommandStatus.vsCommandStatusSupported : vsCommandStatus.vsCommandStatusUnsupported;
					break;

				case vsCommandStatusTextWanted.vsCommandStatusTextWantedNone:
					status = vsCommandStatus.vsCommandStatusSupported;
					if (doc != null && IsSupportedLanguage(doc.Language))
					{
						status |= vsCommandStatus.vsCommandStatusEnabled;
					}

					break;
			}
		}

		private static bool IsSupportedLanguage(string language)
		{
			return ("CSharp".Equals(language));
			//Basic
			//F#
			//XML
			//Plain Text
		}

		private static string GetLineComment(string language)
		{
			if ("CSharp".Equals(language)) return "// ";

			return null;
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param name='commandName'>The name of the command to execute.</param>
		/// <param name='executeOption'>Describes how the command should be run.</param>
		/// <param name='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param name='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param name='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
			if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
				if (commandName == HideSourceCommandName)
				{
					handled = true;
					
					var doc = _applicationObject.ActiveDocument;
					if (doc == null) return;

					string lineComment = GetLineComment(doc.Language);
					if (lineComment == null) return;

					TextDocument textDoc = (TextDocument)doc.Object("TextDocument");
					if (textDoc == null) return;

					var startEditPoint = textDoc.StartPoint.CreateEditPoint();
					string text = startEditPoint.GetText(textDoc.EndPoint);
					
					// TODO: Read key and IV from settings
					byte[] key = new byte[] { };
					byte[] iv = new byte[] { };

					string newText = TextHelper.EncryptOrDecryptText(text, lineComment, key, iv);
					if (!newText.Equals(text))
					{
						textDoc.Selection.Cancel();
						textDoc.ClearBookmarks();
						
						startEditPoint.Delete(textDoc.EndPoint);
						startEditPoint.Insert(newText);
					}
				}
			}
		}
	}
}
