using System;
using System.Text;
using Ankh.UI;

namespace Ankh.WorkingCopyExplorer
{
    internal class FileSystemFileItem : FileSystemItem
    {
        public FileSystemFileItem(IAnkhServiceProvider context, WorkingCopyExplorer explorer, SvnItem item )
            : base(context, null, explorer, item )
        {

        }

        public override bool IsContainer
        {
            get { return false; }
        }

        public override IFileSystemItem[] GetChildren()
        {
            try
            {
                throw new InvalidOperationException( "Cannot get children for leaf item." );
            }
            catch ( Exception ex )
            {
                this.Explorer.Context.ErrorHandler.OnError( ex );
                return new IFileSystemItem[] { };
            }
        }

        public override void Refresh( bool rescan )
        {
            if ( rescan )
            {
                this.SvnItem.Refresh();
            }
            this.CurrentStatus = MergeStatuses( this.CheckChildStatuses(), this.ThisNodeStatus() );
        }
    }
}
