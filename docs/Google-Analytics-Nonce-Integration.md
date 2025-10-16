# Google Analytics Nonce Integration Guide

This guide explains how to use the CSP nonce integration with Google Analytics in your Sitecore 10.1 project.

## Overview

The CSP module now supports nonce-based Content Security Policy headers, which are required for Google Analytics 4 and Google Tag Manager integration. Nonce tokens provide a secure way to allow specific inline scripts while maintaining strict CSP policies.

## Features

- **Request-scoped nonce tokens**: Each HTTP request gets a unique, cryptographically secure nonce token
- **Google Analytics CSP compliance**: Automatic CSP directives for GA4, GTM, and Google Signals
- **Script injection service**: Easy nonce-aware script injection at various DOM locations
- **Rendering model integration**: Base class for automatic nonce access in your components

## Configuration

### 1. Sitecore Settings

Configure the following settings in your `Foundation.CSP.config`:

```xml
<!-- Enable nonce token generation -->
<setting name="CSP.EnableNonce" value="true" />

<!-- Nonce token length (32 bytes recommended) -->
<setting name="CSP.NonceLength" value="32" />

<!-- Enable Google Analytics CSP directives -->
<setting name="CSP.EnableGoogleAnalytics" value="true" />

<!-- Enable Google Signals (requires Google Analytics) -->
<setting name="CSP.EnableGoogleSignals" value="true" />
```

### 2. CSP Settings Item

In Sitecore, configure your CSP settings item (`/sitecore/content/RRA/Data/Settings/CSP`):

- **Enable Nonce**: ✅ Checked
- **Enable Google Analytics**: ✅ Checked  
- **Enable Google Signals**: ✅ Checked (optional)
- **Google Tag Manager ID**: `GTM-XXXXXXX` (your GTM container ID)

## Usage Examples

### 1. Basic Rendering Model

Create a rendering model that inherits from `NonceAwareRenderingModel`:

```csharp
using Foundation.CSP.Models;

public class MyComponentModel : NonceAwareRenderingModel
{
    public string GetCustomScript()
    {
        var script = @"
            console.log('Hello from nonce-aware script!');
            dataLayer.push({'event': 'custom_event'});
        ";
        
        return CreateInlineScript(script);
    }
    
    public string GetExternalScript()
    {
        return CreateExternalScript("https://example.com/script.js", new Dictionary<string, string>
        {
            {"async", "true"},
            {"defer", "true"}
        });
    }
}
```

### 2. Layout Integration

In your main layout file:

```html
@{
    var cspModel = new Foundation.CSP.Models.NonceAwareRenderingModel();
}

<!DOCTYPE html>
<html>
<head>
    <!-- Google Tag Manager (with nonce) -->
    @Html.Raw(cspModel.GetGoogleTagManagerScript())
    
    <!-- Custom head scripts -->
    @Html.Raw(cspModel.InjectScript("head", "console.log('Head script with nonce');"))
</head>
<body>
    <!-- Google Tag Manager (noscript) -->
    @Html.Raw(cspModel.GetGoogleTagManagerNoScript())
    
    <!-- Body top scripts -->
    @Html.Raw(cspModel.InjectScript("body-top", "console.log('Body top script');"))
    
    <!-- Your content here -->
    @RenderBody()
    
    <!-- Body bottom scripts -->
    @Html.Raw(cspModel.InjectScript("body-bottom", "console.log('Body bottom script');"))
</body>
</html>
```

### 3. Component-Level Script Injection

In your component views:

```html
@model MyComponentModel

<div class="my-component">
    <h2>My Component</h2>
    
    <!-- Component-specific script with nonce -->
    @Html.Raw(Model.CreateInlineScript(@"
        (function() {
            console.log('Component initialized with nonce: " + Model.CurrentNonce + @"');
            // Your component JavaScript here
        })();
    "))
</div>
```

### 4. Manual Nonce Access

If you need direct access to the nonce token:

```csharp
public class CustomService
{
    private readonly INonceService _nonceService;
    
    public CustomService(INonceService nonceService)
    {
        _nonceService = nonceService;
    }
    
    public string GenerateCustomScript()
    {
        var nonce = _nonceService.GetCurrentNonce();
        return $"<script nonce=\"{nonce}\">console.log('Custom script');</script>";
    }
}
```

### 5. Script Injection Service

Use the nonce service with your existing script injection service:

