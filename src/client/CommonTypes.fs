module CommonTypes

open Fable.Core

[<Erase>]
type Token = Token of string

type UserInfo = { token: Token; userName: string; userRole: string; target: float }
