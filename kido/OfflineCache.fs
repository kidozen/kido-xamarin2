namespace Kidozen

open System

open System.Collections.Generic
open Newtonsoft
open Newtonsoft.Json

module Offline =
    type OfflineCacheEnum = NoCache=0 | NetworkOnly=1 | LocalElseNetwork=2 | NetworkElseLocal=3    

    let save path data mode =
        match mode with
            | OfflineCacheEnum.NoCache -> "0"
            | OfflineCacheEnum.NetworkOnly -> "1"
            | OfflineCacheEnum.LocalElseNetwork -> "2"
            | OfflineCacheEnum.NetworkElseLocal -> "3"
            | _ -> ""

    let get path data mode =
        match mode with
            | OfflineCacheEnum.NoCache -> "0"
            | OfflineCacheEnum.NetworkOnly -> "1"
            | OfflineCacheEnum.LocalElseNetwork -> "2"
            | OfflineCacheEnum.NetworkElseLocal -> "3"
            | _ -> ""
