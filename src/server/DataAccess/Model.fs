module DataAccess.Model

type User = {
    user_id: int
    login: string
    name: string

    role: string
    targetCalories: float option
}