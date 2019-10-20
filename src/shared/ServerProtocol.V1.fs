module ServerProtocol.V1

type User = {
    user_id: int
    login: string
    name: string

    role: string
    targetCalories: float
}

type SummaryData = {
    rdate: System.DateTime
    count: int
    amount: float
}

type UserData = {
    record_id: int
    rdate: System.DateTime   // TODO remove
    rtime: string
    meal: string
    amount: float
}
