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
    rtime: string
    meal: string
    amount: float
}

type CreateUserData = {
    rtime: string
    meal: string
    amount: float
}

type UpdUserData = {
    rtime: string
    meal: string
    amount: float
}

type UserRecordCreated = {
    record_id: int
}


// Auth related payloads

type LoginData = {
    login: string
    pwd: string
}

type ChPassPayload = {
    oldpwd: string
    newpwd: string
}

type LoginResult = {
    token: string
}

type WhoResult = {
    role: string
    login: string
    name: string
}

// Settings

type UserSettings = {
    targetCalories: float
}

type CreateUserInfo = {
    login: string
    pwd: string
    name: string

    role: string
    targetCalories: float
}

type UserCreatedResponse = {
    user_id: int
}
