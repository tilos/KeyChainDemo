//
// ExtDialogViewController.cs: extends:
// DialogViewController.cs: drives MonoTouch.Dialog
//
// Author:
//   Tilo Szepan
//
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace KeyChain.Demo
{
	public class ExtDialogViewController : DialogViewController
	{
		public ExtDialogViewController (RootElement root) : base (root)
		{
		}
		
		public ExtDialogViewController (UITableViewStyle style, RootElement root) : base (style, root)
		{
		}
		public ExtDialogViewController (RootElement root, bool pushing) : base (root, pushing)
		{
		}

		public ExtDialogViewController (UITableViewStyle style, RootElement root, bool pushing) : base (style, root, pushing)
		{
		}
		
		public bool DisableUpsideDown;
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return DisableUpsideDown ? Autorotate && toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown :
				Autorotate;// || toInterfaceOrientation == UIInterfaceOrientation.Portrait;
		}
	}
}

