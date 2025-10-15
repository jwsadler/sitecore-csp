# Web.config CSP Examples

This document shows how to configure CSP in web.config and how to transition to the CMS-based approach.

## Current Web.config CSP Configuration

If you currently have CSP configured in web.config, it might look like this:

### Example 1: Basic CSP in web.config

```xml
<configuration>
  <system.webServer>
    <httpProtocol>
      <customHeaders>
        <add name="Content-Security-Policy" 
             value="default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; frame-ancestors 'self'; object-src 'none'" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

### Example 2: CSP with Report-URI

```xml
<configuration>
  <system.webServer>
    <httpProtocol>
      <customHeaders>
        <add name="Content-Security-Policy" 
             value="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; report-uri https://example.com/csp-report" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

## Transition Strategy

### Option 1: Remove web.config CSP (Recommended)

When you enable CMS-based CSP, the pipeline processor automatically removes the web.config CSP header. You can optionally remove it from web.config:

```xml
<configuration>
  <system.webServer>
    <httpProtocol>
      <customHeaders>
        <!-- CSP header removed - now managed through Sitecore CMS -->
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

### Option 2: Keep web.config CSP as Fallback

Leave your web.config CSP in place. The CMS-based CSP will override it when enabled:

**When CMS CSP is Enabled:**
- Web.config CSP header is removed by the processor
- CMS-based CSP header is injected

**When CMS CSP is Disabled:**
- Web.config CSP header remains active
- Automatic fallback behavior

```xml
<configuration>
  <system.webServer>
    <httpProtocol>
      <customHeaders>
        <!-- This will be used when CMS CSP is disabled -->
        <add name="Content-Security-Policy" 
             value="default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

## Migrating Your Existing CSP

### Step 1: Document Current Policy

Take note of your current CSP header value from web.config.

**Example:**
```
default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; frame-ancestors 'self'; object-src 'none'
```

### Step 2: Parse Into Directives

Break down the policy into individual directives:

| Directive | Value |
|-----------|-------|
| default-src | `'self'` |
| script-src | `'self' 'unsafe-inline' 'unsafe-eval'` |
| style-src | `'self' 'unsafe-inline'` |
| img-src | `'self' data: https:` |
| font-src | `'self' data:` |
| frame-ancestors | `'self'` |
| object-src | `'none'` |

### Step 3: Enter Into Sitecore

Navigate to `/sitecore/system/Settings/CSP` and enter each directive:

1. **Enabled**: ☑ Check this box
2. **Default Src**: `'self'`
3. **Script Src**: `'self' 'unsafe-inline' 'unsafe-eval'`
4. **Style Src**: `'self' 'unsafe-inline'`
5. **Img Src**: `'self' data: https:`
6. **Font Src**: `'self' data:`
7. **Frame Ancestors**: `'self'`
8. **Object Src**: `'none'`

### Step 4: Test

1. Save the Sitecore item
2. Clear Sitecore cache
3. Test your site in a browser
4. Check browser console for CSP violations
5. Verify the CSP header using browser DevTools

### Step 5: Remove or Comment Out web.config CSP

Once confirmed working, you can:

**Option A - Remove completely:**
```xml
<httpProtocol>
  <customHeaders>
    <!-- CSP now managed via Sitecore -->
  </customHeaders>
</httpProtocol>
```

**Option B - Comment out:**
```xml
<httpProtocol>
  <customHeaders>
    <!-- 
    <add name="Content-Security-Policy" 
         value="default-src 'self'; ..." />
    -->
  </customHeaders>
</httpProtocol>
```

**Option C - Keep as documented fallback:**
```xml
<httpProtocol>
  <customHeaders>
    <!-- Fallback CSP when CMS-based CSP is disabled -->
    <add name="Content-Security-Policy" 
         value="default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'" />
  </customHeaders>
</httpProtocol>
```

## Common Pitfalls

### Duplicate Headers

**Problem:** Both web.config and CMS are sending CSP headers

**Solution:** The processor automatically removes web.config CSP when enabled. If you still see duplicates:
1. Verify the processor is registered in `App_Config/Include/Foundation/Foundation.CSP.config`
2. Check Sitecore logs for processor errors
3. Verify the "Enabled" checkbox is checked

