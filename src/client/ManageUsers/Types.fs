module ManageUsers.Types

open Components
open CommonTypes

type UserData = {
    login: string
    name: string
    role: string
    expenseLimit: float
    pwd: string
}

type Model = TabularForms.Model<UserId, UserData, unit>
type Msg = TabularForms.Msg<UserId, UserData>
