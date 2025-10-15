# Sitecore CSP (Content Security Policy) Module

A Sitecore 10.1 module that enables Content Security Policy (CSP) header management through the CMS, moving configuration from static web.config to dynamic, content-managed settings.

## Features

- 🎯 **CMS-Managed CSP Headers**: Configure CSP directives directly in Sitecore
- 🔄 **Easy On/Off Toggle**: Enable or disable CMS-based CSP with a single checkbox
- 📋 **Comprehensive Directive Support**: All major CSP directives supported
- ⚡ **Performance Optimized**: Built-in caching to minimize performance impact
- 🔙 **Web.config Fallback**: Automatically falls back to web.config CSP when disabled
- 🛡️ **Security First**: Helps protect against XSS, clickjacking, and other injection attacks

## Supported CSP Directives

| Directive | Description |
|-----------|-------------|
| `default-src` | Fallback for other fetch directives |
| `script-src` | Valid sources for JavaScript |
| `style-src` | Valid sources for stylesheets |
| `img-src` | Valid sources for images |
| `font-src` | Valid sources for fonts |
| `connect-src` | Valid sources for AJAX/WebSocket connections |
| `frame-src` | Valid sources for frames and iframes |
| `frame-ancestors` | Valid parents that may embed this page |
| `object-src` | Valid sources for plugins (object, embed, applet) |
| `media-src` | Valid sources for audio and video |
| `worker-src` | Valid sources for web workers |
| `manifest-src` | Valid sources for app manifests |
| `base-uri` | Restricts URLs for the `<base>` element |
| `form-action` | Restricts URLs for form submissions |
| `child-src` | Valid sources for web workers and nested contexts |
| `upgrade-insecure-requests` | Upgrades HTTP requests to HTTPS |
| `block-all-mixed-content` | Blocks HTTP resources on HTTPS pages |

## Installation

### 1. Deploy Code Files

Copy the following files to your Sitecore instance:

```
src/Foundation/CSP/code/
├── Models/
│   ├── ICspSettingsProvider.cs
│   ├── CspSettingsProvider.cs
│   └── CspSettings.cs
├── Pipelines/
│   └── CspHeaderProcessor.cs
└── App_Config/Include/Foundation/
    └── Foundation.CSP.config
```

### 2. Build and Deploy

- Build your solution
- Deploy assemblies to the `bin` folder
- Deploy the config file to `App_Config/Include/Foundation/`

### 3. Serialize and Deploy Sitecore Items

Use your preferred serialization tool (Unicorn, TDS, Sitecore CLI) to deploy:

```
serialization/Foundation.CSP/
├── Templates/
│   └── CSP Settings.yml
└── Content/
    └── sitecore/system/Settings/CSP.yml
```

**Or manually create the template and item:**
- Template ID: `{a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d}`
- Template Path: `/sitecore/templates/Foundation/RRA/Metadata/CSP/CSP Settings`
- Settings Item Path: `/sitecore/content/RRA/Data/Settings/CSP`

### 4. Publish

Publish all CSP-related items to your web database (if using `web` database in config).

## Configuration

### Basic Setup

1. **Navigate to Settings Item**
   - Path: `/sitecore/system/Settings/CSP`

2. **Enable CMS-Based CSP**
   - Check the "Enabled" checkbox
   - This activates CMS-managed CSP headers

3. **Configure CSP Directives**
   - Fill in the desired values for each directive
   - Use standard CSP syntax (see examples below)

### Configuration File Settings

In `Foundation.CSP.config`, you can customize:

```xml
<!-- Path to the CSP settings item -->
<setting name="CSP.SettingsPath" value="/sitecore/content/RRA/Data/Settings/CSP" />

<!-- Database to use (master for draft, web for published) -->
<setting name="CSP.Database" value="master" />

<!-- Skip CSP injection during content editing (default: true) -->
<setting name="CSP.SkipDuringEditing" value="true" />
```

### Database Selection

- **`master`**: Use draft settings (for testing/development)
- **`web`**: Use published settings (for production)

### Content Editing Protection

By default, CSP headers are **automatically disabled** during content editing to prevent interference with Sitecore's editing interfaces:

**Automatically Skipped Scenarios:**
- Experience Editor mode
- Preview mode  
- Debug mode
- Content Editor backend requests
- Sitecore admin/shell sites
- Static resources (.js, .css, images, fonts)

**Configuration:**
- Set `CSP.SkipDuringEditing` to `false` to disable this protection
- **Recommended:** Keep as `true` (default) for content author experience

## Usage Examples

### Example 1: Basic Sitecore CSP

```
Default Src: 'self'
Script Src: 'self' 'unsafe-inline' 'unsafe-eval'
Style Src: 'self' 'unsafe-inline'
Img Src: 'self' data: https:
Font Src: 'self' data:
Frame Ancestors: 'self'
Object Src: 'none'
```

**Resulting Header:**
```
Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; frame-ancestors 'self'; object-src 'none'
```

### Example 2: With Google Analytics & Tag Manager

```
Default Src: 'self'
Script Src: 'self' 'unsafe-inline' https://www.googletagmanager.com https://www.google-analytics.com
Style Src: 'self' 'unsafe-inline'
Img Src: 'self' data: https:
Connect Src: 'self' https://www.google-analytics.com
```

### Example 3: With External Fonts (Google Fonts)

