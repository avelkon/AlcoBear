﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AlcoBear.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string FSRAR_ID {
            get {
                return ((string)(this["FSRAR_ID"]));
            }
            set {
                this["FSRAR_ID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("localhost")]
        public string UTM_host {
            get {
                return ((string)(this["UTM_host"]));
            }
            set {
                this["UTM_host"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlcoBear.log")]
        public string logFilePath {
            get {
                return ((string)(this["logFilePath"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8080")]
        public int UTM_port {
            get {
                return ((int)(this["UTM_port"]));
            }
            set {
                this["UTM_port"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastRestQueryID {
            get {
                return ((string)(this["LastRestQueryID"]));
            }
            set {
                this["LastRestQueryID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.DateTime LastRestQueryDateTime {
            get {
                return ((global::System.DateTime)(this["LastRestQueryDateTime"]));
            }
            set {
                this["LastRestQueryDateTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastRestsURL {
            get {
                return ((string)(this["LastRestsURL"]));
            }
            set {
                this["LastRestsURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.DateTime LastRestsDateTime {
            get {
                return ((global::System.DateTime)(this["LastRestsDateTime"]));
            }
            set {
                this["LastRestsDateTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastPartnerQueryID {
            get {
                return ((string)(this["LastPartnerQueryID"]));
            }
            set {
                this["LastPartnerQueryID"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AlcoBear.db")]
        public string dbFilePath {
            get {
                return ((string)(this["dbFilePath"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2000")]
        public int CurrentSettings {
            get {
                return ((int)(this["CurrentSettings"]));
            }
            set {
                this["CurrentSettings"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>Товар ненадлежащего качества</string>
  <string>Недостача</string>
  <string>Розничная реализация продукции, не подлежащая фиксации в ЕГАИС</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection Pattern_ActWriteOffReasons {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["Pattern_ActWriteOffReasons"]));
            }
        }
        
        /// <summary>
        /// Путь к файлу с информацией о последней версии
        /// </summary>
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Configuration.SettingsDescriptionAttribute("Путь к файлу с информацией о последней версии")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ftp://{0}:{1}@ftp.sibatom.com/AlcoBear/AlcoBear.upd")]
        public string UpdateInfoFileUrl {
            get {
                return ((string)(this["UpdateInfoFileUrl"]));
            }
        }
    }
}
