module Logger

open FSLogger

let log = Logger.ColorConsole

let logDebug msg =
    log.D(" " + msg)

let logInfo msg =
    log.I(" " + msg)
    
let logWarning msg =
    log.W(" " + msg)

let logError msg =
    log.E(" " + msg)