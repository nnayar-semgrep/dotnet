﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Razor.Language;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions;

internal sealed class MvcImportProjectFeature : RazorProjectEngineFeatureBase, IImportProjectFeature
{
    internal const string ImportsFileName = "_ViewImports.cshtml";

    public IReadOnlyList<RazorProjectItem> GetImports(RazorProjectItem projectItem)
    {
        ArgHelper.ThrowIfNull(projectItem);

        // Don't add MVC imports for a component
        if (FileKinds.IsComponent(projectItem.FileKind))
        {
            return [];
        }

        var imports = new List<RazorProjectItem>();
        AddDefaultDirectivesImport(imports);

        // We add hierarchical imports second so any default directive imports can be overridden.
        AddHierarchicalImports(projectItem, imports);

        return imports;
    }

    // Internal for testing
    internal static void AddDefaultDirectivesImport(List<RazorProjectItem> imports)
    {
        imports.Add(DefaultDirectivesProjectItem.Instance);
    }

    // Internal for testing
    internal void AddHierarchicalImports(RazorProjectItem projectItem, List<RazorProjectItem> imports)
    {
        // We want items in descending order. FindHierarchicalItems returns items in ascending order.
        var importProjectItems = ProjectEngine.FileSystem.FindHierarchicalItems(projectItem.FilePath, ImportsFileName).Reverse();
        imports.AddRange(importProjectItems);
    }

    private sealed class DefaultDirectivesProjectItem : RazorProjectItem
    {
        public static readonly DefaultDirectivesProjectItem Instance = new();

        private static readonly InMemoryFileContent s_fileContent = new(@"
@using global::System
@using global::System.Collections.Generic
@using global::System.Linq
@using global::System.Threading.Tasks
@using global::Microsoft.AspNetCore.Mvc
@using global::Microsoft.AspNetCore.Mvc.Rendering
@using global::Microsoft.AspNetCore.Mvc.ViewFeatures
@inject global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TModel> Html
@inject global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json
@inject global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component
@inject global::Microsoft.AspNetCore.Mvc.IUrlHelper Url
@inject global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider
@addTagHelper global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper, Microsoft.AspNetCore.Mvc.Razor
@addTagHelper global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper, Microsoft.AspNetCore.Mvc.Razor
@addTagHelper global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper, Microsoft.AspNetCore.Mvc.Razor
");

        private DefaultDirectivesProjectItem()
        {
        }

#nullable disable

        public override string BasePath => null;

        public override string FilePath => null;

        public override string PhysicalPath => null;

#nullable enable

        public override bool Exists => true;

        public override Stream Read() => s_fileContent.CreateStream();
    }
}
