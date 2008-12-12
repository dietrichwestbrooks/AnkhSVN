// $Id$
//
// Copyright 2004-2008 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

// $Id$
using System;
using Ankh.UI;
using System.Windows.Forms;

using SharpSvn;
using Ankh.Ids;
using System.Windows.Forms.Design;
using Ankh.Selection;
using System.Collections.ObjectModel;

namespace Ankh.Commands
{
    /// <summary>
    /// Command to checkout a Subversion repository.
    /// </summary>
    [Command(AnkhCommand.Checkout)]
    class CheckoutCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            Ankh.Scc.ISvnRepositoryItem selected = GetValidSelectedItem(e.Selection);
            e.Enabled = selected != null;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            Ankh.Scc.ISvnRepositoryItem selected = GetValidSelectedItem(e.Selection);
            if (selected == null) { return; }
            Uri uri = selected.Uri;
            SharpSvn.SvnRevision rev = selected.Revision;
            string name = selected.Name;

            Ankh.VS.IAnkhSolutionSettings ss = e.GetService<Ankh.VS.IAnkhSolutionSettings>();

            IUIService ui = e.GetService<IUIService>();

            using (CheckoutDialog dlg = new CheckoutDialog())
            {
                dlg.Context = e.Context;
                dlg.Uri = uri;
                dlg.LocalPath = System.IO.Path.Combine(ss.NewProjectLocation, name);
                

                if (ui.ShowDialog(dlg) != DialogResult.OK)
                    return;

                e.GetService<IProgressRunner>().RunModal("Checking Out", 
                    delegate(object sender, ProgressWorkerArgs a)
                    {
                        SvnCheckOutArgs args = new SvnCheckOutArgs();
                        args.Revision = dlg.Revision;
                        args.Depth = dlg.Recursive ? SvnDepth.Infinity : SvnDepth.Children;

                        a.Client.CheckOut(dlg.Uri, dlg.LocalPath, args);
                    });
            }
        }

        private Ankh.Scc.ISvnRepositoryItem GetValidSelectedItem(ISelectionContext context)
        {
            Ankh.Scc.ISvnRepositoryItem result = null;
            int counter = 0;
            foreach (Ankh.Scc.ISvnRepositoryItem i in context.GetSelection<Ankh.Scc.ISvnRepositoryItem>())
            {
                counter++;
                if (counter > 1) 
                {
                    return null; 
                } // multiple selection

                if (i.Origin != null)
                {
                    result = i;
                }
            }
            return result;
        }
    }
}
