//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PathSystem3D.Navigation.Debug {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal partial class PathFinderDemo {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PathFinderDemo() {
        }

        private System.Windows.Forms.TextBox MinZevel;
        private System.Windows.Forms.ComboBox CollisionPlaneList;
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PathSystem3D.Navigation.Debug.PathFinderDemo", typeof(PathFinderDemo).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static System.Drawing.Bitmap BtnLoad_Image {
            get {
                object obj = ResourceManager.GetObject("BtnLoad.Image", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap BtnNew_Image {
            get {
                object obj = ResourceManager.GetObject("BtnNew.Image", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Point OpenDLG_TrayLocation {
            get {
                object obj = ResourceManager.GetObject("OpenDLG.TrayLocation", resourceCulture);
                return ((System.Drawing.Point)(obj));
            }
        }
        
        internal static System.Drawing.Point SaveDLG_TrayLocation {
            get {
                object obj = ResourceManager.GetObject("SaveDLG.TrayLocation", resourceCulture);
                return ((System.Drawing.Point)(obj));
            }
        }
        
        internal static System.Drawing.Point ToolStrp_TrayLocation {
            get {
                object obj = ResourceManager.GetObject("ToolStrp.TrayLocation", resourceCulture);
                return ((System.Drawing.Point)(obj));
            }
        }
    }
}
