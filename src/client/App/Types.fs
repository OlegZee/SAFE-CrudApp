module App.Types

open ServerProtocol.V1
open CommonTypes

type AppView =
    | NoView
    | ErrorView of string
    | SummaryData of SummaryView.Types.Model
    | DayView of System.DateTime * UserInfo * EntryForm.Types.Model
    | ManageUsers of ManageUsers.Types.Model
    | UserSummaryData of UserInfo * SummaryView.Types.Model
    | ReportView of ReportsForm.Types.Model

type Model = Model of UserInfo * AppView
type Msg =
    | RefreshUserData
    | DisplayMySummary of SummaryData list
    | DisplayError of string
    | DisplayUserSummary of int * UserInfo * SummaryData list
    | SummaryViewMsg of SummaryView.Types.Msg
    | DayViewMsg of EntryForm.Types.Msg
    | ManageUsersMsg of ManageUsers.Types.Msg
    | ReportViewMsg of ReportsForm.Types.Msg