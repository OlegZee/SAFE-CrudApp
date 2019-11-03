module ManageUsers.Types

open Components
open CommonTypes
open ServerProtocol.V1

type Model = TabularForms.Model<User, CreateUserPayload, Token>
type Msg = TabularForms.Msg<User,UserId>
