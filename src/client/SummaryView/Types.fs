module SummaryView.Types

open ServerProtocol.V1
open CommonTypes

type Model = {
    otherUser: UserId option   // this indicates the summary view displays other user (in admin mode)
    user: UserInfo
    data: SummaryData list
    editedTarget: float option
}

type Msg =
    | EditTarget
    | SaveValue of string
    | CancelEdit
    | SavedTargetValue of float