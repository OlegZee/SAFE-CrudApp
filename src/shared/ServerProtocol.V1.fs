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

type PostDataPayload = {
    rtime: string
    meal: string
    amount: float
}

type PostDataResponse = {
    record_id: int
}


// Auth related payloads

type LoginPayload = {
    login: string
    pwd: string
}

type ChPassPayload = {
    oldpwd: string
    newpwd: string
}

type WhoResult = {
    role: string
    login: string
    name: string
}

type SignupPayload = {
    login: string
    name: string
    pwd: string
}

// Settings

type UserSettings = {
    targetCalories: float
}

type CreateUserPayload = {
    login: string
    pwd: string
    name: string

    role: string
    targetCalories: float
}

type CreateUserResponse = {
    user_id: int
}
