using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Xml.Linq;
using Utilities;
using Task = System.Threading.Tasks.Task;

namespace SortResourceVS2022
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SortRSCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("d6856ab7-6c3a-4a4d-8b45-d362635a2a24");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SortRSCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in SortResourceCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(dte);

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as IMenuCommandService;
            Assumes.Present(commandService);

            // Visual studio status bar.
            var statusbar = await package.GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;

            var cmdId = new CommandID(PackageGuids.guidSortResourceVS2022PackageCmdSet, PackageIds.SortResourceCommandId);
            var cmd = new OleMenuCommand((s, e) => { OnExecute(dte, statusbar); }, cmdId)
            {
                Supported = false
            };

            commandService.AddCommand(cmd);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        private static void OnExecute(DTE2 dte, IVsStatusbar statusbar)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                ProjectItem item = dte.SelectedItems.Item(1).ProjectItem;
                var path = item?.FileNames[1] ?? "";
                var xml = XElement.Load(path);
                XLMHelper.SortXML(xml);

                // Overwritten file.
                xml.Save(path);

                if (statusbar != null)
                {
                    statusbar.SetText("Sort resources successfully.");
                }
            }
            catch
            {
                if (statusbar != null)
                {
                    statusbar.SetText("Cannot sort resources!");
                }
            }
        }
    }
}
