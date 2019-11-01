module CommonTypes

open Fable.Core

[<Erase>]
type Token = Token of string

[<Erase>]
type UserId = UserId of int

type UserInfo = { token: Token; userName: string; userRole: string; target: float }
