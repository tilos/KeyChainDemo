//
// SFHFKeychainUtils.m
//
// Created by Buzz Andersen on 10/20/08.
// Based partly on code by Jonathan Wight, Jon Crosby, and Mike Malone.
// Copyright 2008 Sci-Fi Hi-Fi. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using MonoTouch.Foundation;

namespace MonoTouch.KeyChain
{
    public class KeyChainHelper
    {
        public static string GetPassword(string userName, string serviceName)
        {
            userServiceRequired(userName, serviceName);
            SecurityResult status;
            string result = internalGetPassword(userName, serviceName, out status);
            validateResult(status);
            return result;
        }

        public static void SetPassword(string userName, string password, string serviceName, bool updateExisting)
        {
            userServiceRequired(userName, serviceName);
            validateResult(internalSetPassword(userName, password, serviceName, updateExisting));
        }

        public static void DeletePassword(string userName, string serviceName)
        {
            userServiceRequired(userName, serviceName);
            validateResult(internalDeletePassword(userName, serviceName));
        }

        private static string internalGetPassword(string userName, string serviceName, out SecurityResult status)
        {
	        // Set up a query dictionary with the base query attributes: 
            // item type (generic), username, and service

			NSObject[] keys = new NSObject[] {
				SecurityItem.kSecClass,
				SecurityItem.kSecAttrAccount,
				SecurityItem.kSecAttrService,
				SecurityItem.kSecReturnAttributes,
			};
			NSObject[] objects = new NSObject[] {
				SecurityItem.kSecClassGenericPassword,
				new NSString(userName),
				new NSString(serviceName),
				NSNumber.FromBoolean(true) 
			};
			
	        // First do a query for attributes, in case we already have a Keychain item 
            // with no password data set. One likely way such an incorrect item could have come about 
            // is due to the previous (incorrect) version of this code (which set the password 
            // as a generic attribute instead of password data).
	
			NSDictionary dict = NSDictionary.FromObjectsAndKeys(objects, keys);
			
			keys[3] = SecurityItem.kSecReturnData;
			NSDictionary passQuery = NSDictionary.FromObjectsAndKeys(objects, keys);
			
			NSDictionary attributeResult = null;
			status = SecurityItem.CopyMatching(dict, ref attributeResult);
            dict.Dispose();
	
	        if (status != SecurityResult.Success) 
            {
		        // No existing item found--simply return nil for the password
                if (status == SecurityResult.ItemNotFound)
                    status = SecurityResult.Success;
		        return null;
	        }
	
	        // We have an existing item, now query for the password data associated with it.
			NSData resultData = null;
			status = SecurityItem.CopyMatching(passQuery, ref resultData);
            passQuery.Dispose();

            if (status != SecurityResult.Success) 
            {
		        if (status == SecurityResult.ItemNotFound) 
                {
			        // We found attributes for the item previously, but no password now, so return a special error.
			        // Users of this API will probably want to detect this error and prompt the user to
			        // re-enter their credentials. When you attempt to store the re-entered credentials
			        // using storeUsername:andPassword:forServiceName:updateExisting:error
			        // the old, incorrect entry will be deleted and a new one with a properly encrypted
			        // password will be added.
                    status = SecurityResult.Deprecated;
                }
                else 
                {
			        // Something else went wrong. Simply keep the normal Keychain API error code.
                }
		        return null;
            }
	
        	if (resultData != null) 
            {
                return NSString.FromData(resultData, NSStringEncoding.UTF8);
	        }

            // There is an existing item, but we weren't able to get password data 
            // for it for some reason, Possibly as a result of an item being incorrectly 
            // entered by the previous code. Set the -1999 error so the code above us 
            // can prompt the user again.
            status = SecurityResult.Deprecated;
            return null;
        }

        private static SecurityResult internalSetPassword(string userName, string password, string serviceName, bool updateExisting)
        {
            SecurityResult status;

            // See if we already have a password entered for these credentials.
            string existingPassword = internalGetPassword(userName, serviceName, out status);

            if (status == SecurityResult.Deprecated)
            {
                // There is an existing entry without a password properly stored 
                // (possibly as a result of the previous incorrect version of this code.
                // Delete the existing item before moving on entering a correct one.
                status = internalDeletePassword(userName, serviceName);
                if (status != SecurityResult.Success) return status;
            }
            else if (status != SecurityResult.Success) return status;


            NSObject[] keys;
            NSObject[] objects;
            NSDictionary dict = null;

            try
            {
                if (!String.IsNullOrEmpty(existingPassword))
                {
                    // Only update if we're allowed to update existing. 
                    // If not, simply do nothing.
                    if (existingPassword.Equals(password) || !updateExisting)
                        return SecurityResult.Success;

                    // We have an existing, properly entered item with a password.
                    // Update the existing item.
                    keys = new NSObject[] {
			            SecurityItem.kSecClass,
			            SecurityItem.kSecAttrService,
			            SecurityItem.kSecAttrLabel,
			            SecurityItem.kSecAttrAccount,
		            };
                    objects = new NSObject[] {
			            SecurityItem.kSecClassGenericPassword,
			            new NSString(serviceName),
			            new NSString(serviceName),
			            new NSString(userName),
		            };

                    dict = NSDictionary.FromObjectsAndKeys(objects, keys);
                    NSDictionary attributes = NSDictionary.FromObjectAndKey(
                        new NSString(password).Encode(NSStringEncoding.UTF8),
                        SecurityItem.kSecValueData);
                    return SecurityItem.Update(dict, attributes);
                }
                else
                {
                    // No existing entry (or an existing, improperly entered, and therefore now
                    // deleted, entry). Create a new entry.
                    keys = new NSObject[] {
				        SecurityItem.kSecClass,
				        SecurityItem.kSecAttrService,
				        SecurityItem.kSecAttrLabel,
				        SecurityItem.kSecAttrAccount,
				        SecurityItem.kSecValueData,
			        };
                    objects = new NSObject[] {
				        SecurityItem.kSecClassGenericPassword,
				        new NSString(serviceName),
				        new NSString(serviceName),
				        new NSString(userName),
				        new NSString(password).Encode(NSStringEncoding.UTF8),
			        };
                    dict = NSDictionary.FromObjectsAndKeys(objects, keys);
                    return SecurityItem.Add(dict);
                }
            }
            finally
            {
             if (dict != null) 
                 dict.Dispose();
            }
        }

        private static SecurityResult internalDeletePassword(string userName, string serviceName)
        {
            //delete password
            NSObject[] keys = new NSObject[] {
				SecurityItem.kSecClass,
				SecurityItem.kSecAttrAccount,
				SecurityItem.kSecAttrService,
				SecurityItem.kSecReturnAttributes
			};
            NSObject[] objects = new NSObject[] {
				SecurityItem.kSecClassGenericPassword,
				new NSString(userName),
				new NSString(serviceName),
				NSNumber.FromBoolean(true) 
			};

            using (NSDictionary dict = NSDictionary.FromObjectsAndKeys(objects, keys))
            {
                return SecurityItem.Delete(dict);
            }
        }

        private static void validateResult(SecurityResult status)
        {
            if (status == SecurityResult.Success) return;

            throw new InvalidOperationException(
                String.Format("Security result error ({0}): {1}", 
                status, SecurityItem.SecurityResultToString(status)));
        }

        private static void userServiceRequired(string userName, string serviceName)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Missing argument 'userName'.");
            if (String.IsNullOrEmpty(serviceName))
                throw new ArgumentException("Missing argument 'serviceName'.");
        }

    }
}
