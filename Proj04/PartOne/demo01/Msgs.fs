module Msgs

type RegisterInfo = 
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type RegisterSuccessInfo =
    struct
    end

type RegisterFailureInfo =
    struct
    end

type RegisterOperationInfo =
    struct
    end

type LoginOperation =
    struct
    end

type LoginInfo = 
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type LogoutOperation =
    struct
    end

type LogoutInfo =
    struct
        val NAME: string
        new (name: string) = {
            NAME = name;
        }
    end

type SubscribeOperation =
    struct
    end

type SubscribeInfo = 
    struct
        val NAME: string
        val FOLLOWER: string
        new (name: string, follower: string) = {
            NAME = name;
            FOLLOWER = follower;
        }
    end