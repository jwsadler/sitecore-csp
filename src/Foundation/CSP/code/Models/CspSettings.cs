using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glass.Mapper.Sc.Configuration.Attributes;
using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.Fields;
using Sitecore.Globalization;
using Sitecore.Data;
using Sitecore.Data.Items;
using RRA.Foundation.GlassMapper;
using Foundation.CSP.Services;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
namespace Foundation.CSP.Models
{
    public static class CSPSettingsConstants
    {
        public const string TemplateIdString = "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d";
        public const string TemplatePath = "/sitecore/templates/Foundation/RRA/Metadata/CSP/CSP Settings";
        public const string TemplateName = "CSP Settings";
        public const string BaseUriFieldName = "Base Uri";
        public const string BaseUriFieldIdString = "a2b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d";
        public const string BlockAllMixedContentFieldName = "Block All Mixed Content";
        public const string BlockAllMixedContentFieldIdString = "c4d5e6f7-a8b9-4c5d-8e9f-0a1b2c3d4e5f";
        public const string ChildSrcFieldName = "Child Src";
        public const string ChildSrcFieldIdString = "a4b5c6d7-e8f9-4a5b-8c9d-0e1f2a3b4c5d";
        public const string ConnectSrcFieldName = "Connect Src";
        public const string ConnectSrcFieldIdString = "b3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e";
        public const string DefaultSrcFieldName = "Default Src";
        public const string DefaultSrcFieldIdString = "c4d5e6f7-a8b9-4c0d-1e2f-3a4b5c6d7e8f";
        public const string FontSrcFieldName = "Font Src";
        public const string FontSrcFieldIdString = "d5e6f7a8-b9c0-4d1e-2f3a-4b5c6d7e8f9a";
        public const string FormActionFieldName = "Form Action";
        public const string FormActionFieldIdString = "f3a4b5c6-d7e8-4f5a-8b9c-0d1e2f3a4b5c";
        public const string FrameAncestorsFieldName = "Frame Ancestors";
        public const string FrameAncestorsFieldIdString = "f2a3b4c5-d6e7-4f5a-8b9c-0d1e2f3a4b5c";
        public const string FrameSrcFieldName = "Frame Src";
        public const string FrameSrcFieldIdString = "e6f7a8b9-c0d1-4e2f-3a4b-5c6d7e8f9a0b";
        public const string ImgSrcFieldName = "Img Src";
        public const string ImgSrcFieldIdString = "f7a8b9c0-d1e2-4f3a-4b5c-6d7e8f9a0b1c";
        public const string ManifestSrcFieldName = "Manifest Src";
        public const string ManifestSrcFieldIdString = "a8b9c0d1-e2f3-4a4b-5c6d-7e8f9a0b1c2d";
        public const string MediaSrcFieldName = "Media Src";
        public const string MediaSrcFieldIdString = "b9c0d1e2-f3a4-4b5c-6d7e-8f9a0b1c2d3e";
        public const string ObjectSrcFieldName = "Object Src";
        public const string ObjectSrcFieldIdString = "c0d1e2f3-a4b5-4c6d-7e8f-9a0b1c2d3e4f";
        public const string ScriptSrcFieldName = "Script Src";
        public const string ScriptSrcFieldIdString = "d1e2f3a4-b5c6-4d7e-8f9a-0b1c2d3e4f5a";
        public const string StyleSrcFieldName = "Style Src";
        public const string StyleSrcFieldIdString = "e2f3a4b5-c6d7-4e8f-9a0b-1c2d3e4f5a6b";
        public const string UpgradeInsecureRequestsFieldName = "Upgrade Insecure Requests";
        public const string UpgradeInsecureRequestsFieldIdString = "a4b5c6d7-e8f9-4a0b-1c2d-3e4f5a6b7c8d";
        public const string WorkerSrcFieldName = "Worker Src";
        public const string WorkerSrcFieldIdString = "f3a4b5c6-d7e8-4f9a-0b1c-2d3e4f5a6b7c";
        public const string EnableGoogleAnalyticsFieldName = "Enable Google Analytics";
        public const string EnableGoogleAnalyticsFieldIdString = "8b7a6c5d-4e3f-4210-9865-432109edcbaf";
        public const string EnableGoogleSignalsFieldName = "Enable Google Signals";
        public const string EnableGoogleSignalsFieldIdString = "7c6b5a4d-3e2f-4109-9754-321098dcbafe";
        public const string EnableNonceFieldName = "Enable Nonce";
        public const string EnableNonceFieldIdString = "9a8b7c6d-5e4f-4321-9876-543210fedcba";
        public const string EnabledFieldName = "Enabled";
        public const string EnabledFieldIdString = "f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c";
        public const string GoogleTagManagerIDFieldName = "Google Tag Manager ID";
        public const string GoogleTagManagerIDFieldIdString = "6d5c4b3a-2e1f-4098-9643-210987cbadef";
        public const string ReportUriFieldName = "Report Uri";
        public const string ReportUriFieldIdString = "b5c6d7e8-f9a0-4b1c-2d3e-4f5a6b7c8d9e";
    }

    [SitecoreType(TemplateId = CSPSettingsConstants.TemplateIdString, EnforceTemplate = SitecoreEnforceTemplate.TemplateAndBase, AutoMap = true)]
    public interface ICSPSettings : IGlassBase
    {
        [SitecoreField(FieldId = CSPSettingsConstants.BaseUriFieldIdString)]
        string BaseUri { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.BlockAllMixedContentFieldIdString)]
        bool BlockAllMixedContent { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.ChildSrcFieldIdString)]
        string ChildSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.ConnectSrcFieldIdString)]
        string ConnectSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.DefaultSrcFieldIdString)]
        string DefaultSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.FontSrcFieldIdString)]
        string FontSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.FormActionFieldIdString)]
        string FormAction { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.FrameAncestorsFieldIdString)]
        string FrameAncestors { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.FrameSrcFieldIdString)]
        string FrameSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.ImgSrcFieldIdString)]
        string ImgSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.ManifestSrcFieldIdString)]
        string ManifestSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.MediaSrcFieldIdString)]
        string MediaSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.ObjectSrcFieldIdString)]
        string ObjectSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.ScriptSrcFieldIdString)]
        string ScriptSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.StyleSrcFieldIdString)]
        string StyleSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.UpgradeInsecureRequestsFieldIdString)]
        string UpgradeInsecureRequests { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.WorkerSrcFieldIdString)]
        string WorkerSrc { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.EnableGoogleAnalyticsFieldIdString)]
        bool EnableGoogleAnalytics { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.EnableGoogleSignalsFieldIdString)]
        bool EnableGoogleSignals { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.EnableNonceFieldIdString)]
        bool EnableNonce { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.EnabledFieldIdString)]
        bool Enabled { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.GoogleTagManagerIDFieldIdString)]
        string GoogleTagManagerID { get; set; }
        [SitecoreField(FieldId = CSPSettingsConstants.ReportUriFieldIdString)]
        string ReportUri { get; set; }
    }
    }
}
