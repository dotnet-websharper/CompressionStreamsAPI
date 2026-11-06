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
    
    let inputText = Var.Create ""
    let compressedData = Var.Create ""
    let decompressedData = Var.Create ""

    let encodeToBase64 (arrayBuffer: ArrayBuffer) =
        let uint8Array = Uint8Array(arrayBuffer) |> As<int array>
        JS.Window.Btoa(String.FromCharCode(uint8Array))

    let decodeFromBase64 (base64String: string) =
        let binaryString = JS.Window.Atob(base64String)
        let byteArray = [| for i in 0 .. binaryString.Length - 1 -> binaryString.[i] |> byte |]
        Uint8Array(byteArray).Buffer

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