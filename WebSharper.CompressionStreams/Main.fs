namespace WebSharper.CompressionStreams

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =
    
    module Enum =
        let CompressionFormat =
            Pattern.EnumStrings "CompressionFormat" [
                "gzip"
                "deflate"
                "deflate-raw"
            ]

    let CompressionStream =
        Class "CompressionStream"
        |+> Static [
            Constructor Enum.CompressionFormat?format 
        ]
        |+> Instance [
            "readable" =? T<ReadableStream> 
            "writable" =? T<WritableStream> 
        ]

    let DecompressionStream =
        Class "DecompressionStream"
        |+> Static [
            Constructor Enum.CompressionFormat?format 

        ]
        |+> Instance [
            "readable" =? T<ReadableStream> 
            "writable" =? T<WritableStream> 

        ]    

    let Assembly =
        Assembly [
            Namespace "WebSharper.CompressionStreams" [
                Enum.CompressionFormat                
                CompressionStream
                DecompressionStream
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()