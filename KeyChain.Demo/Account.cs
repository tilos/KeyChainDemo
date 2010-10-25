//
// Account.cs uses MonoTouch.Dialog, ExtDialogViewController.cs and MonoTouch.KeyChain.KeyChainHelper
//
// Author:
//   Tilo Szepan
//
using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.Dialog;

namespace KeyChain.Demo
{
	
	public class Account
	{
		const string StartName = "Account";
		public string Name;
		public string URL = "http://www.google.de";
		public string Login;
		private string Passwd;
		DelegatedMethod SaveAs = null;
		DelegatedMethod Delete = null;
		ExtDialogViewController dvc;
		
		public Account()
		{
			Name = StartName;
		}
		public Account(string NewName)
		{
			Name = NewName;
		}
		public Account(string NewName, string NewURL, string NewLogin)
		{
			Name = NewName; URL = NewURL; Login = NewLogin;
			LoadPW();
		}
		public void SetDelegate(AppDelegate parent)
		{
			SaveAs = new DelegatedMethod(parent.Changed);
			Delete = new DelegatedMethod(parent.Delete);
		}
		
		public RootElement GetRoot()
		{
			RootElement root = CreateRoot ();
			
			dvc = new ExtDialogViewController (root, true){
				Autorotate = true,
				DisableUpsideDown = true
			};
			
			return root;
		}
			
		RootElement CreateRoot ()
		{
			var e_html = new HtmlElement ("Login", URL);
			
			var e_name = new EntryElement("Blogname: ", "Enter the name of the blog", Name);
			
			var e_url = new EntryElement("URL:", "Enter the URL", URL);
			
			var e_login = new EntryElement("Login:", "Enter your login-name", Login);
			e_login.Changed += delegate {
				Login = e_login.Value;
			};
			
			var e_passwd = new EntryElement("Password:", "Enter your password", Passwd, true);
			e_passwd.Changed += delegate {
				Passwd = e_passwd.Value;
			};
			
			var e_save = new StringElement( "Save", SavePW);
			e_save.Alignment = UITextAlignment.Center;
			
			var e_clear = new StringElement( "Delete", Clear);
			e_clear.Alignment = UITextAlignment.Center;
						
			var root = new RootElement (Name, getDVC) {
				new Section (){
					e_name,
					e_url,
					e_login,
					e_passwd
				},
				new Section(){
					e_save,
					e_html
				},
				new Section () { e_clear }
			};
			
			e_name.Changed += delegate {
				Name = e_name.Value;
				root.Caption = Name;
			};

			e_url.Changed += delegate {
				URL = e_url.Value.ToLower();
				if(! URL.StartsWith("http://") ) URL = URL.Insert(0, "http://");
				e_html.Url = URL;
				if( String.IsNullOrEmpty( Name ) || Name == StartName){
					Name = URL.Remove(0, 7);
					root.Caption = Name;
					e_name.Value = Name;
				}
			};
			return root;
		}
		
		public DialogViewController getDVC(RootElement r)
		{
			return dvc;
		}
				
		void SavePW()
		{
			if( String.IsNullOrEmpty( Login )){
				ShowMsg("Login");
				return; }
			if( String.IsNullOrEmpty( URL )){
				ShowMsg("URL");
				return; }
			try{
				if( !String.IsNullOrEmpty( Passwd )){
					MonoTouch.KeyChain.KeyChainHelper.SetPassword( Login, Passwd, URL, true);
					Console.WriteLine("password: " + Passwd + " is saved");
				}
				if(SaveAs !=null) 
					SaveAs( this );
			}catch(Exception e) {
				Console.WriteLine("not saved: " + Name + URL + Login + Passwd);
				Console.WriteLine( e.Message );
			}
		}
		
		void Clear()
		{
//			using(var alert = new UIAlertView( "Deleting Data", "You want to delete this account " + Name + "?", null, "No", "Yes")) {
//			alert.Show();	}
			try{
				if( !String.IsNullOrEmpty( Login ) && !String.IsNullOrEmpty( URL ) && !String.IsNullOrEmpty( Passwd ) ){
					MonoTouch.KeyChain.KeyChainHelper.DeletePassword( Login, URL );
					Console.WriteLine("pw deleted");
				}
			}catch(Exception e) {
				Console.WriteLine("not deleted: " + Name + URL + Login + Passwd);
				Console.WriteLine( e.Message );
			}
			if(Delete != null) Delete(this);
		}
		
		void LoadPW()
		{
			try{
				if( !String.IsNullOrEmpty( Login ) && !String.IsNullOrEmpty( URL ) )
					Passwd = MonoTouch.KeyChain.KeyChainHelper.GetPassword( Login, URL );
			}catch(Exception e) {
				Console.WriteLine("Load: " + Name + URL + Login + Passwd + " an Error occured.");
				Console.WriteLine( e.Message );
			}
		}
		
		void ShowMsg(string msg)
		{
			using(var alert = new UIAlertView( "Missing Data", msg + " is not set yet", null, "OK", null)) {
			alert.Show();
			}
		}
	}
}
