# WebSharper Compression Streams API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [Compression Streams API](https://developer.mozilla.org/en-US/docs/Web/API/Compression_Streams_API), allowing WebSharper applications to perform efficient text compression and decompression using Gzip format.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:

   - Contains the F# WebSharper binding for the Compression Streams API.

2. **Sample Project**:
   - Demonstrates how to use the Compression Streams API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/CompressionStreamsAPI/)

## Installation

To use this package in your WebSharper project, add the NuGet package:

```bash
   dotnet add package WebSharper.CompressionStreams
```

## Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/CompressionStreams.git
   cd CompressionStreams
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.CompressionStreams/WebSharper.CompressionStreams.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.CompressionStreams.Sample
   dotnet build
   dotnet run
   ```

4. Open the sample project in your browser to see it in action.

## Example Usage

Below is an example of how to use the Compression Streams API in a WebSharper project:

```fsharp
namespace WebSharper.CompressionStreams.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.CompressionStreams

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let inputText = Var.Create "" // Stores user input for compression
    let compressedData = Var.Create "" // Stores the compressed result
    let decompressedData = Var.Create "" // Stores the decompressed output

    // Encodes an ArrayBuffer to a Base64 string
    let encodeToBase64 (arrayBuffer: ArrayBuffer) =
        let uint8Array = Uint8Array(arrayBuffer) |> As<int array>
        JS.Window.Btoa(String.FromCharCode(uint8Array))

    // Decodes a Base64 string to an ArrayBuffer
    let decodeFromBase64 (base64String: string) =
        let binaryString = JS.Window.Atob(base64String)
        let byteArray = [| for i in 0 .. binaryString.Length - 1 -> binaryString.[i] |> byte |]
        Uint8Array(byteArray).Buffer

    // Function to compress text using the Compression Streams API
    let compressText () =
        promise {
            let encoder = JS.Eval("new TextEncoder()")
            let data = encoder?encode(inputText.Value) |> As<ArrayBuffer>

            let blob = new Blob([|data|])
            let compressedStream = blob.Stream().PipeThrough(new CompressionStream(CompressionFormat.Gzip) |> As<ReadableWritablePair>)

            let response = new Response(compressedStream)
            let! compressedArrayBuffer = response.ArrayBuffer()

            compressedData.Value <- encodeToBase64(compressedArrayBuffer)
        }

    // Function to decompress text using the Compression Streams API
    let decompressText () =
        promise {
            if compressedData.Value = "" then return ()

            let compressedArrayBuffer = decodeFromBase64(compressedData.Value)

            let blob = new Blob([|compressedArrayBuffer|])
            let decompressedStream = blob.Stream().PipeThrough(new DecompressionStream(CompressionFormat.Gzip) |> As<ReadableWritablePair>)

            let response = new Response(decompressedStream)
            let! decompressedArrayBuffer = response.ArrayBuffer()

            let decoder = JS.Eval("new TextDecoder()")
            let decompressedText = decoder?decode(decompressedArrayBuffer)

            decompressedData.Value <- decompressedText
        }

    [<SPAEntryPoint>]
    let Main () =
        IndexTemplate.Main()
            .InputText(inputText.V)
            .CompressedData(compressedData.V)
            .DecompressedData(decompressedData.V)
            .CompressText(fun _ ->
                async {
                    do! compressText() |> Promise.AsAsync
                } |> Async.StartImmediate
            )
            .DecompressText(fun _ ->
                async {
                    do! decompressText() |> Promise.AsAsync
                } |> Async.StartImmediate
            )
            .Doc()
        |> Doc.RunById "main"
```

This example demonstrates how to compress and decompress text using the Web Compression Streams API in WebSharper.

## Important Considerations

- **Limited Browser Support**: Some browsers may not support the Compression Streams API; check [MDN Compression Streams API](https://developer.mozilla.org/en-US/docs/Web/API/Compression_Streams_API) for the latest compatibility details.
- **Use Cases**: This API is useful for reducing data transfer sizes in web applications and efficiently handling large text data.