```
Default Src: 'self'
Script Src: 'self' 'unsafe-inline'
Style Src: 'self' 'unsafe-inline' https://fonts.googleapis.com
Font Src: 'self' https://fonts.gstatic.com
```

### Example 4: Strict Policy (Production)

```
Default Src: 'self'
Script Src: 'self'
Style Src: 'self'
Img Src: 'self' data:
Font Src: 'self'
Frame Ancestors: 'none'
Object Src: 'none'
Upgrade Insecure Requests: ☑ Checked
```

## How It Works

### Request Flow

1. **HTTP Request Received**
   - Pipeline processor runs early in `httpRequestBegin`

2. **Check If Enabled**
   - Reads the "Enabled" checkbox from settings item
   - If disabled → exits (web.config CSP used if present)

3. **Retrieve Settings**
   - Loads CSP settings from cache (if available)
   - Or retrieves from Sitecore database

4. **Build CSP Header**
   - Constructs header from individual directives
   - Sanitizes values and removes empty directives

5. **Inject Header**
   - Removes existing CSP header (from web.config)
   - Adds new CMS-based CSP header

6. **Cache Results**
   - Caches settings for 60 minutes for performance

### Pipeline Architecture

```
httpRequestBegin Pipeline
├── DeviceResolver (Sitecore)
├── CspHeaderProcessor ← Inserted here
├── ...other processors
```

## Fallback Behavior

### When CMS CSP is Disabled

If the "Enabled" checkbox is unchecked, the processor:
1. Skips CSP header injection
2. Allows web.config CSP header to remain active
3. Logs a debug message

### Example web.config Fallback

```xml
<system.webServer>
  <httpProtocol>
    <customHeaders>
      <add name="Content-Security-Policy" 
           value="default-src 'self'; script-src 'self' 'unsafe-inline'" />
    </customHeaders>
  </httpProtocol>
</system.webServer>
```

## Performance Considerations

### Caching Strategy

- **Cache Duration**: 60 minutes (configurable)
- **Cache Size**: 10MB
- **Cache Key**: Per-database basis

### When Cache is Cleared

Cache automatically clears when:
- Sitecore application restarts
- 60 minutes elapse
- Manual cache clearing operations

### Best Practices

1. **Use Published Database in Production**
   - Set `CSP.Database` to `web` for production
   - Ensures stable, published settings

2. **Test in Master Database First**
   - Use `master` database during development
   - Preview changes before publishing

3. **Monitor Logs**
   - Check Sitecore logs for CSP-related messages
   - Look for prefix "CSP:"

## Troubleshooting

### CSP Headers Not Appearing

**Check:**
1. Is the "Enabled" checkbox checked?
2. Is the settings item at the correct path?
3. Is the pipeline processor registered in config?
4. Are there any errors in Sitecore logs?

**Debug:**
```csharp
// Check if processor is running
Log.Info("CSP: Processor executing", this);
```

### CSP Blocking Resources

**Symptoms:**
- Console errors about blocked resources
- Missing images, fonts, or scripts

**Solutions:**
1. Review browser console for specific CSP violations
2. Add blocked domains to appropriate directives
3. Use `'unsafe-inline'` temporarily for testing (not recommended for production)

### Duplicate CSP Headers

**Cause:** Both web.config and CMS CSP are active

**Solution:**
- Disable one source
- Or remove web.config CSP header when enabling CMS version

### Cache Issues

**If settings aren't updating:**
1. Clear Sitecore cache
2. Check cache expiration settings
3. Verify database setting (master vs. web)

## Security Best Practices

### Recommended Baseline

```
Default Src: 'self'
Script Src: 'self'
Style Src: 'self'
Img Src: 'self' data:
Font Src: 'self'
Frame Ancestors: 'none'
Object Src: 'none'
```

### Avoid When Possible

- ❌ `'unsafe-inline'` for scripts (enables XSS attacks)
- ❌ `'unsafe-eval'` (enables code injection)
- ❌ `*` wildcard (defeats the purpose of CSP)

### Use Incrementally

1. Start with report-only mode (requires code modification)
2. Monitor violations in browser console
3. Gradually tighten policy
4. Remove `'unsafe-inline'` and `'unsafe-eval'` last

## Multi-Site Support

### Option 1: Shared CSP (Current Implementation)

All sites use the same CSP settings from `/sitecore/system/Settings/CSP`

### Option 2: Per-Site CSP (Future Enhancement)

Modify `CspSettingsProvider` to:
1. Detect current site context
2. Load site-specific settings item
3. Fall back to global settings

## Extensibility

### Custom Provider

Implement `ICspSettingsProvider` for custom logic:

```csharp
public class CustomCspSettingsProvider : ICspSettingsProvider
{
    public CspSettings GetCspSettings()
    {
        // Your custom logic
    }
}
```

### Custom Directives

Add new directives:

1. Add field to template
2. Add property to `CspSettings` model
3. Add directive to `BuildCspHeader()` method

## Further Reading

- [MDN Web Docs: CSP](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [Content Security Policy Reference](https://content-security-policy.com/)
- [CSP Evaluator Tool](https://csp-evaluator.withgoogle.com/)

## License

This module is provided as-is for Sitecore 10.1 implementations.

## Support

For issues or questions, refer to the `docs/` folder for additional documentation.
