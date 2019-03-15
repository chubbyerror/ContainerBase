struct tRequestMsg{
    1:string ActionKey;
    2:string RequestObject;
}

struct tReponseMsg{
    1:i32 ErrorCode;
    2:string ErrorInfo;
    3:string Result;
}

service tConnect{
    tReponseMsg GetReponse(1:tRequestMsg Msg);
}