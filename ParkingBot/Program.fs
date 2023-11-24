open Logger
open ParkingBot.MatrixAPI

[<EntryPoint>]
let main _argv =
    let username = System.Environment.GetEnvironmentVariable("MATRIX_USERNAME")
    let password = System.Environment.GetEnvironmentVariable("MATRIX_PASSWORD")

    if username = null || password = null then
        logError "Error. You must set MATRIX_USER and MATRIX_PASSWORD environment variables"
        logError "Exiting..."
        exit -1

    let date = "2023-10-20"

    login username password

    let location = MalParking

    let locationId = getLocationId (getLocations ()) location

    logInfo $"Location ID: %d{locationId}"

    let spots = getSpots locationId date

    logInfo $"Available spots for %s{date} are:"

    Array.iter (fun s -> logInfo s.name) spots

    //bookRandomSpot spots date
    
    0