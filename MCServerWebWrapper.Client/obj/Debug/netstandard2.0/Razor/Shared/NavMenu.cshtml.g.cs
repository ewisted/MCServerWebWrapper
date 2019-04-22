#pragma checksum "C:\Users\RGBeast\source\repos\MCServerWebWrapper\MCServerWebWrapper.Client\Shared\NavMenu.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "ee129dff21f0b4af8995c432dcb7f355bc0e86ac"
// <auto-generated/>
#pragma warning disable 1591
namespace MCServerWebWrapper.Client.Shared
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using System.Net.Http;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.AspNetCore.Components.Layouts;
    using Microsoft.AspNetCore.Components.Routing;
    using Microsoft.JSInterop;
    using MCServerWebWrapper.Client;
    using MCServerWebWrapper.Client.Shared;
    public class NavMenu : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.RenderTree.RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "top-row pl-4 navbar navbar-dark");
            builder.AddContent(2, "\r\n    ");
            builder.AddMarkupContent(3, "<a class=\"navbar-brand\" href>MCServerWebWrapper</a>\r\n    ");
            builder.OpenElement(4, "button");
            builder.AddAttribute(5, "class", "navbar-toggler");
            builder.AddAttribute(6, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.UIMouseEventArgs>(this, ToggleNavMenu));
            builder.AddMarkupContent(7, "\r\n        <span class=\"navbar-toggler-icon\"></span>\r\n    ");
            builder.CloseElement();
            builder.AddContent(8, "\r\n");
            builder.CloseElement();
            builder.AddContent(9, "\r\n\r\n");
            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "class", NavMenuCssClass);
            builder.AddAttribute(12, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.UIMouseEventArgs>(this, ToggleNavMenu));
            builder.AddContent(13, "\r\n    ");
            builder.OpenElement(14, "ul");
            builder.AddAttribute(15, "class", "nav flex-column");
            builder.AddContent(16, "\r\n        ");
            builder.OpenElement(17, "li");
            builder.AddAttribute(18, "class", "nav-item px-3");
            builder.AddContent(19, "\r\n            ");
            builder.OpenComponent<Microsoft.AspNetCore.Components.Routing.NavLink>(20);
            builder.AddAttribute(21, "class", "nav-link");
            builder.AddAttribute(22, "href", "");
            builder.AddAttribute(23, "Match", Microsoft.AspNetCore.Components.RuntimeHelpers.TypeCheck<Microsoft.AspNetCore.Components.Routing.NavLinkMatch>(NavLinkMatch.All));
            builder.AddAttribute(24, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)((builder2) => {
                builder2.AddMarkupContent(25, "\r\n                <span class=\"oi oi-home\" aria-hidden=\"true\"></span> Home\r\n            ");
            }
            ));
            builder.CloseComponent();
            builder.AddContent(26, "\r\n        ");
            builder.CloseElement();
            builder.AddContent(27, "\r\n        ");
            builder.OpenElement(28, "li");
            builder.AddAttribute(29, "class", "nav-item px-3");
            builder.AddContent(30, "\r\n            ");
            builder.OpenComponent<Microsoft.AspNetCore.Components.Routing.NavLink>(31);
            builder.AddAttribute(32, "class", "nav-link");
            builder.AddAttribute(33, "href", "counter");
            builder.AddAttribute(34, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)((builder2) => {
                builder2.AddMarkupContent(35, "\r\n                <span class=\"oi oi-plus\" aria-hidden=\"true\"></span> Counter\r\n            ");
            }
            ));
            builder.CloseComponent();
            builder.AddContent(36, "\r\n        ");
            builder.CloseElement();
            builder.AddContent(37, "\r\n        ");
            builder.OpenElement(38, "li");
            builder.AddAttribute(39, "class", "nav-item px-3");
            builder.AddContent(40, "\r\n            ");
            builder.OpenComponent<Microsoft.AspNetCore.Components.Routing.NavLink>(41);
            builder.AddAttribute(42, "class", "nav-link");
            builder.AddAttribute(43, "href", "fetchdata");
            builder.AddAttribute(44, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)((builder2) => {
                builder2.AddMarkupContent(45, "\r\n                <span class=\"oi oi-list-rich\" aria-hidden=\"true\"></span> Fetch data\r\n            ");
            }
            ));
            builder.CloseComponent();
            builder.AddContent(46, "\r\n        ");
            builder.CloseElement();
            builder.AddContent(47, "\r\n\t\t");
            builder.OpenElement(48, "li");
            builder.AddAttribute(49, "class", "nav-item px-3");
            builder.AddContent(50, "\r\n\t\t\t");
            builder.OpenComponent<Microsoft.AspNetCore.Components.Routing.NavLink>(51);
            builder.AddAttribute(52, "class", "nav-link");
            builder.AddAttribute(53, "href", "servers");
            builder.AddAttribute(54, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)((builder2) => {
                builder2.AddMarkupContent(55, "\r\n\t\t\t\t<span class=\"oi oi-cloud\" aria-hidden=\"true\"></span> Servers\r\n\t\t\t");
            }
            ));
            builder.CloseComponent();
            builder.AddContent(56, "\r\n\t\t");
            builder.CloseElement();
            builder.AddContent(57, "\r\n    ");
            builder.CloseElement();
            builder.AddContent(58, "\r\n");
            builder.CloseElement();
        }
        #pragma warning restore 1998
#line 33 "C:\Users\RGBeast\source\repos\MCServerWebWrapper\MCServerWebWrapper.Client\Shared\NavMenu.cshtml"
            
    bool collapseNavMenu = true;

    string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

#line default
#line hidden
    }
}
#pragma warning restore 1591
