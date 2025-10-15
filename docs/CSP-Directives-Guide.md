# CSP Directives Guide

A comprehensive guide to Content Security Policy (CSP) directives and how to use them in the Sitecore CSP module.

## Table of Contents

- [Understanding CSP Syntax](#understanding-csp-syntax)
- [Core Directives](#core-directives)
- [Navigation Directives](#navigation-directives)
- [Document Directives](#document-directives)
- [Special Directives](#special-directives)
- [Common Values](#common-values)
- [Real-World Examples](#real-world-examples)

## Understanding CSP Syntax

### Basic Format

Each directive follows this pattern:
```
directive-name source1 source2 source3
```

### Multiple Sources

Separate multiple sources with spaces:
```
script-src 'self' https://cdn.example.com https://api.example.com
```

### Keywords (Must Be Quoted)

Special keywords must be wrapped in single quotes:
- `'self'` - Same origin as the document
- `'none'` - Nothing is allowed
- `'unsafe-inline'` - Allows inline scripts/styles
- `'unsafe-eval'` - Allows eval() and similar
- `'strict-dynamic'` - Trusts scripts with nonces/hashes
- `'report-sample'` - Includes code sample in violation reports

### Schemes

You can specify entire schemes:
- `https:` - Allows any HTTPS resource
- `http:` - Allows any HTTP resource
- `data:` - Allows data: URIs
- `blob:` - Allows blob: URIs
- `wss:` - Allows WebSocket connections

## Core Directives

### default-src

**Purpose:** Serves as fallback for other fetch directives

**Example:**
```
'self'
```

**When to use:**
- As a baseline for all resource types
- When most resources come from the same origin
- As a security baseline before specifying exceptions

**Common patterns:**
```
'self'                          # Only same-origin
'self' https:                   # Same-origin + any HTTPS
'none'                          # Block everything (use other directives to allow)
```

---

### script-src

**Purpose:** Controls which scripts can be executed

**Example:**
```
'self' 'unsafe-inline' https://www.googletagmanager.com
```

**When to use:**
- To control JavaScript execution
- To prevent XSS attacks
- To whitelist trusted script sources

**Sitecore considerations:**
```
'self'                          # Minimum for Sitecore
'unsafe-inline'                 # Often needed for Sitecore inline scripts
'unsafe-eval'                   # Sometimes needed for dynamic scripts
```

**Security notes:**
- ⚠️ Avoid `'unsafe-inline'` if possible
- ⚠️ Avoid `'unsafe-eval'` if possible
- ✅ Use nonces or hashes for inline scripts instead

---

### style-src

**Purpose:** Controls which stylesheets can be applied

**Example:**
```
'self' 'unsafe-inline' https://fonts.googleapis.com
```

**When to use:**
- To control CSS sources
- To prevent CSS injection attacks
- To whitelist external stylesheets

**Common patterns:**
```
'self' 'unsafe-inline'                              # Typical Sitecore setup
'self' https://fonts.googleapis.com                 # With Google Fonts
'self' 'unsafe-inline' https://cdn.example.com      # With CDN
```

---

### img-src

**Purpose:** Controls which images can be loaded

**Example:**
```
'self' data: https:
```

**When to use:**
- To control image sources
- To prevent tracking pixels
- To whitelist CDNs

**Common patterns:**
```
'self'                          # Only same-origin images
'self' data:                    # Same-origin + data URIs
'self' data: https:             # Same-origin + data URIs + any HTTPS
* data:                         # Allow all (not recommended)
```

**Sitecore considerations:**
- Include `data:` for inline images
- Include `https:` for flexible media library usage
- Consider CDN domains if using external media

---

### font-src

**Purpose:** Controls which fonts can be loaded

**Example:**
```
'self' data: https://fonts.gstatic.com
```

**When to use:**
- To control font sources
- When using web fonts
- When using icon fonts

**Common patterns:**
```
'self'                                      # Only same-origin fonts
'self' data:                                # Same-origin + data URIs
'self' https://fonts.gstatic.com            # With Google Fonts
'self' data: https://use.typekit.net        # With Adobe Fonts
```

---

### connect-src

**Purpose:** Controls which URLs can be loaded via scripts (AJAX, WebSocket, EventSource)

**Example:**
```
'self' https://www.google-analytics.com https://api.example.com
```

**When to use:**
- To control AJAX endpoints
- To control WebSocket connections
- To control API access

**Common patterns:**
```
'self'                                          # Only same-origin
'self' https://www.google-analytics.com         # With analytics
'self' wss://example.com                        # With WebSockets
'self' https://*.example.com                    # With subdomain API
```

**Sitecore considerations:**
- Include Sitecore services endpoints
- Include external API endpoints
- Include analytics/tracking endpoints

---

### frame-src

**Purpose:** Controls which URLs can be embedded as frames/iframes

**Example:**
```
'self' https://www.youtube.com
```

**When to use:**
- To control embedded content
- When using iframes
- To whitelist video players

**Common patterns:**
```
'self'                                  # Only same-origin frames
'none'                                  # Block all frames
'self' https://www.youtube.com          # Allow YouTube embeds
'self' https://www.youtube.com https://vimeo.com  # Multiple video sources
```

**Sitecore considerations:**
- May need `'self'` for Sitecore Experience Editor
- Consider if using external embeds (maps, videos, etc.)

---

### frame-ancestors

**Purpose:** Controls which parents can embed this page in frames

**Example:**
```
'self'
```

**When to use:**
- To prevent clickjacking attacks
- To control where your site can be framed
- As a replacement for X-Frame-Options header

**Common patterns:**
```
'self'                          # Can only frame itself
'none'                          # Cannot be framed at all
'self' https://trusted.com      # Can be framed by self and trusted.com
```

**Security recommendation:**
- Use `'none'` or `'self'` unless you specifically need framing
- This helps prevent clickjacking attacks

---

### object-src

**Purpose:** Controls `<object>`, `<embed>`, and `<applet>` elements

**Example:**
```
'none'
```

**When to use:**
- Almost always set to `'none'`
- Flash and other plugins are deprecated
- Reduces attack surface

**Recommendation:**
- Always use `'none'` unless you specifically need plugins

---

### media-src

**Purpose:** Controls which sources can be used for `<audio>` and `<video>` elements

**Example:**
```
'self' https://media.example.com
```

**When to use:**
- When using audio/video elements
- To control media sources
- To whitelist media CDNs

**Common patterns:**
```
'self'                              # Only same-origin media
'self' https://media.cdn.com        # With media CDN
'self' blob:                        # For MediaStream APIs
```

## Navigation Directives

### base-uri

**Purpose:** Restricts URLs that can be used in `<base>` element

**Example:**
```
'self'
```

**When to use:**
- To prevent base tag hijacking
- As a security baseline

**Recommendation:**
- Always set to `'self'`

---

### form-action

**Purpose:** Restricts URLs that can be used as form action targets

**Example:**
```
'self'
```

**When to use:**
- To control form submissions
- To prevent form hijacking
- To whitelist payment processors

**Common patterns:**
```
'self'                                  # Only same-origin forms
'self' https://payment.example.com      # Allow external payment processor
```

**Sitecore considerations:**
- Include Sitecore form endpoints
- Include external form processors (if any)

## Document Directives

### child-src

**Purpose:** Defines valid sources for web workers and nested browsing contexts

**Example:**
```
'self'
```

**When to use:**
- When using Web Workers
- As a fallback for frame-src and worker-src

**Note:**
- Deprecated in favor of `frame-src` and `worker-src`
- Still useful for backwards compatibility

---

### worker-src

**Purpose:** Controls valid sources for Worker, SharedWorker, or ServiceWorker

**Example:**
```
'self'
```

**When to use:**
- When using Web Workers
- When using Service Workers (PWAs)

**Common patterns:**
```
'self'                          # Only same-origin workers
'self' blob:                    # Include blob URLs
```

---

### manifest-src

**Purpose:** Controls which manifest can be loaded

**Example:**
```
'self'
```

**When to use:**
- For Progressive Web Apps (PWAs)
- When using web app manifests

**Common patterns:**
```
'self'                          # Typical usage
```

## Special Directives

### upgrade-insecure-requests

**Purpose:** Instructs browsers to upgrade HTTP requests to HTTPS

**Value:** Checkbox (no sources needed)

**When to use:**
- On HTTPS sites
- To enforce HTTPS for all resources
- During HTTP to HTTPS migration

**Example:**
```
Check the checkbox to enable
```

**Browser behavior:**
- Automatically upgrades http:// to https://
- Applies to all resource types

---

### block-all-mixed-content

**Purpose:** Prevents loading any HTTP resources on HTTPS pages

**Value:** Checkbox (no sources needed)

**When to use:**
- On HTTPS sites
- For strict security
- When all resources are available over HTTPS

**Example:**
```
Check the checkbox to enable
```

**Browser behavior:**
- Blocks all HTTP content on HTTPS pages
- Stricter than upgrade-insecure-requests

## Common Values

### Keywords

| Value | Description |
|-------|-------------|
| `'self'` | Same origin (scheme, host, port) |
| `'none'` | Nothing is allowed |
| `'unsafe-inline'` | Allows inline code (scripts/styles) |
| `'unsafe-eval'` | Allows eval() and similar functions |
| `'strict-dynamic'` | Trusts scripts with valid nonces |
| `'unsafe-hashes'` | Allows inline event handlers |

### Schemes

| Value | Description |
|-------|-------------|
| `https:` | Any HTTPS resource |
| `http:` | Any HTTP resource |
| `data:` | Data URIs (base64) |
| `blob:` | Blob URIs |
| `wss:` | Secure WebSocket |
| `ws:` | WebSocket |

### Wildcards

| Value | Description |
|-------|-------------|
| `*` | Any origin (except data:, blob:, filesystem:) |
| `*.example.com` | Any subdomain of example.com |
| `https://*.example.com` | Any HTTPS subdomain |

## Real-World Examples

### Example 1: Basic Sitecore Site

```
Default Src: 'self'
Script Src: 'self' 'unsafe-inline' 'unsafe-eval'
Style Src: 'self' 'unsafe-inline'
Img Src: 'self' data: https:
Font Src: 'self' data:
Connect Src: 'self'
Frame Src: 'self'
Frame Ancestors: 'self'
Object Src: 'none'
```

**Use case:** Standard Sitecore implementation with minimal external dependencies

---

### Example 2: Sitecore with Analytics

```
Default Src: 'self'
Script Src: 'self' 'unsafe-inline' 'unsafe-eval' https://www.googletagmanager.com https://www.google-analytics.com
Style Src: 'self' 'unsafe-inline'
Img Src: 'self' data: https: https://www.google-analytics.com
Font Src: 'self' data:
Connect Src: 'self' https://www.google-analytics.com https://stats.g.doubleclick.net
Frame Src: 'self'
Frame Ancestors: 'self'
Object Src: 'none'
```

**Use case:** Sitecore with Google Analytics and Tag Manager

---

### Example 3: Sitecore with External Resources

```
Default Src: 'self'
Script Src: 'self' 'unsafe-inline' https://cdn.example.com https://ajax.googleapis.com
Style Src: 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.example.com
Img Src: 'self' data: https: https://cdn.example.com
Font Src: 'self' data: https://fonts.gstatic.com
Connect Src: 'self' https://api.example.com
Frame Src: 'self' https://www.youtube.com https://player.vimeo.com
Frame Ancestors: 'self'
Object Src: 'none'
Media Src: 'self' https://cdn.example.com
```

**Use case:** Sitecore with CDN, Google Fonts, and video embeds

---

### Example 4: Strict Production Policy

```
Default Src: 'self'
Script Src: 'self'
Style Src: 'self'
Img Src: 'self' data:
Font Src: 'self'
Connect Src: 'self'
Frame Src: 'none'
Frame Ancestors: 'none'
Object Src: 'none'
Base Uri: 'self'
Form Action: 'self'
Upgrade Insecure Requests: ☑ Checked
```

**Use case:** Maximum security for production site with no external dependencies

---

### Example 5: Progressive Web App (PWA)

```
Default Src: 'self'
Script Src: 'self'
Style Src: 'self'
Img Src: 'self' data: blob:
Font Src: 'self'
Connect Src: 'self' wss://example.com
Worker Src: 'self' blob:
Manifest Src: 'self'
Frame Ancestors: 'none'
Object Src: 'none'
Upgrade Insecure Requests: ☑ Checked
```

**Use case:** Progressive Web App with Service Workers and WebSockets

## Testing Your CSP

### Browser Console

1. Open browser developer tools
2. Check Console tab for CSP violations
3. Violations will show blocked resources

### Example violation:
```
Refused to load the script 'https://example.com/script.js' 
because it violates the following Content Security Policy directive: 
"script-src 'self'".
```

### CSP Testing Tools

- [CSP Evaluator](https://csp-evaluator.withgoogle.com/) - Analyze your policy
- [Report URI](https://report-uri.com/home/generate) - CSP report collection
- Browser DevTools - Real-time violation reports

## Best Practices

1. **Start Permissive, Then Restrict**
   - Begin with relaxed policy
   - Monitor violations
   - Gradually tighten

2. **Test in Non-Production First**
   - Use report-only mode initially
   - Test across all site sections
   - Verify third-party integrations

3. **Document Your Policy**
   - Record why each directive is needed
   - Track external dependencies
   - Document security exceptions

4. **Regular Reviews**
   - Audit policy quarterly
   - Remove unused directives
   - Update for new features

5. **Minimize 'unsafe-inline' and 'unsafe-eval'**
   - Use nonces for inline scripts
   - Refactor eval() usage
   - Move inline scripts to files