```csharp
public class MyController : Controller
{
    private readonly INonceService _nonceService;
    
    public MyController(INonceService nonceService)
    {
        _nonceService = nonceService;
    }
    
    public ActionResult Index()
    {
        // Get the current nonce for use in your scripts
        var nonce = _nonceService.GetCurrentNonce();
        
        // Use with your existing script injection service
        ViewBag.HeadScript = $"<script nonce=\"{nonce}\">console.log('Head');</script>";
        ViewBag.BodyScript = $"<script nonce=\"{nonce}\">console.log('Body');</script>";
        
        return View();
    }
}
```

## Generated CSP Headers

When Google Analytics is enabled, the following CSP directives are automatically added:

```
Content-Security-Policy: 
  default-src 'self'; 
  script-src 'self' 'nonce-abc123...' https://*.googletagmanager.com; 
  img-src 'self' data: https: https://*.google-analytics.com https://*.googletagmanager.com; 
  connect-src 'self' https://*.google-analytics.com https://*.analytics.google.com https://*.googletagmanager.com;
```

With Google Signals enabled, additional domains are included:
- `https://*.g.doubleclick.net`
- `https://*.google.com`
- `https://*.google.`
- `https://pagead2.googlesyndication.com`

## Security Considerations

1. **Nonce Uniqueness**: Each request generates a unique nonce token using cryptographically secure random number generation
2. **Request Scope**: Nonce tokens are stored in `HttpContext.Items` and are unique per request
3. **HTML Encoding**: All nonce tokens and script content are properly HTML-encoded to prevent XSS
4. **Fallback**: If nonce generation fails, the system falls back to GUID-based tokens

## Troubleshooting

### CSP Violations

If you see CSP violations in the browser console:

1. **Check nonce tokens**: Ensure all inline scripts have the correct nonce attribute
2. **Verify CSP settings**: Confirm that nonce is enabled in both config and Sitecore settings
3. **Google Analytics domains**: Make sure Google Analytics is enabled if using GA/GTM

### Missing Nonce Tokens

If scripts don't have nonce attributes:

1. **Inherit from NonceAwareRenderingModel**: Ensure your models inherit from the base class
2. **Use script injection methods**: Use `CreateInlineScript()` instead of manual script tags
3. **Check service registration**: Verify that services are registered in DI container

### Performance Issues

If you experience performance issues:

1. **Cache settings**: CSP settings are cached for 30 minutes by default
2. **Nonce length**: Consider reducing nonce length if memory usage is a concern
3. **Service lifetimes**: Services are registered as Transient for request-scoped behavior

## API Reference

### INonceService

```csharp
public interface INonceService
{
    string GetCurrentNonce();           // Get current request's nonce
    string GenerateNonce();             // Generate new nonce
    bool HasCurrentNonce();             // Check if nonce exists
    void ClearCurrentNonce();           // Clear nonce (testing)
}
```



### NonceAwareRenderingModel

```csharp
public abstract class NonceAwareRenderingModel : RenderingModel
{
    public string CurrentNonce { get; }                                    // Current nonce token
    public string CreateInlineScript(string scriptContent, Dictionary<string, string> additionalAttributes = null);
    public string CreateExternalScript(string scriptSrc, Dictionary<string, string> additionalAttributes = null);
    public string GetGoogleTagManagerScript();                            // GTM script with nonce
    public string GetGoogleTagManagerNoScript();                          // GTM noscript fallback
    public string InjectScript(string location, string scriptContent, bool includeScriptTags = true);
    public bool IsNonceEnabled();                                         // Check if nonce is enabled
    public bool IsGoogleAnalyticsEnabled();                              // Check if GA is enabled
}
```

## Migration Guide

### From Basic CSP to Nonce-Based CSP

1. **Update your rendering models**:
   ```csharp
   // Before
   public class MyModel : RenderingModel
   
   // After  
   public class MyModel : NonceAwareRenderingModel
   ```

2. **Replace inline scripts**:
   ```html
   <!-- Before -->
   <script>console.log('Hello');</script>
   
   <!-- After -->
   @Html.Raw(Model.CreateInlineScript("console.log('Hello');"))
   ```

3. **Update external scripts**:
   ```html
   <!-- Before -->
   <script src="https://example.com/script.js"></script>
   
   <!-- After -->
   @Html.Raw(Model.CreateExternalScript("https://example.com/script.js"))
   ```

4. **Configure Google Analytics**:
   - Enable nonce in CSP settings
   - Add Google Tag Manager ID
   - Enable Google Analytics in CSP settings
   - Replace manual GTM code with `GetGoogleTagManagerScript()`

This integration provides a secure, maintainable way to use Google Analytics with strict Content Security Policy headers in your Sitecore application.
