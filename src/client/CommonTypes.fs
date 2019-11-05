module CommonTypes

open Fable.Core

[<Erase>]
type UserId = UserId of int

// data entry id
[<Erase>]
type EntryId = EntryId of int

type UserInfo = { userName: string; userRole: string; target: float }
