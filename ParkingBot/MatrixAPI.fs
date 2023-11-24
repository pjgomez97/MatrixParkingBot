module ParkingBot.MatrixAPI

open System.Net
open FSharp.Data
open HttpUtils
open Logger

type User = {
    id: int
    name: string
}

type Locations =
    | MalOffice
    | MalParking

type Location = {
    id: int
    kind: string
    name: string
}

let cc = CookieContainer()

let mutable user : Option<User> = None

let login username password =
    logInfo $"Trying to log in with user %s{username}"
    
    let credentials = {| username = username; password = password |}

    let response = post "https://app.matrixbooking.com/api/v1/user/login" (objectToJson credentials)

    let body = getResponseBodyAsString response
    
    logDebug $"%s{body}"

    match response.StatusCode with
    | 200 ->
        logInfo "Successfully logged in"
    | _ ->
        logError "Error during login. Review the introduced credentials and if the site is up"
        exit -1

    user <- Some { id = JsonValue.Parse(body).GetProperty("personId").AsInteger(); name = username }
    
let getLocations () : Location array =
    logInfo "Fetching locations"

    let response = get "https://app.matrixbooking.com/api/v1/org/141510196"
        
    validateResponse response
    
    let body = getResponseBodyAsString response

    JsonValue.Parse(body).GetProperty("locations").AsArray()
    |> Array.map (fun l -> { id = l["id"].AsInteger(); kind  = l["kind"].AsString(); name = l["name"].AsString() })

let getLocationId locations location =
    let locationText =
        match location with
        | MalOffice -> "MAL Office"
        | MalParking -> "MAL Parking"

    logInfo $"Getting location id for %s{locationText}"

    Array.find (fun (l: Location) -> l.name.Contains locationText) locations
    |> fun l -> l.id

let getSpots locationId date =
    logInfo $"Getting available spots for locationId %d{locationId} and date %s{date}"

    let response = get $"https://app.matrixbooking.com/api/v1/availability?bc=158240906&f=%s{date}T00:00&include=locations&l=%d{locationId}&status=available&t=%s{date}T23:59"

    validateResponse response

    let body = getResponseBodyAsString response

    let availableIds =
        JsonValue.Parse(body).GetProperty("availability").AsArray()
        |> Array.map (fun l -> l["locationId"].AsInteger())
    
    JsonValue.Parse(body).GetProperty("locations").AsArray()
    |> Array.map (fun l -> { id = l["id"].AsInteger(); kind  = l["kind"].AsString(); name = l["qualifiedName"].AsString() })
    |> Array.filter (fun l -> l.name.Contains("MAL Office Parking") && Array.contains l.id availableIds)

let book (user: User) locationId date =
    let bookingRequest = {|
                            timeFrom = $"%s{date}T08:00:00.000"
                            timeTo = $"%s{date}T19:00:00.000"
                            locationId = locationId
                            attendees = [||]
                            extraRequests = [||]
                            bookingGroup = {| repeatEndDate = date |}
                            owner = {| id = user.id ; email = user.name |}
                            ownerIsAttendee = true
                            source = "WEB"
                         |}

    let response = post "https://app.matrixbooking.com/api/v1/booking" (objectToJson bookingRequest)

    validateResponse response

let bookRandomSpot spots date =
    let mutable spot =
        spots
        |> Array.tryFind (fun s -> s.name.Contains("Building"))

    if Option.isNone spot then
        spot <- Some(Array.head spots)
        
    logInfo $"Booking spot %s{spot.Value.name}"

    book user.Value spot.Value.id date
