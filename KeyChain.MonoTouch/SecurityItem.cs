

using System;
using System.Runtime.InteropServices;
using MonoTouch.Foundation;

namespace MonoTouch.KeyChain
{
    public enum SecurityResult
    {
        Success = 0,                    // No error.
        Unimplemented = -4,             // Function or operation not implemented.
        Parameter = -50,                // One or more parameters passed to a function where not valid.
        Allocate = -108,                // Failed to allocate memory.
        Deprecated = -1999,             // Deprecated password encryption error found.
        NotAvailable = -25291,	        // No keychain is available. You may need to restart your computer.
        DuplicateItem = -25299,	        // The specified item already exists in the keychain.
        ItemNotFound = -25300,	        // The specified item could not be found in the keychain.
        InteractionNotAllowed = -25308, // User interaction is not allowed.
        Decode = -26275,                // Unable to decode the provided data.
    } 

    public class SecurityItem
    {
        public static NSString kSecClass = new NSString("class");
        public static NSString kSecAttrAccount = new NSString("acct");
        public static NSString kSecAttrService = new NSString("svce");
        public static NSString kSecAttrLabel = new NSString("labl");
        public static NSString kSecClassGenericPassword = new NSString("genp");
        public static NSString kSecValueData = new NSString("v_Data");
        public static NSString kSecReturnData = new NSString("r_Data");
        public static NSString kSecReturnAttributes = new NSString("r_Attributes");

        [DllImport("/System/Library/Frameworks/Security.framework/Security")]
        private static extern int SecItemAdd(IntPtr attributes, IntPtr result);

        [DllImport("/System/Library/Frameworks/Security.framework/Security")]
        private static extern int SecItemUpdate(IntPtr query, IntPtr attributesToUpdate);

        [DllImport("/System/Library/Frameworks/Security.framework/Security")]
        private static extern int SecItemDelete(IntPtr query);

        [DllImport("/System/Library/Frameworks/Security.framework/Security")]
        private static extern int SecItemCopyMatching(IntPtr query, ref IntPtr result);

        public static SecurityResult Add(NSDictionary query)
        {
            return (SecurityResult)SecItemAdd(query.Handle, IntPtr.Zero);
        }

        public static SecurityResult Update(NSDictionary query, NSDictionary attributesToUpdate)
        {
            return (SecurityResult)SecItemUpdate(query.Handle, attributesToUpdate.Handle);
        }

        public static SecurityResult Delete(NSDictionary query)
        {
            return (SecurityResult)SecItemDelete(query.Handle);
        }

        public static SecurityResult CopyMatching(NSDictionary query, ref NSDictionary result)
        {
            IntPtr handle = IntPtr.Zero;
            SecurityResult osStatus = (SecurityResult)SecItemCopyMatching(query.Handle, ref handle);
            if (osStatus == SecurityResult.Success) 
                result = new NSDictionary(handle);
            return osStatus;
        }

        public static SecurityResult CopyMatching(NSDictionary query, ref NSData result)
        {
            IntPtr handle = IntPtr.Zero;
            SecurityResult osStatus = (SecurityResult)SecItemCopyMatching(query.Handle, ref handle);
            if (osStatus == SecurityResult.Success)
                result = new NSData(handle);
            return osStatus;
        }

        public static string SecurityResultToString(SecurityResult status)
        {
            switch (status)
            {
                case SecurityResult.Success:
				
				
				    //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de-DE");

                    Localization.Culture = new System.Globalization.CultureInfo("de-DE");

                    return Localization.Success;
                    //return "No error.";
                case SecurityResult.Unimplemented:
                    return "Function or operation not implemented.";
                case SecurityResult.Parameter:
                    return "One or more parameters passed to a function where not valid.";
                case SecurityResult.Allocate:
                    return "Failed to allocate memory.";
                case SecurityResult.Deprecated:
                    return "Deprecated password encryption error found.";
                case SecurityResult.NotAvailable:
                    return "No keychain is available. You may need to restart your computer.";
                case SecurityResult.DuplicateItem:
                    return "The specified item already exists in the keychain.";
                case SecurityResult.ItemNotFound:
                    return "The specified item could not be found in the keychain.";
                case SecurityResult.InteractionNotAllowed:
                    return "User interaction is not allowed.";
                case SecurityResult.Decode:
                    return "Unable to decode the provided data.";
                default:
                    return "Unknown security result error.";
            }
        }
    }
}
