//
// uses MonoTouch.Dialog, ExtDialogViewController.cs and Account.cs
//
// Author:
//   Tilo Szepan
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace KeyChain.Demo
{
	public delegate void DelegatedMethod( Account a );
	
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args);
		}
	}
	
	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class AppDelegate : UIApplicationDelegate
	{
		UINavigationController navigation = new UINavigationController();
		List<string[]> titles = new List<string[]>();
		List<Account> accounts = new List<Account>();
		Section mainMenu;
		const string fileName = "../Documents/accdata.txt";

		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			load();
			string[] s; 
			mainMenu = new Section("Blogs", "Click for a new Account");
			for(int i = 0; i < titles.Count; i++){
				s = titles.ElementAt(i);
				var a = new Account( s[0], s[1], s[2] );
				accounts.Add( a );
				a.SetDelegate(this);
				mainMenu.Add( a.GetRoot() );
			}
			var nb = new StringElement("new Account", NewBlog);
			nb.Alignment = UITextAlignment.Center;
			mainMenu.Add( nb );
			
			var dv = new ExtDialogViewController ( new RootElement("Blogs"){ mainMenu } ) {
				Autorotate = true,
				DisableUpsideDown = true
			};

			window.AddSubview (navigation.View);
			navigation.PushViewController (dv, true);				
			window.MakeKeyAndVisible ();
			
			return true;
		}

		public void Changed( Account a )
		{
			string[] s = titles.ElementAt( accounts.IndexOf( a ) );
			if( s[0] != a.Name || s[1] != a.URL || s[2] != a.Login){
				s[0] = a.Name; s[1] = a.URL; s[2] = a.Login;
				save();
			} else Console.WriteLine( "no changes" );
		}

		public void Delete( Account a )
		{
			Console.WriteLine( "Deleting: " + a.Name + a.URL + a.Login );
			titles.RemoveAt( accounts.IndexOf( a ) );
			mainMenu.Remove( accounts.IndexOf( a ) );
			accounts.Remove(a);
			navigation.PopViewControllerAnimated(true);
			save();
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}

		bool load()
		{
			if(! File.Exists(fileName) ) return false;
			try{
				var f =	new StreamReader(fileName);
				while( !f.EndOfStream){
					var s = new string[3];
					for(int j = 0; j < s.Length; j++)
					{
						s[j] = f.ReadLine( );
					}
					titles.Add(s);
				}
				f.Close();
			}catch(Exception e) {
				Console.WriteLine("could not open: " + fileName);
				Console.WriteLine( e.Message );
				return false;
			}
			return true;
		}
		
		bool save()
		{
			try{
				var f =	new StreamWriter( fileName, false);
				for(int i = 0; i < titles.Count; i++)
				{
					var s = titles.ElementAt(i);
					for(int j = 0; j < s.Length; j++)
					{
						f.WriteLine( s[j] );
				}}
				f.Close();
				Console.WriteLine( "saved to file" + Environment.GetFolderPath (Environment.SpecialFolder.Personal) + fileName);
			}catch(Exception e) {
				Console.WriteLine("could not write to: " + fileName);
				Console.WriteLine( e.Message );
				return false;
			}
			return true;
		}

		public void NewBlog()
		{
			var a = new Account();
			accounts.Add( a );
			var s = new string[]{ a.Name, a.URL, a.Login };
			titles.Add(s);
			a.SetDelegate(this);
			mainMenu.Insert( mainMenu.Count-1, a.GetRoot() );
			navigation.PushViewController ( a.getDVC(null), true);
		}
	}
}
