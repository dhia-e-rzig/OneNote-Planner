# OneNote Agent

A .NET MAUI application that provides an AI-powered agent for accessing and organizing your OneNote notebooks using Microsoft Graph API.

## Security Features

This application implements multiple security measures:

| Feature | Implementation |
|---------|----------------|
| ✅ In-process MCP | All tools run within the app process - no IPC, tokens never cross process boundary |
| ✅ No caching | Zero data at rest - nothing to steal if device is compromised |
| ✅ Certificate pinning | MITM protection for Microsoft Graph and Azure AD endpoints |
| ✅ Short-lived tokens | 1-hour maximum lifetime, MSAL handles automatic refresh |
| ✅ Scope minimization | `Notes.Read` by default, `Notes.ReadWrite` only when write operations needed |
| ✅ WAM broker (Windows) | OS-managed authentication - tokens never in app memory during login |
| ✅ Audit logging | All operations logged (operation type, timestamp, IDs - no content) |

## Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or VS Code with .NET MAUI extension
- Azure AD application registration

## Azure AD Setup

1. Go to [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations
2. Click **New registration**
3. Configure:
   - Name: `OneNote Agent`
   - Supported account types: `Accounts in any organizational directory and personal Microsoft accounts`
   - Redirect URI: Select **Public client/native (mobile & desktop)** and add:
     - `msal{YOUR_CLIENT_ID}://auth` (for all platforms)

4. After creation, note your **Application (client) ID**

5. Go to **API permissions** → Add a permission → Microsoft Graph → Delegated permissions:
   - `Notes.Read`
   - `Notes.ReadWrite`
   - `User.Read`
   
6. Click **Grant admin consent** (if you're an admin) or users will consent on first sign-in

## Configuration

Replace `YOUR_CLIENT_ID` with your actual Azure AD client ID in these files:

1. **`src/OneNoteAgent.Maui/Services/AuthService.cs`**:
   ```csharp
   private const string ClientId = "YOUR_CLIENT_ID_HERE";
   ```

2. **`src/OneNoteAgent.Maui/Platforms/Android/AndroidManifest.xml`**:
   ```xml
   <data android:scheme="msalYOUR_CLIENT_ID" android:host="auth" />
   ```

3. **`src/OneNoteAgent.Maui/Platforms/iOS/Info.plist`**:
   ```xml
   <string>msalYOUR_CLIENT_ID</string>
   ```

## Build and Run

### Windows
```bash
cd "src/OneNoteAgent.Maui"
dotnet build -f net9.0-windows10.0.19041.0
dotnet run -f net9.0-windows10.0.19041.0
```

### Android
```bash
dotnet build -f net9.0-android
# Deploy to connected device or emulator
```

### iOS/macOS
```bash
dotnet build -f net9.0-ios
dotnet build -f net9.0-maccatalyst
```

## Project Structure

```
OneNoteAgent/
├── src/
│   ├── OneNoteAgent.Maui/           # .NET MAUI application
│   │   ├── Converters/              # XAML value converters
│   │   ├── Mcp/                     # In-process MCP tools
│   │   ├── Platforms/               # Platform-specific code
│   │   ├── Services/                # Auth, OneNote, Certificate pinning
│   │   ├── ViewModels/              # MVVM ViewModels
│   │   └── Views/                   # XAML pages
│   │
│   └── OneNoteAgent.Core/           # Shared models and interfaces
│       ├── Interfaces/              # Service contracts
│       └── Models/                  # Data models
```

## Usage

1. Launch the app
2. Click **Sign In** to authenticate with your Microsoft account
3. Use natural language to interact with your OneNote:
   - "List my notebooks"
   - "Show sections in [notebook-id]"
   - "Search for meeting notes"
   - "Create a page in [section-id] titled 'My New Note'"
   - "Read page [page-id]"

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     MAUI App Process                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ AuthService  │  │   Chat       │  │  MCP Tools       │  │
│  │ (MSAL + WAM) │◄─│  ViewModel   │──│  (In-Process)    │  │
│  │ 1hr tokens   │  └──────────────┘  └────────┬─────────┘  │
│  └──────┬───────┘                             │            │
│         │ Token (in-memory only)              │            │
│         └─────────────────────────────────────┘            │
│                           │                                 │
│  ┌────────────────────────▼────────────────────────────┐   │
│  │              HttpClient + Certificate Pinning        │   │
│  └──────────────────────────┬───────────────────────────┘  │
└─────────────────────────────┼───────────────────────────────┘
                              │ TLS 1.3
                              ▼
                 ┌─────────────────────────┐
                 │    Microsoft Graph      │
                 └─────────────────────────┘
```

## MCP Tools Available

This project uses the official [Model Context Protocol SDK](https://github.com/modelcontextprotocol/csharp-sdk) for tool definitions.
Tools are decorated with `[McpServerTool]` attributes for automatic schema generation and discovery.

| Tool | Description | ReadOnly |
|------|-------------|----------|
| `list_notebooks` | List all accessible notebooks | ✓ |
| `get_notebook` | Get details of a specific notebook | ✓ |
| `list_sections` | List sections in a notebook | ✓ |
| `list_pages` | List pages in a section | ✓ |
| `get_page_content` | Read the content of a page | ✓ |
| `create_page` | Create a new page | |
| `update_page` | Update an existing page | |
| `search_notes` | Search across all notes | ✓ |
| `organize_notes` | Analyze notebook for organization suggestions | ✓ |

## Certificate Pinning

The app pins certificates for:
- `graph.microsoft.com`
- `login.microsoftonline.com`
- `login.microsoft.com`
- `login.windows.net`

Pinned root CA thumbprints:
- DigiCert Global Root G2
- DigiCert Global Root CA
- Baltimore CyberTrust Root
- Microsoft RSA Root Certificate Authority 2017

## License

MIT License - See LICENSE file for details.
