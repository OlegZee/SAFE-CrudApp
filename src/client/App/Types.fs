module App.Types
open ServerProtocol.V1

type UserInfo = { token: ServerComm.Token; userName: string; userRole: string; target: float }

type SummaryViewModel = {
    otherUser: int option   // this indicates the summary view displays other user (in admin mode)
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
    | DayView of System.DateTime * UserInfo * EntryForm.Types.Model
    | ManageUsers of ManageUsers.Types.Model
    | UserSummaryData of UserInfo * SummaryViewModel

type Model = Model of UserInfo * AppView
type Msg =
    | RefreshUserData
    | DisplayMySummary of SummaryData list
    | DisplayError of string
    | DisplayUserSummary of int * UserInfo * SummaryData list
    | SummaryViewMsg of SummaryViewMsg
    | DayViewMsg of EntryForm.Types.Msg
    | ManageUsersMsg of ManageUsers.Types.Msg