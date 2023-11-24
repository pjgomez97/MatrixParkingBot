module ParkingBot.HttpUtils

open System.Net
open System.Text.Json
open FSharp.Data
open Logger

let cc = CookieContainer()

let validateResponse (response: HttpResponse) =
    if response.StatusCode = 401 then
        logError "Error. You must be logged in"
        exit -1

let getResponseBodyAsString response =
    match response.Body with
    | Text t -> t
    | _ ->
        logError "Unknown response format"
        exit -1
        
let objectToJson object = TextRequest <| JsonSerializer.Serialize object

let get url = Http.Request(url, silentHttpErrors = true, cookieContainer = cc)
    
let post url body =
    Http.Request(url, body = body, headers= [ "Content-Type", "application/json" ], silentHttpErrors = true, cookieContainer = cc)