module App.Types
open ServerProtocol.V1

type UserInfo = { token: string; userName: string; userRole: string; target: float }

type AppView =
    | NoView
    | OverviewMode of string
    | SummaryData of SummaryData list
    | DayView of System.DateTime

type Model = Model of UserInfo * AppView
type Msg =
    | RefreshUserData
    | ReceivedUserSummary of Result<SummaryData list,string>