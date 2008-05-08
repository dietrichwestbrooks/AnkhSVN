// $Id$
using System;
using Ankh.UI;
using EnvDTE;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using Utils.Win32;

using SharpSvn;
using Ankh.ContextServices;
using Ankh.VS;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using Ankh.WorkingCopyExplorer;
using System.Windows.Forms.Design;

namespace Ankh
{
    /// <summary>
    /// Summary description for UIShell.
    /// </summary>
    public class UIShell : AnkhService, IUIShell
    {
        public UIShell(IAnkhServiceProvider context)
            : base(context)
        {

        }        

        /// <summary>
        /// Shows the commit dialog, blocking until the user hits cancel or commit.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        //public DialogResult ShowCommitDialogModal(CommitContext ctx)
        //{
        /*
        //if ( this.commitDialogWindow == null ) 
            this.CreateCommitDialog();
        Debug.Assert( this.commitDialog != null );

        this.commitDialog.UrlPaths = ctx.UrlPaths;
        this.commitDialog.CommitItems = ctx.CommitItems;
        this.commitDialog.LogMessageTemplate = ctx.LogMessageTemplate;
        this.commitDialog.KeepLocks = ctx.KeepLocks;

        this.commitDialog.LogMessage = ctx.LogMessage;

        // we want to preserve the original state.
        bool originalVisibility = this.commitDialogWindow.Visible;

        this.commitDialogWindow.Visible = true;
        this.commitDialogModal = true;

        this.EnsureWindowSize( this.commitDialogWindow );

        // Fired when user clicks Commit or Cancel.
        this.commitDialog.Proceed += new EventHandler( this.ProceedCommit );

        // we need the buttons enabled now, since it's pseudo-modal.
        this.commitDialog.ButtonsEnabled = true;
        this.commitDialog.Initialize();

        // run a message pump while the commit dialog's open
        Utils.Win32.Message msg;
        while ( this.commitDialogModal && this.commitDialogWindow.Visible ) 
        {
            if ( Win32.GetMessage( out msg, IntPtr.Zero, 0, 0 ))
            {
                Win32.TranslateMessage( out msg );
                Win32.DispatchMessage( out msg );                    
            }
        }

        ctx.LogMessage = this.commitDialog.LogMessage;
        ctx.RawLogMessage = this.commitDialog.RawLogMessage;
        ctx.CommitItems = this.commitDialog.CommitItems;
        ctx.KeepLocks = this.commitDialog.KeepLocks;

        ctx.Cancelled = this.commitDialog.CommitDialogResult == CommitDialogResult.Cancel;

        // restore the pre-modal state.
        this.commitDialog.ButtonsEnabled = false;
        this.commitDialogWindow.Visible = originalVisibility;
        this.commitDialog.CommitItems = new object[]{};

        return ctx;
         */
        //    return DialogResult.OK;
        //}

        AnkhMessageBox _box;
        AnkhMessageBox MessageBox
        {
            get { return _box ?? (_box = new AnkhMessageBox(this)); }
        }

        /// <summary>
        /// Display a message box.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="text"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public DialogResult ShowMessageBox(string text, string caption,
            MessageBoxButtons buttons)
        {
            return MessageBox.Show(text, caption, buttons);
        }

        /// <summary>
        /// Display a message box.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="text"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public DialogResult ShowMessageBox(string text, string caption,
            MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(text, caption, buttons, icon);
        }

        /// <summary>
        /// Display a message box.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="text"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public DialogResult ShowMessageBox(string text, string caption,
            MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return MessageBox.Show(text, caption, buttons, icon, defaultButton);
        }

        public void DisplayHtml(string caption, string html, bool reuse)
        {
            IAnkhWebBrowser browser = Context.GetService<IAnkhWebBrowser>();

            string htmlFile = Path.GetTempFileName();
            using (StreamWriter w = new StreamWriter(htmlFile, false, System.Text.Encoding.UTF8))
                w.Write(html);

            // have it show the html
            Uri url = new Uri("file://" + htmlFile);
            BrowserArgs args = new BrowserArgs();
            args.BaseCaption = caption;

            //if(reuse)
            // args.CreateFlags |= __VSCREATEWEBBROWSER.VSCWB_ReuseExisting;

            browser.Navigate(url, args);
        }

        public PathSelectorResult ShowPathSelector(PathSelectorInfo info)
        {
            IUIService uiService = GetService<IUIService>();

            using (PathSelector selector = new PathSelector(info))
            {
                selector.Context = Context;

                bool succeeded = uiService.ShowDialog(selector) == DialogResult.OK;
                PathSelectorResult result = new PathSelectorResult(succeeded, selector.CheckedItems);
                result.Depth = selector.Recursive ? SvnDepth.Infinity : SvnDepth.Empty;
                result.RevisionStart = selector.RevisionStart;
                result.RevisionEnd = selector.RevisionEnd;
                return result;
            }
        }       
    }
}