### Different Headers on Different Servers

**Problem:** Some servers show web.config CSP, others show CMS CSP

**Solution:**
1. Ensure all servers have the Foundation.CSP.dll deployed
2. Ensure all servers have the config patch deployed
3. Verify Sitecore item exists on all databases
4. Check if using different databases (master vs web)

### CSP Stops Working After Deployment

**Problem:** CSP headers disappear after deployment

**Solution:**
1. Verify Foundation.CSP.dll is deployed
2. Verify Foundation.CSP.config is in App_Config/Include/Foundation/
3. Check if Sitecore item was published to web database
4. Review Sitecore logs for errors

## Testing Both Modes

### Test CMS-Based CSP

1. Navigate to `/sitecore/system/Settings/CSP`
2. Check "Enabled" checkbox
3. Configure directives
4. Save
5. Clear cache
6. Test site and verify headers

### Test Web.config Fallback

1. Navigate to `/sitecore/system/Settings/CSP`
2. Uncheck "Enabled" checkbox
3. Save
4. Clear cache
5. Test site and verify web.config CSP is used

## Environment-Specific Configuration

### Development Environment

**web.config:**
```xml
<!-- Permissive policy for development -->
<add name="Content-Security-Policy" 
     value="default-src 'self' 'unsafe-inline' 'unsafe-eval' https: data: blob:" />
```

**CMS Settings:** Use master database
```xml
<setting name="CSP.Database" value="master" />
```

### Staging/QA Environment

**web.config:**
```xml
<!-- More restrictive than dev, but not production -->
<add name="Content-Security-Policy" 
     value="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:" />
```

**CMS Settings:** Use web database
```xml
<setting name="CSP.Database" value="web" />
```

### Production Environment

**web.config:**
```xml
<!-- Strict fallback policy -->
<add name="Content-Security-Policy" 
     value="default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; object-src 'none'; frame-ancestors 'self'" />
```

**CMS Settings:** Use web database
```xml
<setting name="CSP.Database" value="web" />
```

## Verification Commands

### Check Current CSP Header

**Using curl:**
```bash
curl -I https://your-site.com | grep Content-Security-Policy
```

**Using PowerShell:**
```powershell
(Invoke-WebRequest -Uri "https://your-site.com" -UseBasicParsing).Headers."Content-Security-Policy"
```

### Check Header Source

Add logging to your processor to see which CSP is being used:

```csharp
// In CspHeaderProcessor.cs
if (!_settingsProvider.IsCspEnabled())
{
    Log.Info("CSP: Using web.config CSP header (CMS CSP is disabled)", this);
    return;
}
else
{
    Log.Info("CSP: Using CMS-based CSP header", this);
}
```

## Troubleshooting

### Header Not Showing

1. **Check if processor is registered:**
   - Look in `App_Config/Include/Foundation/Foundation.CSP.config`
   - Verify no config patches are removing it

2. **Check Sitecore logs:**
   - Look for "CSP:" prefixed messages
   - Check for exceptions

3. **Verify item exists:**
   - Navigate to `/sitecore/system/Settings/CSP`
   - Verify it uses the correct template

### Wrong Header Showing

1. **Clear all caches:**
   - Clear Sitecore cache
   - Clear browser cache
   - Restart Sitecore app pool

2. **Check configuration:**
   - Verify `CSP.Database` setting
   - Check if correct database has the item

3. **Review web.config:**
   - Look for multiple CSP header definitions
   - Check for conflicting HTTP modules

## Best Practices

1. **Always Test Changes**
   - Test in non-production first
   - Use browser DevTools to verify
   - Check for console errors

2. **Document Your Policy**
   - Keep a record of web.config CSP
   - Document migration steps
   - Track policy changes over time

3. **Monitor After Migration**
   - Watch Sitecore logs
   - Monitor browser console
   - Check analytics for errors

4. **Have a Rollback Plan**
   - Keep web.config CSP as fallback
   - Document how to disable CMS CSP
   - Test fallback behavior

5. **Use Version Control**
   - Commit web.config changes
   - Track Sitecore item serialization
   - Document in release notes

