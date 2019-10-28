module ReportsForm.Types

open System
open ServerProtocol.V1

type QueryModel = {
    dateStart: DateTime option
    dateEnd: DateTime option

    timeStart: TimeSpan option
    timeEnd: TimeSpan option
}

type Model = {
    token: ServerComm.Token
    data: SummaryData list
    lastError: string option
}

type Msg =
    | QueryData of QueryModel
    | UpdateData of SummaryData list
    | DisplayError of string option
