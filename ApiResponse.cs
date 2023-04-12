namespace MapMate; 

public class ApiResponse {

    public class PutResponse {
        public string Key = string.Empty;
    }

    public class GetResponse {
        public string ID = string.Empty;
        public uint Row = 0;
        public uint SubRow = 0;
    }
    
}
