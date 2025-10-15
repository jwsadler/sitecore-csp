#!/bin/bash

# Array of directive fields with their details
declare -A directives=(
    ["Script Src"]="f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c|200|Specifies valid sources for JavaScript. Example: 'self' 'unsafe-inline' https://cdn.example.com"
    ["Style Src"]="a2b3c4d5-e6f7-4a5b-8c9d-0e1f2a3b4c5d|300|Specifies valid sources for stylesheets. Example: 'self' 'unsafe-inline'"
    ["Img Src"]="b2c3d4e5-f6a7-4b5c-8d9e-0f1a2b3c4d5e|400|Specifies valid sources for images. Example: 'self' data: https:"
    ["Font Src"]="c2d3e4f5-a6b7-4c5d-8e9f-0a1b2c3d4e5f|500|Specifies valid sources for fonts. Example: 'self' https://fonts.googleapis.com"
    ["Connect Src"]="d2e3f4a5-b6c7-4d5e-8f9a-0b1c2d3e4f5a|600|Specifies valid sources for XMLHttpRequest, WebSocket, and EventSource connections. Example: 'self' https://api.example.com"
    ["Frame Src"]="e2f3a4b5-c6d7-4e5f-8a9b-0c1d2e3f4a5b|700|Specifies valid sources for nested browsing contexts (frames, iframes). Example: 'self' https://youtube.com"
    ["Frame Ancestors"]="f2a3b4c5-d6e7-4f5a-8b9c-0d1e2f3a4b5c|800|Specifies valid parents that may embed this page in frames. Example: 'self' or 'none' to prevent clickjacking"
    ["Object Src"]="a3b4c5d6-e7f8-4a5b-8c9d-0e1f2a3b4c5d|900|Specifies valid sources for <object>, <embed>, and <applet> elements. Example: 'none'"
    ["Media Src"]="b3c4d5e6-f7a8-4b5c-8d9e-0f1a2b3c4d5e|1000|Specifies valid sources for <audio> and <video> elements. Example: 'self' https://media.example.com"
    ["Worker Src"]="c3d4e5f6-a7b8-4c5d-8e9f-0a1b2c3d4e5f|1100|Specifies valid sources for Worker, SharedWorker, or ServiceWorker scripts. Example: 'self'"
    ["Manifest Src"]="d3e4f5a6-b7c8-4d5e-8f9a-0b1c2d3e4f5a|1200|Specifies valid sources for application manifest files. Example: 'self'"
    ["Base Uri"]="e3f4a5b6-c7d8-4e5f-8a9b-0c1d2e3f4a5b|1300|Restricts the URLs which can be used in a document's <base> element. Example: 'self'"
    ["Form Action"]="f3a4b5c6-d7e8-4f5a-8b9c-0d1e2f3a4b5c|1400|Restricts the URLs which can be used as targets for form submissions. Example: 'self'"
    ["Child Src"]="a4b5c6d7-e8f9-4a5b-8c9d-0e1f2a3b4c5d|1500|Defines valid sources for web workers and nested browsing contexts. Example: 'self'"
)

# Array of checkbox directives
declare -A checkbox_directives=(
    ["Upgrade Insecure Requests"]="b4c5d6e7-f8a9-4b5c-8d9e-0f1a2b3c4d5e|1600|Instructs browsers to upgrade HTTP requests to HTTPS. Check to enable."
    ["Block All Mixed Content"]="c4d5e6f7-a8b9-4c5d-8e9f-0a1b2c3d4e5f|1700|Prevents loading any resources over HTTP when the page is loaded over HTTPS. Check to enable."
)

# Create Multi-Line Text fields
for directive in "${!directives[@]}"; do
    IFS='|' read -r id sortorder description <<< "${directives[$directive]}"
    
    cat > "serialization/Foundation.CSP/Templates/Foundation/RRA/Metadata/CSP/CSP Settings/CSP Directives/${directive}.yml" << FIELD_EOF
---
ID: "${id}"
Parent: "d1e2f3a4-b5c6-4d7e-8f9a-0b1c2d3e4f5a"
Template: "455a3e98-a627-4b40-8035-e683a0331ac7"
Path: /sitecore/templates/Foundation/RRA/Metadata/CSP/CSP Settings/CSP Directives/${directive}
DB: master
SharedFields:
- ID: "ab162cc0-dc80-4abf-8871-998ee5d7ba32"
  Hint: Type
  Value: "Multi-Line Text"
- ID: "ba3f86a2-4a1c-4d78-b63d-91c2779c1b5e"
  Hint: __Sortorder
  Value: ${sortorder}
Languages:
- Language: en
  Versions:
  - Version: 1
    Fields:
    - ID: "19a69332-a23e-4e70-8d16-b44b38c4d16b"
      Hint: Title
      Value: ${directive}
    - ID: "577f1689-7de4-4ad2-a15f-7fdc1759285f"
      Hint: __Long description
      Value: "${description}"
FIELD_EOF
done

# Create Checkbox fields
for directive in "${!checkbox_directives[@]}"; do
    IFS='|' read -r id sortorder description <<< "${checkbox_directives[$directive]}"
    
    cat > "serialization/Foundation.CSP/Templates/Foundation/RRA/Metadata/CSP/CSP Settings/CSP Directives/${directive}.yml" << FIELD_EOF
---
ID: "${id}"
Parent: "d1e2f3a4-b5c6-4d7e-8f9a-0b1c2d3e4f5a"
Template: "455a3e98-a627-4b40-8035-e683a0331ac7"
Path: /sitecore/templates/Foundation/RRA/Metadata/CSP/CSP Settings/CSP Directives/${directive}
DB: master
SharedFields:
- ID: "ab162cc0-dc80-4abf-8871-998ee5d7ba32"
  Hint: Type
  Value: Checkbox
- ID: "ba3f86a2-4a1c-4d78-b63d-91c2779c1b5e"
  Hint: __Sortorder
  Value: ${sortorder}
Languages:
- Language: en
  Versions:
  - Version: 1
    Fields:
    - ID: "19a69332-a23e-4e70-8d16-b44b38c4d16b"
      Hint: Title
      Value: ${directive}
    - ID: "577f1689-7de4-4ad2-a15f-7fdc1759285f"
      Hint: __Long description
      Value: "${description}"
FIELD_EOF
done

echo "Created all directive field YML files"
