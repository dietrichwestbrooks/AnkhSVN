// $Id$
//
// Copyright 2003-2008 The AnkhSVN Project
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using System.Text.RegularExpressions;
using SharpSvn;
using Ankh.Scc.UI;
using Ankh.Scc;
using System.Collections.Generic;

namespace Ankh.UI.PathSelector
{
    /// <summary>
    /// A control that allows the user to pick a revision.
    /// </summary>
    public partial class VersionSelector : System.Windows.Forms.UserControl
    {
        public event EventHandler Changed;

        public VersionSelector()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            /*            this.revisionTypeBox.Items.AddRange( new object[]{
                                                                             RevisionChoice.Head,
                                                                             RevisionChoice.Committed,
                                                                             RevisionChoice.Base,
                                                                             RevisionChoice.Previous,
                                                                             RevisionChoice.Working } );
                        this.revisionTypeBox.SelectedItem = RevisionChoice.Head;
                        this.dateRevisionChoice = new DateRevisionChoice( this.datePicker );
                        this.revisionTypeBox.Items.Add( this.dateRevisionChoice ); */
        }

        /// <summary>
        /// Whether the control has a valid revision.
        /// </summary>
        public bool Valid
        {
            get
            {
                /*if ( this.revisionTypeBox.SelectedItem == null )
                    return NUMBER.IsMatch( this.revisionTypeBox.Text ); 
                else*/
                return true;
            }
        }

        IAnkhServiceProvider _context;
        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>The context.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IAnkhServiceProvider Context
        {
            get { return _context; }
            set 
            { 
                bool set = (_context == null);
                _context = value;

                if (value != null && set && _newValue != null)
                {
                    EnsureList();
                    Revision = _newValue;                    
                }
            }
        }

        IAnkhRevisionResolver _resolver;
        /// <summary>
        /// Gets the revision resolver.
        /// </summary>
        /// <value>The revision resolver.</value>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IAnkhRevisionResolver RevisionResolver
        {
            get
            {
                if (_resolver == null && Context != null)
                    return _resolver = Context.GetService<IAnkhRevisionResolver>();

                return _resolver;
            }
        }

        AnkhRevisionType _currentRevType;
        List<AnkhRevisionType> _revTypes;
        SvnRevision _newValue;
        /// <summary>
        /// The revision selected by the user.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public SvnRevision Revision
        {
            get
            {
                if (_currentRevType != null)
                    return _currentRevType.CurrentValue;
                else
                    return null;
            }
            set
            {
                _newValue = null;
                if (value == null || RevisionResolver == null)
                {
                    _newValue = value;
                    SetRevision(null);
                }
                else
                    SetRevision(RevisionResolver.Resolve(SvnOrigin, value));
            }
        }

        SvnOrigin _origin;
        public SvnOrigin SvnOrigin
        {
            get { return _origin; }
            set { _origin = value; EnsureList(); }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            EnsureList();
        }

        bool _ensured;
        private void EnsureList()
        {
            if (_ensured || RevisionResolver == null || SvnOrigin == null)
                return;

            if (_revTypes != null)
                foreach (AnkhRevisionType rt in _revTypes)
                {
                    if (rt.IsValidOn(SvnOrigin))
                        typeCombo.Items.Add(rt);
                }
            else
                _revTypes = new List<AnkhRevisionType>();

            foreach (AnkhRevisionType rt in RevisionResolver.GetRevisionTypes(SvnOrigin))
            {
                if (_revTypes.Contains(rt))
                    continue;

                _revTypes.Add(rt);
                typeCombo.Items.Add(rt);
            }

            if(_currentRevType != null)
                typeCombo.SelectedItem = _currentRevType;

            _ensured = true;
        }

        void SetRevision(AnkhRevisionType rev)
        {
            if (rev == _currentRevType)
                return;

            if (_revTypes == null)
                _revTypes = new List<AnkhRevisionType>();

            if (_currentRevType != null && !_revTypes.Contains(_currentRevType))
                _revTypes.Add(_currentRevType);

            if (rev != null && !_revTypes.Contains(rev))
                _revTypes.Add(rev);

            EnsureList();

            foreach (Control c in versionTypePanel.Controls)
            {
                c.Enabled = c.Visible = false;
            }

            _currentRevType = rev;

            if (rev.HasUI)
            {
                if (rev.CurrentControl == null)
                    rev.InstantiateUIIn(versionTypePanel, EventArgs.Empty);

                rev.CurrentControl.Visible = rev.CurrentControl.Enabled = true;                    
            }

            typeCombo.SelectedItem = _currentRevType;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void typeCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            AnkhRevisionType rev = typeCombo.SelectedItem as AnkhRevisionType;

            if (rev != null)
                SetRevision(rev);

            OnChanged(EventArgs.Empty);
        }

        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }
    }
}
