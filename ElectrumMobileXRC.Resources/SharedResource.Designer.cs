﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ElectrumMobileXRC.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class SharedResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SharedResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ElectrumMobileXRC.Resources.SharedResource", typeof(SharedResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Address is not valid..
        /// </summary>
        public static string Error_AddressIsntValid {
            get {
                return ResourceManager.GetString("Error_AddressIsntValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Seed contains unsupported values. Please to generate random seed with button..
        /// </summary>
        public static string Error_FieldContainsUnsupported {
            get {
                return ResourceManager.GetString("Error_FieldContainsUnsupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The field {0} is required..
        /// </summary>
        public static string Error_FieldRequired {
            get {
                return ResourceManager.GetString("Error_FieldRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your login data are wrong. Please to try it again..
        /// </summary>
        public static string Error_WrongLogin {
            get {
                return ResourceManager.GetString("Error_WrongLogin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Syncing....
        /// </summary>
        public static string Main_Syncing {
            get {
                return ResourceManager.GetString("Main_Syncing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Local.
        /// </summary>
        public static string Main_Transaction_State_Local {
            get {
                return ResourceManager.GetString("Main_Transaction_State_Local", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unconfirmed.
        /// </summary>
        public static string Main_Transaction_State_Unconfirmed {
            get {
                return ResourceManager.GetString("Main_Transaction_State_Unconfirmed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Auto Selection.
        /// </summary>
        public static string NetworkSelection_Auto {
            get {
                return ResourceManager.GetString("NetworkSelection_Auto", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use Own Server.
        /// </summary>
        public static string NetworkSelection_Specific {
            get {
                return ResourceManager.GetString("NetworkSelection_Specific", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XRC MainNet.
        /// </summary>
        public static string NetworkType_Main {
            get {
                return ResourceManager.GetString("NetworkType_Main", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XRC TestNet.
        /// </summary>
        public static string NetworkType_Test {
            get {
                return ResourceManager.GetString("NetworkType_Test", resourceCulture);
            }
        }
    }
}
