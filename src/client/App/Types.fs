module App.Types
open ServerProtocol.V1

type UserInfo = { token: ServerComm.Token; userName: string; userRole: string; target: float }

type SummaryViewModel = {
    user: UserInfo
    data: SummaryData list
    editedTarget: float option
}

type SummaryViewMsg =
    | EditTarget
    | SaveValue of string
    | CancelEdit
    | SavedTargetValue of float

type AppView =
    | NoView
    | ErrorView of string
    | SummaryData of SummaryViewModel
    | DayView of EntryForm.Types.Model

type Model = Model of UserInfo * AppView
type Msg =
    | RefreshUserData
    | ReceivedUserSummary of Result<SummaryData list,string>
    | SummaryViewMsg of SummaryViewMsg
    | DayViewMsg of EntryForm.Types.Msg