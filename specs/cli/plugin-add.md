# kiota plugin add

## Description 

`kiota plugin add` allows a developer to add a new plugin to the `workspace.json` file. If no `workspace.json` file is found, a new `workspace.json` file would be created in the `.kiota` directory under the current working directory. The command will add a new entry to the `plugins` section of the `workspace.json` file. Once this is done, a local copy of the OpenAPI document is generated and kept in the `.kiota/documents/{plugin-name}` folder. If a plugin or client with the same name already exists, the command will fail and display an actionable error message.

When executing, a new plugin entry will be added and will use the `--plugin-name` parameter as the key for the map. When loading the OpenAPI document, it will store the location of the description in the `descriptionLocation` property. If `--include-path` or `--exclude-path` are provided, they will be stored in the `includePatterns` and `excludePatterns` properties respectively.

Every time a plugin is added, a copy of the OpenAPI document file will be stored in the `./.kiota/documents/{plugin-name}` folder. The OpenAPI will be named using the plugin name `{plugin-name}.json|yaml`. This will allow the CLI to detect changes in the description and avoid downloading the description again if it hasn't changed.

An [API Manifest][def] file named `apimanifest.json` will be generated (if non existing) or updated (if already existing) in the root folder `./kiota` next to `workspace.json`.  API Manifest represents a snapshot of API dependencies and permissions required to access those APIs. This file will represent a concatenated surface of all APIs used across plugins and clients. Both files, `apimanifest.json` and `workspace.json` will be used to generate the code files. A new hash composed of the Kiota version, the OpenAPI document location and the properties of the manifest will be generated and would trigger an update to the [API Manifest][def].

