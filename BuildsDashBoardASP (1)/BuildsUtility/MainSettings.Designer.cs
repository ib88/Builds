﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BuildsUtility {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class MainSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static MainSettings defaultInstance = ((MainSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new MainSettings())));
        
        public static MainSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("v-komefi@microsoft.com")]
        public string AlertFrom {
            get {
                return ((string)(this["AlertFrom"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("smtphost.redmond.corp.microsoft.com")]
        public string smtpserver {
            get {
                return ((string)(this["smtpserver"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("v-komefi@microsoft.com")]
        public string AlertTo {
            get {
                return ((string)(this["AlertTo"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string AlertMinRange {
            get {
                return ((string)(this["AlertMinRange"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string AlertMaxRange {
            get {
                return ((string)(this["AlertMaxRange"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public string WarningMinRange {
            get {
                return ((string)(this["WarningMinRange"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public string WarningMaxRange {
            get {
                return ((string)(this["WarningMaxRange"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Cc {
            get {
                return ((string)(this["Cc"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("[AdsApps][BVT][MT-Refresh],[AdsApps][BVT][MT-Full],[AdsApps][FFTP][MT-Refresh],[A" +
            "dsApps][FFTP][MT-Full],[AdsApps][BVT-Buddy][MT-RefreshOnly],[LabMan2.0][Buddy][A" +
            "dsApps][RME]-5,[LabMan2.0][BVT][AdsApps][RME]-5,[LabMan2.0][BVT][AdsApps][RME]-1" +
            "0")]
        public string buildDefinitions {
            get {
                return ((string)(this["buildDefinitions"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AppsEnv2K8VMs,AdsAppsBuddyBVT-Pooledm,AdsAppEnv10VMs,AdsAppsSingleBox")]
        public string templates {
            get {
                return ((string)(this["templates"]));
            }
            set {
                this["templates"] = value;
            }
        }
    }
}
