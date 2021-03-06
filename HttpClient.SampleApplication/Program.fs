﻿// Some examples of the HttpClient module in action
module Program

open HttpClient
open HttpClient.SampleApplication
open System.IO
open System

// Use our PageDownloader to count the instances of a word on the bbc news site
// We pass the getResponseBody function in as a dependency to PageDownloader so
// it can be unit tested
let countWords () =
    let downloader = new PageDownloader( HttpClient.getResponseBody )
    printfn "What word would you like to count on bbc.co.uk/news?"
    let word = Console.ReadLine();

    let count = downloader.countWordInstances word "http://www.bbc.co.uk/news/"
    printfn "'%s' was found %d times on bbc.co.uk/news" word count

// Download some sites sequentially, using the synchronous version of getResponseCode
let downloadSequentially sites =
    let timer = System.Diagnostics.Stopwatch.StartNew()
    sites
    |> List.map (fun url -> createRequest Get url |> getResponseCode)
    |> ignore
    printfn "Pages downloaded sequentially in %d ms" timer.ElapsedMilliseconds

// Download some sites in parallel, using the asynchronous version of getResponseCode
let downloadInParallel sites =
    let timer = System.Diagnostics.Stopwatch.StartNew()
    sites
    |> List.map (fun url -> createRequest Get url |> getResponseCodeAsync)
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
    printfn "Pages downloaded in parallel in %d ms" timer.ElapsedMilliseconds

let returnToContinue message =
    printfn "\n%s" message
    Console.ReadLine() |> ignore

// create a more coplex request, and see the request & response
// (this should get a 302)
let complexRequest() =
    let request =
        createRequest Get "http://www.google.com/search"
        |> withQueryStringItem {name="q"; value="gibbons"}
        |> withAutoDecompression DecompressionScheme.GZip
        |> withAutoFollowRedirectsDisabled
        |> withCookie {name="ignoreMe"; value="hi mum"}
        |> withHeader (Accept "text/html")
        |> withHeader (UserAgent "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.57 Safari/537.36")

    returnToContinue "Press Return to see the request"
    printfn "%A" request

    printfn "\nRetrieving response..."
    let response = request |> getResponse

    returnToContinue "Press Return to see the response"
    printfn "%A" response

let downloadImage() =
    let response = createRequest Get "http://fsharp.org/img/logo.png" |> getResponseBytes

    printfn "Please enter path to save the image, e.g. c:/temp (file will be testImage.png)"
    let filename = Console.ReadLine() + "/testImage.png"

    use file = File.Create(filename)
    file.Write(response, 0, response.Length)

    printfn "'%s' written to disk" filename

// Download some sites sequentially, using the synchronous version of getResponseCode
let downloadImagesSequentially images =
    let timer = System.Diagnostics.Stopwatch.StartNew()
    images
    |> List.map (fun url -> createRequest Get url |> getResponseBytes)
    |> ignore
    printfn "Images downloaded sequentially in %d ms" timer.ElapsedMilliseconds

// Download some sites in parallel, using the asynchronous version of getResponseCode
let downloadImagesInParallel images =
    let timer = System.Diagnostics.Stopwatch.StartNew()
    images
    |> List.map (fun url -> createRequest Get url |> getResponseBytesAsync)
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
    printfn "Images downloaded in parallel in %d ms" timer.ElapsedMilliseconds

[<EntryPoint>]
let Main(_) = 

    countWords()

    printfn "\nDownloading sites: Sequential vs Parallel..."

    let sites = [
        "http://news.bbc.co.uk"
        "http://www.facebook.com"
        "http://www.wikipedia.com"
        "http://www.stackoverflow.com"]

    sites |> downloadSequentially
    sites |> downloadInParallel

    printfn "\nCreating a complex request.."
    complexRequest()

    printfn "\nDownloading image..."
    downloadImage()

    printfn "\nDownloading images: Sequential vs Parallel..."

    let images = [
        "http://fsharp.org/img/sup/quantalea.png"
        "http://fsharp.org/img/sup/mbrace.png"
        "http://fsharp.org/img/sup/statfactory.jpg"
        "http://fsharp.org/img/sup/tsunami.png"]

    images |> downloadImagesSequentially
    images |> downloadImagesInParallel

    returnToContinue "Press Return to exit"
    0