Developers can generate `name_to_be_defined`, `openai` and `apimanifest` type of plugins. By generating plugins, three outputs will be generated: 1\) a sliced OpenAPI document named `{plugin-name}-openapi.json|yaml`, 2\) the plugin type you have chosen and  3\) an [app manifest](https://learn.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema) file named `manifest.json` which conforms with the schema https://learn.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema.
> [!NOTE] 
> In one's solution, there might be two different [API Manifests][def]. The `apimanifest.json` in the `./kiota` folder represents a single artifact surface of all APIs and it will always be generated. The second one, specific to each plugin when providing `--type apimanifest`, will be named `{plugin-name}-apimanifest.json` and saved in the chosen output directory.

Once the `workspace.json` file is generated and the OpenAPI document file is saved locally, the generation will be executed and the plugin, the sliced OpenAPI document and the `manifest.json` will become available.

### Sliced OpenAPI document
The generated sliced OpenAPI document 
will include only the endpoints that matches `--include-path` and `--exclude-path`, if provided, along with their referenced components. All unused components will be removed as well as all OpenAPI extensions (starts with x-) that don't match with our internal documentation of supported extensions in Kiota.

### Plugins
For `name_to_be_defined` plugins, the generated plugin should follow the internal document dedicate to it that explains naming, schema and required fields.

For `openai` plugins, the generated plugin will be named `openai-plugins.json` and the mapping should follow [Hidi logic to generate OpenAI Plugin](https://github.com/microsoft/OpenAPI.NET/blob/vnext/src/Microsoft.OpenApi.Hidi/OpenApiService.cs#L748). Requiring fields default as the following:

| OpenAI field | Default value |
| -- | -- |
| name_for_human | Defaults to the OpenAPI document title. |
| name_for_model | Defaults to the OpenAPI document title. |
| description_for_human | Defaults to the description from the OpenAPI document.  If the description is not available, it defaults to `Description for {name_for_human}`. |
| description_for_model | Defaults to x-ai-description extension from the OpenAPI document.  If the x-ai-description is not available, it defaults to `description_for_human` or `Description for {name_for_human}`. |
| contact_email | Defaults to the contact email from the OpenAPI document. If the contact email is not available, it defaults to 'publisher-email@example.com'. |
| logo_url | Defaults to x-logo extension from the OpenAPI document. If the x-logo is not available, the logo_url will not be added in the plugin. |
| legal_info_url | Defaults to x-legal-info-url extension from the OpenAPI document. If the x-legal-info-url is not availabe, the legal_info_url will not be added in the plugin. |
|  |  |

For `apimanifest`, the generated file will be named `{plugin-name}-apimanifest.json` and the mapping should follow the [OpenApi.ApiManifest lib map](https://github.com/microsoft/OpenApi.ApiManifest/blob/main/docs/OpenApiToApiManifestMapping.md). Requiring fields are as the following:

| API Manifest field | Default value |
| -- | -- |
| apiDependencies.Key | Defaults to `{plugin-name}`. |
| publisherName | Defaults to the contact name from the OpenAPI document. If the contact name is not available, it defaults to 'publisher-name'. |
| publisherEmail | Defaults to the contact email from the OpenAPI document. If the contact email is not available, it defaults to 'publisher-email@example.com'. |
|  |  |

### App manifest
The app manifest file describes how one's plugin integrates into Microsoft 365 and it's not related to plugin types. App manifests are required for [publishing apps to Microsoft 365 app stores](https://learn.microsoft.com/en-us/partner-center/marketplace/checklist#step-3-check-that-your-manifest-is-compliant).
For `manifest.json` file, we will:
1. Add a plugin node to the `manifest.json` file if a `manifest.json` file already exists in the output directory.

```json
"copilotExtensions": {
  "plugins": [
    {
      "id": "{plugin-name}",
      "file": "<generated_plugin_file>.json"
    }
  ]
},
```
2. If a `plugins` node already exists and there is no plugin with the same `id`, add the new plugin information. If the `id` already exists, replace the current content.
3. If there is no `manifest.json` file, we should create a basic manifest with only required information as the following example:

```jsonc
{
  "$schema": "https://developer.microsoft.com/json-schemas/teams/vDevPreview/MicrosoftTeams.schema.json",
  "manifestVersion": "devPreview",
  "version": "1.0.0",
  "id": "<generated_GUID>",
  "developer": {
    "name": "<Defaults to `contact.name` from the OpenAPI document. If the `contact.name` is not available, it defaults to `Kiota Generator, Inc.`>",
    "websiteUrl": "<Defaults to `contact.url` from the OpenAPI document. If the `contact.url` is not available, it defaults to `https://www.example.com/contact/`>",
    "privacyUrl": "<Defaults to `x-privacy-policy-url` extension from the OpenAPI document. If the `x-privacy-policy-url` is not available, it defaults to `https://www.example.com/privacy/`>",
    "termsOfUseUrl": "<Defaults to `termsOfService` from the OpenAPI document. If the `termsOfService` is not available, it defaults to `https://www.example.com/terms/`>"
  },
  "packageName": "com.microsoft.kiota.plugin.<pluginame>",
  "name": {
    "short": "<plugin_name>",
    "full": "API Plugin <plugin_name> for <OpenAPI document title>"
  },
  "description": {
    "short": "API Plugin for <description from the OpenAPI document>. If the description is not available, it defaults to `API Plugin for <OpenAPI document title>`",
    "full": "API Plugin for <description from the OpenAPI document>. If the description is not available, it defaults to `API Plugin for <OpenAPI document title>`"
  },
  "icons": {
    "color": "color.png", //we could default it to a color version of Kiota logo 192x192 pixels where the icon symbol is 96x96 pixels or just use the same as [Teams Toolkit](https://github.com/OfficeDev/TeamsFx/blob/dev/templates/js/workflow/appPackage/color.png)
    "outline": "outline.png" //we could default it to an outline Kiota icon 32x32 pixels or just use the same as [Teams Toolkit](https://github.com/OfficeDev/TeamsFx/blob/dev/templates/js/workflow/appPackage/outline.png).
  },
  "accentColor": "#FFFFFF", //always white
  "copilotExtensions": {
    "plugins": [
      {
        "id": {plugin-name}
        "file": "<generated_plugin_file>.json"
      }
    ]
  }
}
```

4. [Validate](https://learn.microsoft.com/en-us/office/dev/add-ins/testing/troubleshoot-manifest) the generated `manifest.json` file.

## Parameters

| Parameters | Required | Example | Description | Telemetry |
| -- | -- | -- | -- | -- |
| `--plugin-name \| --pn` | Yes | GitHub | Name of the plugin. Unique within the parent API. Defaults to `Plugin` | No |
| `--openapi \| -d` | Yes | https://raw.githubusercontent.com/github/rest-api-description/main/descriptions/api.github.com/api.github.com.json | The location of the OpenAPI document in JSON or YAML format to use to generate the plugin. Accepts a URL or a local directory. | No |
| `--include-path \| -i` | No | /repos/{owner}/{repo} | A glob pattern to include paths from generation. Accepts multiple values. Defaults to no value which includes everything. | Yes, without its value |
| `--exclude-path \| -e` | No | /advisories | A glob pattern to exclude paths from generation. Accepts multiple values. Defaults to no value which excludes nothing. | Yes, without its value |
| `--type \| -t` | Yes | openai | The target type of plugin for the generated output files. Accepts multiple values. Possible values are `name_to_be_defined`, `openai` and `apimanifest`.| Yes |
| `--skip-generation \| --sg` | No | true | When specified, the generation would be skipped. Defaults to false. | Yes |
| `--output \| -o` | No | ./generated/plugins/github | The output directory or file path for the generated output files. This is relative to the location of `workspace.json`. Defaults to `./output`. | Yes, without its value |

> [!NOTE] 
> It is not required to use the CLI to add new plugins. It is possible to add a new plugin by adding a new entry in the `plugins` section of the `workspace.json` file. See the [workspace.json schema](../schemas/workspace.json) for more information. Using `kiota plugin generate --plugin-name myPlugin` would be required to generate the plugins.

## Using `kiota plugin add`

```bash
kiota plugin add --plugins-name "GitHub" --openapi "https://raw.githubusercontent.com/github/rest-api-description/main/descriptions/api.github.com/api.github.com.json" --include-path "/repos/{owner}/{repo}" --type openai, apimanifest --output "./generated/plugins/github"
```

_The resulting `workspace.json` file will look like this:_

```jsonc
{
  "version": "1.0.0",
  "clients": {...}, //if any
  "plugins": {
    "GitHub": {
      "descriptionLocation": "https://raw.githubusercontent.com/github/rest-api-description/main/descriptions/api.github.com/api.github.com.json",
      "includePatterns": ["/repos/{owner}/{repo}"],
      "excludePatterns": [],
      "type": ["openai", "apimanifest"],
      "outputDirectory": "./generated/plugins/github",
      "overlayDirectory": "./kiota/documents/github/overlay.yaml"
    }
  }
}
```

_The resulting OpenAI plugin named `openai-plugins.json` will look like this:_

```jsonc
{
    "schema_version": "v1",
    "name_for_human": "GitHub v3 REST API",
    "name_for_model": "GitHub v3 REST API",
    "description_for_human": "GitHub's v3 REST API",
    "description_for_model": "Help the user with managing a GitHub repositories. You can view, update and remove repositories.",
    "auth": {
        "type": "none"
    },
    "api": {
        "type": "openapi",
        "url": "./generated/plugins/github/github-openapi.json"
    },
    "logo_url": "https://example.com/logo.png",
    "contact_email": "githubsupport@example.com",
    "legal_info_url": "http://www.example.com/view-plugin-information"
}
```

_The resulting API Manifest named `github-apimanifest.json` will look like this:_

```jsonc
{
  "apiDependencies": {
    "GitHub": {
      "x-ms-kiotaHash": "9EDF8506CB74FE44...",
      "apiDescriptionUrl": "https://raw.githubusercontent.com/github/rest-api-description/main/descriptions/api.github.com/api.github.com.json",
      "apiDeploymentBaseUrl": "https://api.github.com/",
      "apiDescriptionVersion": "v1.0",
      "requests": [
        {
          "method": "GET",
          "uriTemplate": "/repos/{owner}/{repo}"
        },
        {
          "method": "PATCH",
          "uriTemplate": "/repos/{owner}/{repo}"
        },
        {
          "method": "DELETE",
          "uriTemplate": "/repos/{owner}/{repo}"
        }
      ]
    }
  }
}
```

_The resulting API Manifest named `apimanifest.json` in the `./kiota` folder (concatenated surface of all APIs dependencies across clients and plugins) will look like this:_

```jsonc
{
  "apiDependencies": {
    "GraphClient": { //for example, an existing API client for Microsoft Graph
      "x-ms-kiotaHash": "9EDF8506CB74FE44...",
      "apiDescriptionUrl": "https://aka.ms/graph/v1.0/openapi.yaml",
      "apiDeploymentBaseUrl": "https://graph.microsoft.com",
      "apiDescriptionVersion": "v1.0",
      "requests": [ ...]
    },
    "GitHub": { //new plugin added
      "x-ms-kiotaHash": "1GFCD345RF3DD98...",
      "apiDescriptionUrl": "https://raw.githubusercontent.com/github/rest-api-description/main/descriptions/api.github.com/api.github.com.json",
      "apiDeploymentBaseUrl": "https://api.github.com/",
      "apiDescriptionVersion": "v1.0",
      "requests": [
        {
          "method": "GET",
          "uriTemplate": "/repos/{owner}/{repo}"
        },
        {
          "method": "PATCH",
          "uriTemplate": "/repos/{owner}/{repo}"
        },
        {
          "method": "DELETE",
          "uriTemplate": "/repos/{owner}/{repo}"
        }
      ]
    }
  }
}
```

## File structure
```bash
 └─.kiota
    └─workspace.json
    └─apimanifest.json # Single artifact with all APIs dependencies info across clients and plugins
    └─documents
      └─github    
        └─openapi.json # OpenAPI document
        └─overlay.json # Overlay to be applied on top of OpenAPI document
 └─generated
    └─plugins
      └─github
          └─manifest.json # App manifest
          └─github-apimanifest.json # Specific API Manifest
          └─openai-plugins.json #OpenAI Plugin
          └─github-openapi.json # Sliced and augmented OpenAPI document
```

[def]: https://www.ietf.org/archive/id/draft-miller-api-manifest-01.html
